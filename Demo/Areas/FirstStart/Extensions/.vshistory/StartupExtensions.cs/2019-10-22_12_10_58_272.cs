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
        //private static bool _RunAfterConfiguration = false;
        //private static bool _FirstStartIncomplete = true;
        //private static string _AppConfigurationFilename;
        public static Func<Task> _RestartHost;
        private static IConfiguration Configuration;
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

            string SqlServerConnection = Configuration.GetConnectionString(ConstStrings.SqlServerConnection);
            string InMemoryConnection = Configuration.GetConnectionString(ConstStrings.InMemoryConnection);
            string PostgreSQLConnection = Configuration.GetConnectionString(ConstStrings.PostgreSQLConnection);
            string SQLiteConnection = Configuration.GetConnectionString(ConstStrings.SQLiteConnection);

            switch (config[DataProvider].ToLowerInvariant())
            {
                case  :
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
            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;

                //IWebHostEnvironment webHostEnvironment = serviceProvider.GetRequiredService<IWebHostEnvironment>();
                Configuration = serviceProvider.GetRequiredService<IConfiguration>();
                UserManager<IdentityUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

                //_AppConfigurationFilename = Path.Combine(webHostEnvironment.ContentRootPath, "appsettings.json");
                _RestartHost = restartHost;
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
                     endpoints.MapDefaultControllerRoute();
                     endpoints.MapRazorPages();
                 });
             });

            return app;
        }

        private static bool IsFirstStartIncomplete(HttpContext context) => !_IsAdminUserCreated;
    }
}
