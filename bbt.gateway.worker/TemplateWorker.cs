using bbt.gateway.common;
using bbt.gateway.common.Api.dEngage;
using bbt.gateway.common.Api.dEngage.Model.Contents;
using bbt.gateway.common.Api.dEngage.Model.Login;
using bbt.gateway.common.GlobalConstants;
using bbt.gateway.common.Models;
using Dapr.Client;
using Elastic.Apm.Api;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Refit;
using System.Text;

namespace bbt.gateway.worker
{
    public class TemplateWorker : BackgroundService
    {
        private readonly LogManager _logManager;
        private readonly ITracer _tracer;
        private readonly DatabaseContext _dbContext;
        private Operator _operatorBurgan;
        private Operator _operatorOn;
        private readonly IdEngageClient _dEngageClient;
        private readonly DaprClient _daprClient;

        public TemplateWorker(
            LogManager logManager, ITracer tracer, DbContextOptions<DatabaseContext> dbContextOptions,
            IdEngageClient dEngageClient, DaprClient daprClient)
        {
            _logManager = logManager;
            _tracer = tracer;
            _dEngageClient = dEngageClient;
            _dbContext = new DatabaseContext(dbContextOptions);
            _daprClient = daprClient;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logManager.LogInformation("Set Templates Initiated");
            _operatorBurgan = await _dbContext.Operators.AsNoTracking().FirstAsync(o => o.Type == OperatorType.dEngageBurgan);
            _operatorOn = await _dbContext.Operators.AsNoTracking().FirstAsync(o => o.Type == OperatorType.dEngageOn);
            while (!stoppingToken.IsCancellationRequested)
            {
                _logManager.LogInformation("Set Templates Triggered");
                var taskList = new List<Task>();
                taskList.Add(SetTemplates(_operatorBurgan));
                taskList.Add(SetTemplates(_operatorOn));
                await Task.WhenAll(taskList);

                await Task.Delay(1000 * 60 * 10, stoppingToken);
            }
        }

        private async Task SetTemplates(Operator @operator)
        {
            await _tracer.CaptureTransaction("SetTemplates" + @operator.Type.ToString(), ApiConstants.TypeRequest, async () =>
            {
                try
                {
                    //Auth dEngage
                    await dEngageAuth(@operator);
                    if (!string.IsNullOrWhiteSpace(@operator.AuthToken))
                    {
                        var taskList = new List<Task>();

                        taskList.Add(SetSmsTemplates(@operator));
                        taskList.Add(SetMailTemplates(@operator));
                        taskList.Add(SetPushTemplates(@operator));

                        await Task.WhenAll(taskList);
                    }

                }
                catch (Exception ex)
                {
                    _logManager.LogError(ex.ToString());
                    _tracer.CaptureException(ex);
                }
            });
        }

        private async Task dEngageAuth(Operator @operator)
        {
            try
            {
                @operator.AuthToken = String.Empty;
                var response = await _dEngageClient.Login(new LoginRequest
                {
                    userkey = @operator.User,
                    password = @operator.Password,
                });
                @operator.AuthToken = response.access_token;
            }
            catch (ApiException apiEx)
            {
                _logManager.LogCritical(apiEx.ToString());
            }
            catch (Exception ex)
            {
                _logManager.LogCritical(ex.ToString());
            }
        }

        private async Task SetSmsTemplates(Operator @operator)
        {
            var span = _tracer.CurrentTransaction.StartSpan("Messaging Worker Set Sms", ApiConstants.TypeRequest, ApiConstants.SubtypeHttp);

            try
            {
                var smsContents = await _dEngageClient.GetSmsContents(@operator.AuthToken, 5000, "0");

                if (smsContents != null)
                {
                    if (smsContents.data?.result.Count > 0)
                    {
                        await _daprClient.SaveStateAsync("messaginggateway-statestore",
                            @operator.Type.ToString() + "_" + GlobalConstants.SMS_CONTENTS_SUFFIX,
                            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(smsContents.data.result)));
                        foreach (SmsContentInfo content in smsContents.data.result)
                        {
                            try
                            {
                                var smsContent = await _dEngageClient.GetSmsContent(@operator.AuthToken, content.publicId);
                                await _daprClient.SaveStateAsync("messaginggateway-statestore",
                                    @operator.Type.ToString() + "_" + GlobalConstants.SMS_CONTENTS_SUFFIX + "_" + content.publicId,
                                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(smsContent.data.contentDetail)));
                            }
                            catch (ApiException ex)
                            {
                                _logManager.LogError($"Api Exception - Status Code:{(int)ex.StatusCode} | An Error Occured While Trying To Caching Mail Contents");
                            }
                        }
                    }
                }
            }
            catch (ApiException apiEx)
            {
                span.CaptureException(apiEx);
                _logManager.LogError($"Api Exception - Status Code:{(int)apiEx.StatusCode} | An Error Occured While Trying To Caching Mail Contents");
            }
            catch (Exception ex)
            {
                span.CaptureException(ex);
                _logManager.LogError("An Error Occured While Trying To Caching Sms Contents");
            }
            finally
            {
                span.End();
            }
        }

        private async Task SetMailTemplates(Operator @operator)
        {
            var span = _tracer.CurrentTransaction.StartSpan("Messaging Worker Set Mail", ApiConstants.TypeRequest, ApiConstants.SubtypeHttp);

            try
            {
                var smsContents = await _dEngageClient.GetMailContents(@operator.AuthToken, 5000, "0");

                if (smsContents != null)
                {
                    if (smsContents.data?.result.Count > 0)
                    {
                        await _daprClient.SaveStateAsync("messaginggateway-statestore",
                            @operator.Type.ToString() + "_" + GlobalConstants.MAIL_CONTENTS_SUFFIX,
                            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(smsContents.data.result)));
                        foreach (ContentInfo content in smsContents.data.result)
                        {
                            try
                            {
                                var smsContent = await _dEngageClient.GetMailContent(@operator.AuthToken, content.publicId);
                                await _daprClient.SaveStateAsync("messaginggateway-statestore",
                                    @operator.Type.ToString() + "_" + GlobalConstants.MAIL_CONTENTS_SUFFIX + "_" + content.publicId,
                                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(smsContent.data.contentDetail)));
                            }
                            catch (ApiException ex)
                            {
                                _logManager.LogError($"Api Exception - Status Code:{(int)ex.StatusCode} | An Error Occured While Trying To Caching Mail Contents");
                            }
                        }
                    }
                }
            }
            catch (ApiException apiEx)
            {
                span.CaptureException(apiEx);
                _logManager.LogError($"Api Exception - Status Code:{(int)apiEx.StatusCode} | An Error Occured While Trying To Caching Mail Contents");
            }
            catch (Exception ex)
            {
                span.CaptureException(ex);
                _logManager.LogError("An Error Occured While Trying To Caching Mail Contents");
            }
            finally
            {
                span.End();
            }
        }

        private async Task SetPushTemplates(Operator @operator)
        {
            var span = _tracer.CurrentTransaction.StartSpan("Messaging Worker Set Push", ApiConstants.TypeRequest, ApiConstants.SubtypeHttp);

            try
            {
                var smsContents = await _dEngageClient.GetPushContents(@operator.AuthToken, 5000, "0");

                if (smsContents != null)
                {
                    if (smsContents.data?.result.Count > 0)
                    {
                        await _daprClient.SaveStateAsync("messaginggateway-statestore",
                            @operator.Type.ToString() + "_" + GlobalConstants.PUSH_CONTENTS_SUFFIX,
                            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(smsContents.data.result)));
                        foreach (PushContentInfo content in smsContents.data.result)
                        {
                            try
                            {
                                var smsContent = await _dEngageClient.GetPushContent(@operator.AuthToken, content.id);
                                await _daprClient.SaveStateAsync("messaginggateway-statestore",
                                    @operator.Type.ToString() + "_" + GlobalConstants.PUSH_CONTENTS_SUFFIX + "_" + content.id,
                                    Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(smsContent.data.contentDetail)));
                            }
                            catch (ApiException ex)
                            {
                                _logManager.LogError($"Api Exception - Status Code:{(int)ex.StatusCode} | An Error Occured While Trying To Caching Mail Contents");
                            }
                        }
                    }
                }
            }
            catch (ApiException apiEx)
            {
                span.CaptureException(apiEx);
                _logManager.LogError($"Api Exception - Status Code:{(int)apiEx.StatusCode} | An Error Occured While Trying To Caching Mail Contents");
            }
            catch (Exception ex)
            {
                span.CaptureException(ex);
                _logManager.LogError("An Error Occured While Trying To Caching Push Contents");
            }
            finally
            {
                span.End();
            }
        }
    }
}
