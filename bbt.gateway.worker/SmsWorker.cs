
using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Refit;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace bbt.gateway.worker
{
    public class SmsWorker : BackgroundService
    {
        private readonly ILogger<OtpWorker> _logger;
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMessagingGatewayApi _messagingGatewayApi;
        public SmsWorker(ILogger<OtpWorker> logger, IRepositoryManager repositoryManager,
            IMessagingGatewayApi messagingGatewayApi)
        {
            _logger = logger;
            _repositoryManager = repositoryManager;
            _messagingGatewayApi = messagingGatewayApi;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var smsResponseLogs = (await _repositoryManager.SmsResponseLogs.GetSmsResponseLogsAsync(
                    o => o.OperatorResponseCode == 0 &&
                    (o.Status == null || String.IsNullOrWhiteSpace(o.Status))
                    && o.CreatedAt < DateTime.Now.AddMinutes(-10)
                    )).ToList();
                var time = (DateTime.Now - now).TotalSeconds;
                var taskList = new List<Task>();
                ConcurrentBag<SmsEntitiesToBeProcessed> concurrentBag = new();
                smsResponseLogs.ForEach(smsResponseLog =>
                {
                    taskList.Add(GetDeliveryStatus(smsResponseLog, concurrentBag));
                });

                await Task.WhenAll(taskList);

                foreach (var entities in concurrentBag)
                {
                    if (entities.smsTrackingLog != null)
                    {
                        await _repositoryManager.SmsTrackingLogs.AddAsync(entities.smsTrackingLog);
                    }
                    if (entities.smsResponseLog != null)
                    {
                        _repositoryManager.SmsResponseLogs.Update(entities.smsResponseLog);
                    }
                }
                await _repositoryManager.SaveChangesAsync();

            }
        }

        private async Task GetDeliveryStatus(SmsResponseLog smsResponseLog,ConcurrentBag<SmsEntitiesToBeProcessed> concurrentBag)
        {
            try
            {
                SmsEntitiesToBeProcessed entitiesToBeProcessed = new();
                var response = await _messagingGatewayApi.CheckSmsStatus(new common.Models.v2.CheckFastSmsRequest
                {
                    Operator = smsResponseLog.Operator,
                    SmsRequestLogId = smsResponseLog.Id,
                    StatusQueryId = smsResponseLog.StatusQueryId
                });

                entitiesToBeProcessed.smsTrackingLog = response;

                if (response.Status != SmsTrackingStatus.Pending)
                {
                    smsResponseLog.Status = response.Status.ToString();
                    entitiesToBeProcessed.smsResponseLog = smsResponseLog;
                }

                concurrentBag.Add(entitiesToBeProcessed);
            }
            catch (ApiException ex)
            {
                _logger.LogCritical($"Messaging Gateway Api Error | Status Code : {ex.StatusCode}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Messaging Gateway Worker Error | Error : {ex.Message}");
            }
        }
    }

    public class SmsEntitiesToBeProcessed
    {
        public SmsResponseLog smsResponseLog { get; set; }
        public SmsTrackingLog smsTrackingLog { get; set; }
    }

}