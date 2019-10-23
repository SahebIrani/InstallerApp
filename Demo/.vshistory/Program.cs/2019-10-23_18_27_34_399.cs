using System.Threading.Tasks;

using Demo.Areas.FirstStart.StartupTask;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
                _Host = await CreateHostBuilder(args).Build().RunWithTasksAsync();
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
                    webBuilder
                        .ConfigureServices(services => services.AddTransient<IStartupTask, MigratorStartupFilter>())
                        .UseStartup<Startup>();
                });
    }
}
