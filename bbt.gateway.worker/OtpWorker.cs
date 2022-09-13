
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
    public class OtpWorker : BackgroundService
    {
        private readonly ILogger<OtpWorker> _logger;
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMessagingGatewayApi _messagingGatewayApi;
        public OtpWorker(ILogger<OtpWorker> logger, IRepositoryManager repositoryManager,
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
                var otpResponseLogs = (await _repositoryManager.OtpResponseLogs.GetOtpResponseLogsAsync(
                    o => o.ResponseCode == SendSmsResponseStatus.Success &&
                    o.TrackingStatus == SmsTrackingStatus.Pending
                    && o.CreatedAt < DateTime.Now.AddMinutes(-5)
                    )).ToList();

                var taskList = new List<Task>();
                ConcurrentBag<OtpEntitiesToBeProcessed> concurrentBag = new();
                otpResponseLogs.ForEach(otpResponseLog =>
                {
                    taskList.Add(GetDeliveryStatus(otpResponseLog, concurrentBag));
                });

                await Task.WhenAll(taskList);

                foreach (var entities in concurrentBag)
                {
                    if (entities.otpTrackingLog != null)
                    {
                        await _repositoryManager.OtpTrackingLog.AddAsync(entities.otpTrackingLog);
                    }
                    if (entities.otpResponseLog != null)
                    {
                        _repositoryManager.OtpResponseLogs.Update(entities.otpResponseLog);
                    }
                }
                await _repositoryManager.SaveChangesAsync();

            }
        }

        private async Task GetDeliveryStatus(OtpResponseLog otpResponseLog,ConcurrentBag<OtpEntitiesToBeProcessed> concurrentBag)
        {
            try
            {
                OtpEntitiesToBeProcessed entitiesToBeProcessed = new();
                var response = await _messagingGatewayApi.CheckOtpStatus(new CheckSmsRequest
                {
                    Operator = otpResponseLog.Operator,
                    OtpRequestLogId = otpResponseLog.Id,
                    StatusQueryId = otpResponseLog.StatusQueryId
                });

                entitiesToBeProcessed.otpTrackingLog = response;

                if (response.Status != SmsTrackingStatus.Pending)
                {
                    otpResponseLog.TrackingStatus = response.Status;
                    entitiesToBeProcessed.otpResponseLog = otpResponseLog;
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

    public class OtpEntitiesToBeProcessed
    {
        public OtpResponseLog otpResponseLog { get; set; }
        public OtpTrackingLog otpTrackingLog { get; set; }
    }

}