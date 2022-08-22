using Refit;
using Serilog;

namespace bbt.gateway.worker
{
    public class TemplateWorker : BackgroundService
    {
        private readonly Serilog.ILogger _logger;
        private readonly IMessagingGatewayApi _messagingGatewayApi;

        public TemplateWorker(ILogger<TemplateWorker> logger,
            IMessagingGatewayApi messagingGatewayApi)
        {
            _logger = Log.ForContext<TemplateWorker>();
            _messagingGatewayApi = messagingGatewayApi;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var taskList = new List<Task>();

                taskList.Add(SetSmsTemplates());
                taskList.Add(SetMailTemplates());
                taskList.Add(SetPushTemplates());
                
                await Task.WhenAll(taskList);
                await Task.Delay(1000 * 60 * 10, stoppingToken);

            }
        }

        private async Task SetSmsTemplates()
        {
            try
            {
                await _messagingGatewayApi.SetSmsTemplates();
            }
            catch (ApiException ex)
            {
                _logger.Error("An Error Occured While Trying To Caching Sms Contents");
            }
        }

        private async Task SetMailTemplates()
        {
            try
            {
                await _messagingGatewayApi.SetMailTemplates();
            }
            catch (ApiException ex)
            {
                _logger.Error("An Error Occured While Trying To Caching Mail Contents");
            }
        }

        private async Task SetPushTemplates()
        {
            try
            {
                await _messagingGatewayApi.SetPushTemplates();
            }
            catch (ApiException ex)
            {
                _logger.Error("An Error Occured While Trying To Caching Push Contents");
            }
        }
    }
}
