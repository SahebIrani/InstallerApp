using System;
using System.Collections.Generic;
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
        //private static bool _RunAfterConfiguration = false;
        //private static bool _FirstStartIncomplete = true;
        //private static string _AppConfigurationFilename;
        //private static IConfiguration Configuration;
        public static Func<Task> _RestartHost;
        private static bool _IsAdminUserCreated = false;

        private static string DataProvider = ConstStrings.DataProvider;
        private static string Administrators = ConstStrings.Administrators;
        private static string FirstStart = $"/{ConstStrings.FirstStart}";

        public static IServiceCollection ConfigureDatabaseWithIdentity(this IServiceCollection services, IConfiguration config)
        {
            if (string.IsNullOrEmpty(config[DataProvider])) return services;
            services.AddRepositories(config);
            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration config)
        {
            Action<DbContextOptionsBuilder> optionsBuilder;

            string SqlServerConnection = config.GetConnectionString(ConstStrings.SqlServerConnection);
            string InMemoryConnection = config.GetConnectionString(ConstStrings.InMemoryConnection);
            string PostgreSQLConnection = config.GetConnectionString(ConstStrings.PostgreSQLConnection);
            string SQLiteConnection = config.GetConnectionString(ConstStrings.SQLiteConnection);

            switch (config[DataProvider].ToLowerInvariant())
            {
                case "InMemory":
                    services.AddEntityFrameworkInMemoryDatabase();
                    optionsBuilder = options => options.UseInMemoryDatabase(InMemoryConnection);
                    break;

                case "SqlServer":
                    services.AddEntityFrameworkSqlite();
                    optionsBuilder = options => options.UseSqlite(SQLiteConnection);
                    break;

                case "PostgreSQL":
                    services.AddEntityFrameworkNpgsql();
                    optionsBuilder = options => options.UseNpgsql(PostgreSQLConnection);
                    break;

                default:
                    services.AddEntityFrameworkSqlServer();
                    optionsBuilder = options => options.UseSqlServer(SqlServerConnection);
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

        public static IServiceCollection AddFirstStartConfiguration(this IServiceCollection services)
        {
            //services.AddSingleton<FirstStartConfiguration>(new FirstStartConfiguration());
            return services;
        }

        public static IApplicationBuilder UseFirstStartConfiguration(this IApplicationBuilder app, Func<Task> restartHost)
        {
            _RestartHost = restartHost;

            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;

                //IWebHostEnvironment webHostEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
                //Configuration = serviceProvider.GetRequiredService<IConfiguration>();
                //_AppConfigurationFilename = Path.Combine(webHostEnvironment.ContentRootPath, "appsettings.json");

                UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
                _IsAdminUserCreated = userManager.GetUsersInRoleAsync(Administrators).GetAwaiter().GetResult().Count > 0;
            }

            app.UseWhen(IsFirstStartIncomplete, configuration =>
            {
                configuration.MapWhen(context => !context.Request.Path.StartsWithSegments(FirstStart), configuration =>
                    configuration.Run(request =>
                    {
                        request.Response.Redirect(FirstStart);
                        return Task.CompletedTask;
                    })
                );

                configuration.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                });
            });

            if (!_IsAdminUserCreated)
                app.UseIntializeApp();

            return app;
        }

        private static bool IsFirstStartIncomplete(HttpContext context) => !_IsAdminUserCreated;

        public static void UseIntializeApp(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();
            IServiceProvider serviceProvider = scope.ServiceProvider;
            ApplicationDbContext context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            IEnumerable<string> getAppliedMigrations = context.Database.GetAppliedMigrations();
            IEnumerable<string> getPendingMigrations = context.Database.GetPendingMigrations();

            context.Database.Migrate();
        }
    }
}
