using System;
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
        public static Func<Task> _RestartHost;

        private static string DataProvider = ConstStrings.DataProvider;
        private static string Administrators = ConstStrings.Administrators;
        private static string FirstStart = ConstStrings.FirstStart;

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
                case "inmemory":
                    services.AddEntityFrameworkInMemoryDatabase();
                    optionsBuilder = options => options.UseInMemoryDatabase(InMemoryConnection);
                    break;

                case "sqlite":
                    services.AddEntityFrameworkSqlite();
                    optionsBuilder = options => options.UseSqlite(SQLiteConnection);
                    break;

                case "postgresql":
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
                .AddRoles<IdentityRole>()
                .AddRoleManager<RoleManager<IdentityRole>>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            return services;
        }

        public static IApplicationBuilder UseFirstStartConfiguration(this IApplicationBuilder app, Func<Task> restartHost)
        {
            _RestartHost = restartHost;

            app.UseWhen(IsFirstStartIncomplete, configuration =>
            {
                configuration.MapWhen(context => !context.Request.Path.StartsWithSegments(FirstStart), configuration =>
                    configuration.Run(request =>
                    {
                        request.Response.Redirect(FirstStart);
                        return Task.CompletedTask;
                    })
                );

                configuration.UseRouting();

                configuration.UseEndpoints(endpoints =>
                {
                    endpoints.MapRazorPages();
                });
            });

            return app;
        }

        private static bool IsFirstStartIncomplete(HttpContext context) =>
            context.RequestServices.GetRequiredService<IConfiguration>().GetValue<bool>(ConstStrings.IsFirstStart);
    }
}
