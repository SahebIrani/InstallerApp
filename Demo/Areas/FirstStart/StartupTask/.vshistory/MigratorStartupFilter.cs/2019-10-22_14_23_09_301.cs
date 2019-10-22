using System;
using System.Threading;
using System.Threading.Tasks;

using Demo.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Demo.Areas.FirstStart.StartupTask
{
    public interface IStartupTask
    {
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
    public class MigratorStartupFilter : IStartupTask
    {
        public MigratorStartupFilter(IServiceProvider serviceProvider)
            => ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

        public IServiceProvider ServiceProvider { get; }

        public async Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            using var scope = ServiceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync(cancellationToken);

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            await userManager.CreateAsync(new IdentityUser { UserName = "Sinjul.MSBH@Yahoo.Com", EmailConfirmed = true }, "Pa$$w0rd");
        }
    }

    public static class StartupTaskWebHostExtensions
    {
        public static async Task<IHost> RunWithTasksAsync(this IHost webHost, CancellationToken cancellationToken = default)
        {
            await webHost.Services.GetService<IStartupTask>().ExecuteAsync(cancellationToken);
            await webHost.RunAsync(cancellationToken);

            return webHost;
        }
    }
}
