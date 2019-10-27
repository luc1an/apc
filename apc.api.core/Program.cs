using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace apc.api.core
{
    public class Program
    {
        private const string Urls = "http://0.0.0.0:5003";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(Urls)
                              .UseStartup<Startup>();
                });
    }
}
