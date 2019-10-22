using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Demo.Areas.FirstStart.Configurations;
using Demo.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            var serviceProvider = scope.ServiceProvider;

            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            IEnumerable<string> getAppliedMigrations = context.Database.GetAppliedMigrations();
            IEnumerable<string> getPendingMigrations = context.Database.GetPendingMigrations();
            //if (getPendingMigrations.Count() > 0)
            context.Database.EnsureCreatedAsync().GetAwaiter().GetResult();

            IConfiguration Configuration = serviceProvider.GetRequiredService<IConfiguration>();
            string AdminUserName = Configuration[ConstStrings.AdminUserName];
            string AdminEmail = Configuration[ConstStrings.AdminEmail];
            string AdminPassword = Configuration[ConstStrings.AdminPassword];
            string Administrators = Configuration[ConstStrings.Administrators];

            UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            IdentityUser AdminUser = new IdentityUser { UserName = AdminUserName, Email = AdminEmail, EmailConfirmed = true };

            var userResult = await userManager.CreateAsync(AdminUser, AdminPassword);
            if (userResult.Succeeded)
            {
                if (!await roleManager.RoleExistsAsync(Administrators))
                {
                    var role = new IdentityRole(Administrators);
                    await roleManager.CreateAsync(role);
                }
                await userManager.AddToRoleAsync(AdminUser, Administrators);
            }
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
