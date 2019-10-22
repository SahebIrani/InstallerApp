using System;
using System.IO;
using System.Threading.Tasks;

using Demo.Areas.FirstStart.Configurations;
using Demo.Data;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Areas.FirstStart.Extensions
{

    public static class StartupExtensions
    {
        public static IServiceCollection ConfigureDatabaseWithIdentity(this IServiceCollection services, IConfiguration config)
        {
            if (string.IsNullOrEmpty(config["DataProvider"])) return services;
            services.AddRepositories(config);
            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
        {
            Action<DbContextOptionsBuilder> optionsBuilder;

            string SqlServerConnection = Configuration.GetConnectionString(ConnectinStrings.SqlServerConnection);
            string InMemoryConnection = Configuration.GetConnectionString(ConnectinStrings.InMemoryConnection);
            string PostgreSQLConnection = Configuration.GetConnectionString(ConnectinStrings.PostgreSQLConnection);
            string SQLiteConnection = Configuration.GetConnectionString(ConnectinStrings.SQLiteConnection);

            switch (config["DataProvider"].ToLowerInvariant())
            {
                case "":
                    services.AddEntityFrameworkInMemoryDatabase();
                    optionsBuilder = options => options.UseInMemoryDatabase();
                    break;

                case "SqlServer":
                    services.AddEntityFrameworkSqlite();
                    optionsBuilder = options => options.UseSqlite();
                    break;

                case "PostgreSQL":
                    services.AddEntityFrameworkNpgsql();
                    optionsBuilder = options => options.UseNpgsql("");
                    break;

                default:
                    services.AddEntityFrameworkSqlServer();
                    optionsBuilder = options => options.UseSqlServer();
                    break;
            }

            services.AddDbContextPool<ApplicationDbContext>(options =>
            {
                optionsBuilder(options);
                options.EnableSensitiveDataLogging();
            });

            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            return services;
        }


        //private static bool _RunAfterConfiguration = false;
        //private static bool _FirstStartIncomplete = true;
        public static Func<Task> _RestartHost;
        private static IConfiguration Configuration;
        private static string _AppConfigurationFilename;
        private static bool _IsAdminUserCreated = false;

        public static IServiceCollection AddFirstStartConfiguration(this IServiceCollection services)
        {
            //services.AddSingleton<FirstStartConfiguration>(new FirstStartConfiguration());
            return services;
        }

        public static IApplicationBuilder UseFirstStartConfiguration(this IApplicationBuilder app, Func<Task> restartHost)
        {
            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;

                IWebHostEnvironment webHostEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
                Configuration = serviceProvider.GetRequiredService<IConfiguration>();
                UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

                _AppConfigurationFilename = Path.Combine(webHostEnvironment.ContentRootPath, "appsettings.json");
                _RestartHost = restartHost;
                _IsAdminUserCreated = userManager.GetUsersInRoleAsync("Administrators").GetAwaiter().GetResult().Count > 0;
            }

            app.UseWhen(IsFirstStartIncomplete, thisApp =>
            {
                thisApp.MapWhen(context => !context.Request.Path.StartsWithSegments("/FirstStart"), mapApp =>
                    mapApp.Run(request =>
                    {
                        request.Response.Redirect("/FirstStart");
                        return Task.CompletedTask;
                    })
                );

                thisApp.UseEndpoints(endpoints =>
                {
                    endpoints.MapDefaultControllerRoute();
                    endpoints.MapRazorPages();
                });
            });

            return app;
        }

        private static bool IsFirstStartIncomplete(HttpContext context) =>
            string.IsNullOrEmpty(Configuration["DataProvider"]) || !_IsAdminUserCreated;
    }
}
