using bbt.gateway.messaging.Api.TurkTelekom.Model;
using bbt.gateway.messaging.Workers;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace bbt.gateway.messaging.Api.TurkTelekom
{
    public class TurkTelekomApi
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly OperatorManager _operatorManager;
        public TurkTelekomApi(IConfiguration configuration,OperatorManager operatorManager) {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _operatorManager = operatorManager;
        }

        public async Task<TurkTelekomSmsResponse> SendSms(TurkTelekomSmsRequest turkTelekomSmsRequest) 
        {
            await Task.CompletedTask;
            //var xmlBody = new StringContent(turkTelekomSmsRequest.SerializeXml(), Encoding.UTF8, "application/xml");

            //Create process
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "cmd.exe";

            File.WriteAllText("request.json", turkTelekomSmsRequest.SerializeXml());

            //strCommandParameters are parameters to pass to program
            pProcess.StartInfo.Arguments = $"/c curl {_operatorManager.Get(Models.OperatorType.TurkTelekom).SendService} -X POST -d @request.json";

            pProcess.StartInfo.UseShellExecute = false;

            //Set output of program to be written to process output stream
            pProcess.StartInfo.RedirectStandardOutput = true;

            pProcess.StartInfo.CreateNoWindow = true;

            //Start the process
            pProcess.Start();

            //Get program output
            string strOutput = pProcess.StandardOutput.ReadToEnd();

            //Wait for process to finish
            pProcess.WaitForExit();
            //Call Web Service
            
            //var response = await _httpClient.PostAsync(_configuration.GetValue<string>("OperatorsApiEndpoints:TurkTelekom:SendSms"), xmlBody);
                
            return strOutput.DeserializeXml<TurkTelekomSmsResponse>();

        }

        public async Task<TurkTelekomSmsStatusResponse> CheckSmsStatus(TurkTelekomSmsStatusRequest turkTelekomSmsStatusRequest)
        {
            //async methods should use await
            await Task.CompletedTask;
            //Call Web Service
            //var xmlBody = new StringContent(turkTelekomSmsStatusRequest.SerializeXml(), Encoding.UTF8, "application/xml");
            //var response = await _httpClient.PostAsync(_configuration.GetValue<string>("OperatorsApiEndpoints:TurkTelekom:CheckSms"), xmlBody);

            //Create process
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "cmd.exe";

            File.WriteAllText("request.json", turkTelekomSmsStatusRequest.SerializeXml());

            //strCommandParameters are parameters to pass to program
            pProcess.StartInfo.Arguments = $"/c curl {_operatorManager.Get(Models.OperatorType.TurkTelekom).QueryService} -X POST -d @request.json";

            pProcess.StartInfo.UseShellExecute = false;

            //Set output of program to be written to process output stream
            pProcess.StartInfo.RedirectStandardOutput = true;

            pProcess.StartInfo.CreateNoWindow = true;

            //Start the process
            pProcess.Start();

            //Get program output
            string strOutput = pProcess.StandardOutput.ReadToEnd();

            //Wait for process to finish
            pProcess.WaitForExit();


            return strOutput.DeserializeXml<TurkTelekomSmsStatusResponse>();
        }
    }
}
