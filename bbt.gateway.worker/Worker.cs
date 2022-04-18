
using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Refit;
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
                    o.TrackingStatus == SmsTrackingStatus.Pending
                    && o.TrackingLogs.Count <= 5)
                    .ToList();
                otpResponseLogs.ForEach(async (e) =>
                {
                    try
                    {
                        var response = await _messagingGatewayApi.CheckMessageStatus(new CheckSmsRequest
                        {
                            Operator = e.Operator,
                            OtpRequestLogId = e.Id,
                            StatusQueryId = e.StatusQueryId
                        });

                        _repositoryManager.OtpTrackingLog.Add(response);
                        if (response.Status != SmsTrackingStatus.Pending)
                        {
                            e.TrackingStatus = response.Status;
                            _repositoryManager.OtpResponseLogs.Update(e);
                        }
                        _repositoryManager.SaveChanges();
                    }
                    catch (ApiException ex)
                    {
                        _logger.LogCritical($"Messaging Gateway Api Error | Status Code : {ex.StatusCode}");
                    }
                    
                    
                });
                
                await Task.Delay(3600, stoppingToken);
            }
        }
    }
}