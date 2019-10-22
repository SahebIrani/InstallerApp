using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Demo
{
    public class Program
    {
        private static IHost _Host;
        private static bool _Restart = true;

        public static async Task Main(string[] args)
        {
            while (_Restart)
            {

                _Restart = false;
                _Host = CreateHostBuilder(args).Build();
                await _Host.RunAsync();
            }
        }

        public static Task Restart()
        {
            _Restart = true;
            return _Host.StopAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
