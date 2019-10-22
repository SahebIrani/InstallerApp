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

        public static IServiceCollection AddConfigureDatabaseWithIdentity(this IServiceCollection services, IConfiguration config)
        {
            string DataProvider = ConstStrings.DataProvider;

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

        public static IApplicationBuilder UseFirstStartConfiguration(this IApplicationBuilder app, IConfiguration configuration, Func<Task> restartHost)
        {
            _RestartHost = restartHost;

            bool IsFirstStart = configuration.GetValue<bool>(ConstStrings.IsFirstStart);
            if (IsFirstStart)
            {
                app.UseWhen(IsFirstStartIncomplete, configuration =>
                {
                    configuration.MapWhen(IsNotStartsWithSegmentsFirstStart, configuration =>
                        configuration.Run(request =>
                        {
                            request.Response.Redirect(ConstStrings.FirstStart);
                            return Task.CompletedTask;
                        })
                    );

                    configuration.UseRouting();

                    configuration.UseEndpoints(endpoints =>
                    {
                        endpoints.MapRazorPages();
                    });
                });
            }
            else
            {
                app.MapWhen(, configuration =>
                {
                    configuration.Run(request =>
                    {
                        request.Response.Redirect("/Index");
                        return Task.CompletedTask;
                    });
                });
            }

            return app;
        }

        private static bool IsFirstStartIncomplete(HttpContext context) =>
            context.RequestServices.GetRequiredService<IConfiguration>().GetValue<bool>(ConstStrings.IsFirstStart);

        private static bool IsStartsWithSegmentsFirstStart(HttpContext context) =>
            context.Request.Path.StartsWithSegments(ConstStrings.FirstStart);

        private static bool IsNotStartsWithSegmentsFirstStart(HttpContext context) =>
            !context.Request.Path.StartsWithSegments(ConstStrings.FirstStart);
    }
}
