
using bbt.gateway.common;
using bbt.gateway.common.Api.MessagingGateway;
using bbt.gateway.common.Extensions;
using bbt.gateway.common.Helpers;
using bbt.gateway.common.Models;
using Elastic.Apm.Api;
using Microsoft.EntityFrameworkCore;
using Refit;
using System.Collections.Concurrent;

namespace bbt.gateway.worker.SmsReports
{
    public class SmsWorker : BackgroundService
    {
        private readonly IMessagingGatewayApi _messagingGatewayApi;
        private readonly ITracer _tracer;
        private readonly LogManager _logManager;
        private readonly DatabaseContext _dbContext;
        public SmsWorker(LogManager logManager, ITracer tracer,
            IMessagingGatewayApi messagingGatewayApi, DbContextOptions<DatabaseContext> dbContextOptions)
        {
            _logManager = logManager;
            _tracer = tracer;
            _messagingGatewayApi = messagingGatewayApi;
            _dbContext = new DatabaseContext(dbContextOptions);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logManager.LogInformation("Sms Tracking Triggered");
            try
            {
                await _tracer.CaptureTransaction("Sms Tracking", ApiConstants.TypeRequest, async () =>
                {
                    try
                    {
                        var endDate = DateTime.Now.AddHours(-6);
                        var startDate = endDate.AddHours(-8);
                        var smsResponseLogs = await _dbContext.SmsResponseLog.
                        FromSqlRaw("Select Id,SmsRequestLogId,StatusQueryId,Operator from SmsResponseLog (NOLOCK) WHERE OperatorResponseCode = 0 AND CreatedAt Between {0} AND {1} AND (status is null OR status = '')", startDate.ToString("yyyy-MM-dd HH:mm"), endDate.ToString("yyyy-MM-dd HH:mm"))
                        .AsNoTracking().ToListAsync();

                        _logManager.LogInformation("Sms Count : " + smsResponseLogs.Count);

                        ConcurrentBag<SmsEntitiesToBeProcessed> concurrentBag = new();

                        smsResponseLogs.DivideListIntoParts(50).ForEach(async smsResponseLogsParts =>
                        {
                            _logManager.LogInformation("Part Count : " + smsResponseLogsParts.Count);
                            var taskList = new List<Task>();
                            smsResponseLogsParts.ForEach(smsResponseLog =>
                            {
                                taskList.Add(GetDeliveryStatus(smsResponseLog, concurrentBag));
                            });
                            await Task.WhenAll(taskList);
                        });

                        int counter = 0;
                        foreach (var entities in concurrentBag)
                        {
                            if (entities.smsTrackingLog != null)
                            {
                                await _dbContext.SmsTrackingLog.AddAsync(entities.smsTrackingLog);
                                counter++;
                            }
                            if (entities.smsResponseLog != null)
                            {
                                _dbContext.SmsResponseLog.Update(entities.smsResponseLog);
                            }
                            if (counter != 0 && counter % 100 == 0)
                            {
                                await _dbContext.SaveChangesAsync();
                            }
                        }
                        if (counter != 0 && counter % 100 != 0)
                        {
                            await _dbContext.SaveChangesAsync();
                        }
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
            _logManager.LogInformation("Sms Tracking Finished");
            
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