using Elastic.Apm.Api;
using Refit;
using Serilog;

namespace bbt.gateway.worker
{
    public class TemplateWorker : BackgroundService
    {
        private readonly Serilog.ILogger _logger;
        private readonly IMessagingGatewayApi _messagingGatewayApi;
        private readonly LogManager _logManager;
        private readonly ITracer _tracer;

        public TemplateWorker(IMessagingGatewayApi messagingGatewayApi,
            LogManager logManager,ITracer tracer)
        {
            _messagingGatewayApi = messagingGatewayApi;
            _logManager = logManager;
            _tracer = tracer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logManager.LogInformation("Set Templates Triggered");
                await _tracer.CaptureTransaction("SetTemplates",ApiConstants.TypeRequest,async () => {
                    try
                    {
                        var taskList = new List<Task>();

                        taskList.Add(SetSmsTemplates());
                        taskList.Add(SetMailTemplates());
                        taskList.Add(SetPushTemplates());

                        await Task.WhenAll(taskList);
                    }
                    catch (Exception ex)
                    {
                        _logManager.LogError(ex.ToString());
                        _tracer.CaptureException(ex);
                    }
                });
                await Task.Delay(1000 * 60 * 10, stoppingToken);
            }
        }

        private async Task SetSmsTemplates()
        {
            var span = _tracer.CurrentTransaction.StartSpan("Messaging Gateway Set Sms", ApiConstants.TypeRequest, ApiConstants.SubtypeHttp);

            try
            {
                await _messagingGatewayApi.SetSmsTemplates();
            }
            catch (ApiException ex)
            {
                span.CaptureException(ex);
                _logManager.LogError("An Error Occured While Trying To Caching Sms Contents");
            }
            finally{
                span.End();
            }
        }

        private async Task SetMailTemplates()
        {
            var span = _tracer.CurrentTransaction.StartSpan("Messaging Gateway Set Mail", ApiConstants.TypeRequest, ApiConstants.SubtypeHttp);

            try
            {
                await _messagingGatewayApi.SetMailTemplates();
            }
            catch (ApiException ex)
            {
                _logManager.LogError("An Error Occured While Trying To Caching Mail Contents");
            }
            finally{
                span.End();
            }
        }

        private async Task SetPushTemplates()
        {
            var span = _tracer.CurrentTransaction.StartSpan("Messaging Gateway Set Sms", ApiConstants.TypeRequest, ApiConstants.SubtypeHttp);

            try
            {
                await _messagingGatewayApi.SetPushTemplates();
            }
            catch (ApiException ex)
            {
                _logger.Error("An Error Occured While Trying To Caching Push Contents");
            }
            finally{
                span.End();
            }
        }
    }
}
