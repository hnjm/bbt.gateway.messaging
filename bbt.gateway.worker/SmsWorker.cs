
using bbt.gateway.common;
using bbt.gateway.common.Models;
using bbt.gateway.common.Repositories;
using Elastic.Apm.Api;
using Microsoft.EntityFrameworkCore;
using Refit;
using System.Collections.Concurrent;

namespace bbt.gateway.worker
{
    public class SmsWorker : BackgroundService
    {
        private readonly IMessagingGatewayApi _messagingGatewayApi;
        private readonly ITracer _tracer;
        private readonly LogManager _logManager;
        private readonly DatabaseContext _dbContext;
        public SmsWorker(LogManager logManager,ITracer tracer,
            IMessagingGatewayApi messagingGatewayApi,DbContextOptions<DatabaseContext> dbContextOptions)
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
                _logManager.LogInformation("Sms Tracking Triggered");
                try
                {
                    await _tracer.CaptureTransaction("Sms Tracking", ApiConstants.TypeRequest, async () =>
                    {
                        try
                        {
                            var smsResponseLogsAsc = await _dbContext.SmsResponseLog.Include(s => s.TrackingLogs).Where(
                                    o => o.OperatorResponseCode == 0 &&
                                    o.Operator != 0 &&
                                    (o.Status == null || String.IsNullOrWhiteSpace(o.Status))
                                    && o.CreatedAt < DateTime.Now.AddMinutes(-10)
                                    && o.TrackingLogs.Count <= 10
                                )
                                .Take(5)
                                .OrderBy(s => s.CreatedAt)
                                .ToListAsync();

                            var smsResponseLogsDesc = await _dbContext.SmsResponseLog.Include(s => s.TrackingLogs).Where(
                                        o => o.OperatorResponseCode == 0 &&
                                        o.Operator != 0 &&
                                        (o.Status == null || String.IsNullOrWhiteSpace(o.Status))
                                        && o.CreatedAt < DateTime.Now.AddMinutes(-10)
                                        && o.TrackingLogs.Count <= 10
                                    )
                                    .Take(5)
                                    .OrderByDescending(s => s.CreatedAt)
                                    .ToListAsync();

                            var smsResponseLogs = smsResponseLogsAsc.Concat(smsResponseLogsDesc).Distinct().ToList();
                            _logManager.LogInformation("Sms Count : " + smsResponseLogs.Count);

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
                                    await _dbContext.SmsTrackingLog.AddAsync(entities.smsTrackingLog);
                                }
                                if (entities.smsResponseLog != null)
                                {
                                    _dbContext.SmsResponseLog.Update(entities.smsResponseLog);
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

        private async Task GetDeliveryStatus(SmsResponseLog smsResponseLog, ConcurrentBag<SmsEntitiesToBeProcessed> concurrentBag)
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
                _logManager.LogError($"Messaging Gateway Api Error | Status Code : {ex.StatusCode} | Detail : Operator => {smsResponseLog.Operator}, SmsResponseLogId => {smsResponseLog.Id}, StatusQueryId => {smsResponseLog.StatusQueryId}");
            }
            catch (Exception ex)
            {
                _logManager.LogError($"Messaging Gateway Worker Error | Error : {ex.Message}");
            }
        }
    }

    public class SmsEntitiesToBeProcessed
    {
        public SmsResponseLog smsResponseLog { get; set; }
        public SmsTrackingLog smsTrackingLog { get; set; }
    }

}