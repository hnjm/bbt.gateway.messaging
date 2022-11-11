
using bbt.gateway.common;
using bbt.gateway.common.GlobalConstants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace bbt.gateway.messaging
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
            .UseVaultSecrets(GlobalConstants.DAPR_SECRET_STORE)
            .UseConsulSettings(typeof(Program))
            .UseSeriLog("entegrasyon")
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            }).Build().Run();
        }


    }
}
