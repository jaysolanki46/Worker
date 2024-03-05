using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Worker.Functions;
using Worker.Services.Database;
using Worker.Settings;

namespace Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            /* App configurations */
            IConfiguration config = Configuration();

            var services = new ServiceCollection();

            /* Environment settings */
            services.AddSingleton(config.GetSection("DatabaseConnectionSettings").Get<DatabaseConnectionSettings>());
            services.AddSingleton(config.GetSection("ScanEventSettings").Get<ScanEventSettings>());
            services.AddSingleton<ParcelEventWorker>();

            /*  Dependency Injections */
            services.AddScoped<IDatabaseService, DatabaseService>();

            /* HTTP Client setup */
            services.AddHttpClient();

            /* Serilog configurations */
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(config).CreateLogger();

            var serviceProvider = services.BuildServiceProvider();

            /* Execute parcel scan events  */
            await serviceProvider.GetRequiredService<ParcelEventWorker>().ExecuteAsync(CancellationToken.None);
        }

        /* App environmet settings */
        private static IConfiguration Configuration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();

            /*  Get basepath to configure appsettings file */
            var basePath = Environment.GetEnvironmentVariable("CONFIG_DIR") ?? Directory.GetCurrentDirectory();
            builder.SetBasePath(basePath);
            builder.AddJsonFile("appsettings.json", false, false);

            /* Choose appsettings file according to the environment  */
            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            if (!string.IsNullOrEmpty(environment))
                builder.AddJsonFile($"appsettings.{environment}.json", false, false);

            return builder.AddEnvironmentVariables().Build();
        }
    }
}
