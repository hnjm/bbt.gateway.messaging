
using bbt.gateway.common;
using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using Elastic.Apm.Api;
using Microsoft.EntityFrameworkCore;
using Refit;
using System.Collections.Concurrent;

namespace bbt.gateway.worker
{
    public class OtpWorker : BackgroundService
    {
        private readonly LogManager _logManager;
        private readonly IMessagingGatewayApi _messagingGatewayApi;
        private readonly ITracer _tracer;
        private readonly DatabaseContext _dbContext;

        public OtpWorker(LogManager logManager,ITracer tracer,
            IMessagingGatewayApi messagingGatewayApi, DbContextOptions<DatabaseContext> dbContextOptions)
        {
            _logManager = logManager;
            _tracer = tracer;
            _messagingGatewayApi = messagingGatewayApi;
            _dbContext = new DatabaseContext(dbContextOptions);
        }
        

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logManager.LogInformation("Otp Tracking Triggered");
                try
                {
                    await _tracer.CaptureTransaction("Otp Tracking", ApiConstants.TypeRequest, async () =>
                    {
                        try
                        {
                            var otpResponseLogsAsc = await _dbContext.OtpResponseLog.Include(o => o.TrackingLogs).Where(
                                o => o.ResponseCode == SendSmsResponseStatus.Success &&
                                o.TrackingStatus == SmsTrackingStatus.Pending
                                && o.CreatedAt < DateTime.Now.AddMinutes(-5)
                                && o.TrackingLogs.Count <= 10
                            )
                            .OrderBy(o => o.CreatedAt)
                            .Take(5)
                            .ToListAsync();

                            var otpResponseLogsDesc = await _dbContext.OtpResponseLog.Include(o => o.TrackingLogs).Where(
                                    o => o.ResponseCode == SendSmsResponseStatus.Success &&
                                    o.TrackingStatus == SmsTrackingStatus.Pending
                                    && o.CreatedAt < DateTime.Now.AddMinutes(-5)
                                    && o.TrackingLogs.Count <= 10
                                )
                                .OrderByDescending(o => o.CreatedAt)
                                .Take(5)
                                .ToListAsync();

                            var otpResponseLogs = otpResponseLogsAsc.Concat(otpResponseLogsDesc).Distinct().ToList();
                            _logManager.LogInformation("Otp Count : "+otpResponseLogs.Count);

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
                                    await _dbContext.OtpTrackingLog.AddAsync(entities.otpTrackingLog);
                                }
                                if (entities.otpResponseLog != null)
                                {
                                    _dbContext.OtpResponseLog.Update(entities.otpResponseLog);
                                }
                            }
                            await _dbContext.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            await _dbContext.DisposeAsync();
                            _logManager.LogError(ex.ToString());
                            _tracer.CaptureException(ex);
                            await StopAsync(new CancellationToken(true));
                            
                        }

                        
                    });
                }
                catch (Exception ex)
                {
                    _logManager.LogError(ex.ToString());
                    await _dbContext.DisposeAsync();
                    await StopAsync(new CancellationToken(true));
                }
                await Task.Delay(1000 * 60 * 1, stoppingToken);
            }
            
        }

        private async Task GetDeliveryStatus(OtpResponseLog otpResponseLog, ConcurrentBag<OtpEntitiesToBeProcessed> concurrentBag)
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
                _logManager.LogError($"Messaging Gateway Api Error | Status Code : {ex.StatusCode} | Detail : Operator => {otpResponseLog.Operator}, OtpResponseLogId => {otpResponseLog.Id}, StatusQueryId => {otpResponseLog.StatusQueryId}");
            }
            catch (Exception ex)
            {
                _logManager.LogError($"Messaging Gateway Worker Error | Error : {ex.Message}");
            }
        }

    }

    public class OtpEntitiesToBeProcessed
    {
        public OtpResponseLog otpResponseLog { get; set; }
        public OtpTrackingLog otpTrackingLog { get; set; }
    }

}