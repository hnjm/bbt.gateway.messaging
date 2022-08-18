using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.worker
{
    public class TemplateWorker : BackgroundService
    {
        private readonly ILogger<TemplateWorker> _logger;
        private readonly IMessagingGatewayApi _messagingGatewayApi;

        public TemplateWorker(ILogger<TemplateWorker> logger,
            IMessagingGatewayApi messagingGatewayApi)
        {
            _logger = logger;
            _messagingGatewayApi = messagingGatewayApi;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var taskList = new List<Task>();

                taskList.Add(_messagingGatewayApi.SetSmsTemplates());
                taskList.Add(_messagingGatewayApi.SetMailTemplates());
                taskList.Add(_messagingGatewayApi.SetPushTemplates());

                await Task.WhenAll(taskList);

                await Task.Delay(1000*60*10, stoppingToken);

            }
        }
    }
}
