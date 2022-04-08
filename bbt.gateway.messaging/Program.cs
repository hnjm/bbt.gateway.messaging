
using bbt.gateway.common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace bbt.gateway.messaging
{
    
    public class Program
    {
        public static void Main(string[] args)
        {
            Host.CreateDefaultBuilder(args)
            .UseConsulSettings(typeof(Program))
            .UseSeriLog("entegrasyon")
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            }).Build().Run();

        }


    }
}
