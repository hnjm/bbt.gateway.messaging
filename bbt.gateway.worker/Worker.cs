
using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace bbt.gateway.worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMessagingGatewayApi _messagingGatewayApi;
        public Worker(ILogger<Worker> logger, IRepositoryManager repositoryManager,
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
                var otpResponseLogs = _repositoryManager.OtpResponseLogs.GetOtpResponseLogsWithTrackingLog(
                    o => o.ResponseCode == SendSmsResponseStatus.Success &&
                    o.TrackingStatus == SmsTrackingStatus.Pending)
                    .ToList();
                otpResponseLogs.ForEach(async (e) =>
                {
                    _logger.LogInformation($"Tracking log count : {e.TrackingLogs.Count}");
                    if (e.TrackingLogs.Count <= 5)
                    {
                        var response = await _messagingGatewayApi.CheckMessageStatus(new CheckSmsRequest
                        {
                            Operator = e.Operator,
                            OtpRequestLogId = e.Id,
                            StatusQueryId = e.StatusQueryId
                        });
                        _logger.LogInformation($"Response Delivery Status : {response.Status}");
                        _repositoryManager.OtpTrackingLog.Add(response);
                        if (response.Status != SmsTrackingStatus.Pending)
                        {
                            e.TrackingStatus = response.Status;
                            _repositoryManager.OtpResponseLogs.Update(e);
                        }

                    }
                });
                _repositoryManager.SaveChanges();
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(3600, stoppingToken);
            }
        }
    }
}