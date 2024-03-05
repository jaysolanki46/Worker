using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Worker.Functions;
using Worker.Services.Database;
using Worker.Settings;

namespace Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IConfiguration config = Configuration();

            var services = new ServiceCollection();

            services.AddSingleton(config.GetSection("DatabaseConnectionSettings").Get<DatabaseConnectionSettings>());
            services.AddSingleton(config.GetSection("ScanEventSettings").Get<ScanEventSettings>());
            services.AddSingleton<ParcelEventWorker>();

            //Services
            services.AddScoped<IDatabaseService, DatabaseService>();

            services.AddHttpClient();

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.GetRequiredService<ParcelEventWorker>().ExecuteAsync(CancellationToken.None);
        }

        private static IConfiguration Configuration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();

            var basePath = Environment.GetEnvironmentVariable("CONFIG_DIR") ?? Directory.GetCurrentDirectory();
            builder.SetBasePath(basePath);
            builder.AddJsonFile("appsettings.json", false, false);

            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            if (!string.IsNullOrEmpty(environment))
                builder.AddJsonFile($"appsettings.{environment}.json", false, false);

            builder.AddEnvironmentVariables();

            IConfiguration config = builder.Build();
            return config;
        }
    }
}
