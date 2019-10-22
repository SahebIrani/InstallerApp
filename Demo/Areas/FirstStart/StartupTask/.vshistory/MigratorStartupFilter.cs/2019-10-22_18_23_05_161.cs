using System;
using System.Threading;
using System.Threading.Tasks;

using Demo.Areas.FirstStart.Configurations;
using Demo.Data;
using Demo.Models;

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

            ApplicationDbContext context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            IConfiguration Configuration = serviceProvider.GetRequiredService<IConfiguration>();

            bool FirstStart = Configuration.GetValue<bool>(ConstStrings.FirstStart);
            string Administrators = ConstStrings.Administrators;
            bool InitializeFakeData = Configuration.GetValue<bool>(ConstStrings.InitializeFakeData);
            string DataProvider = Configuration.GetValue<string>(ConstStrings.DataProvider);
            string AdminUserName = Configuration.GetValue<string>(ConstStrings.AdminUserName);
            string AdminEmail = Configuration.GetValue<string>(ConstStrings.AdminEmail);
            string AdminPassword = Configuration.GetValue<string>(ConstStrings.AdminPassword);

            //IEnumerable<string> getAppliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            //IEnumerable<string> getPendingMigrations = await context.Database.GetPendingMigrationsAsync();
            //if (getPendingMigrations.Count() > 0)

            if (FirstStart || DataProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
                await context.Database.EnsureDeletedAsync(cancellationToken);

            await context.Database.EnsureCreatedAsync(cancellationToken);

            if (InitializeFakeData)
            {
                if (!await context.Products.AsNoTracking().AnyAsync(cancellationToken))
                {
                    Product[] products;
                    if (DataProvider.Equals("inmemory", StringComparison.OrdinalIgnoreCase))
                    {
                        products = new Product[]{
                        new Product{ FileName ="https://placeimg.com/260/220/arch" ,      Price = 10000 , Title = "Title Item 01" },
                        new Product{ FileName ="https://placeimg.com/260/220/nature" ,    Price = 20000 , Title = "Title Item 02" },
                        new Product{ FileName ="https://placeimg.com/260/220/animals" ,   Price = 30000 , Title = "Title Item 03" },
                        new Product{ FileName ="https://placeimg.com/260/220/people" ,    Price = 40000 , Title = "Title Item 04" },
                        new Product{ FileName ="https://placeimg.com/260/220/tech" ,      Price = 50000 , Title = "Title Item 05" },
                        new Product{ FileName ="https://placeimg.com/260/220/grayscale" , Price = 60000 , Title = "Title Item 06" },
                        new Product{ FileName ="https://placeimg.com/260/220/sepia" ,     Price = 70000 , Title = "Title Item 07" },
                        new Product{ FileName ="https://placeimg.com/260/220/arch" ,      Price = 80000 , Title = "Title Item 08" },
                    };
                    }
                    else
                    {
                        products = new Product[]
                        {
                        new Product{ Id = 1 , FileName ="https://placeimg.com/260/220/arch" ,      Price = 10000 , Title = "Title Item 01" },
                        new Product{ Id = 2 , FileName ="https://placeimg.com/260/220/nature" ,    Price = 20000 , Title = "Title Item 02" },
                        new Product{ Id = 3 , FileName ="https://placeimg.com/260/220/animals" ,   Price = 30000 , Title = "Title Item 03" },
                        new Product{ Id = 4 , FileName ="https://placeimg.com/260/220/people" ,    Price = 40000 , Title = "Title Item 04" },
                        new Product{ Id = 5 , FileName ="https://placeimg.com/260/220/tech" ,      Price = 50000 , Title = "Title Item 05" },
                        new Product{ Id = 6 , FileName ="https://placeimg.com/260/220/grayscale" , Price = 60000 , Title = "Title Item 06" },
                        new Product{ Id = 7 , FileName ="https://placeimg.com/260/220/sepia" ,     Price = 70000 , Title = "Title Item 07" },
                        new Product{ Id = 8 , FileName ="https://placeimg.com/260/220/arch" ,      Price = 80000 , Title = "Title Item 08" },
                        };
                    }
                    await context.Products.AddRangeAsync(products, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }
            }

            UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            IdentityUser AdminUser = new IdentityUser { UserName = AdminUserName, Email = AdminEmail, EmailConfirmed = true };
            bool checkAdmin = await context.Users.AsNoTracking().AnyAsync(cancellationToken);
            if (!checkAdmin)
            {
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
