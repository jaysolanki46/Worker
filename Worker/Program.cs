using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Worker.Functions;
using Worker.Settings;

namespace Worker
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            IConfiguration Config = Configuration();

            var services = new ServiceCollection();
            services.AddSingleton(Config.GetSection("DatabaseConnectionSettings").Get<DatabaseConnectionSettings>());
            services.AddSingleton<ParcelEventWorker>();

            services.AddHttpClient();

            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.GetService<ParcelEventWorker>().ExecuteAsync();
        }

        private static IConfiguration Configuration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            
            var basePath = Environment.GetEnvironmentVariable("CONFIG_DIR") ?? Directory.GetCurrentDirectory();
            builder.SetBasePath(basePath);
            builder.AddJsonFile("appsettings.json", false, false);

            var environment = Environment.GetEnvironmentVariable("ENVIRONMENT");
            if (!string.IsNullOrEmpty(environment))
                builder.AddJsonFile($"appsettings.development.json", false, false);
            builder.AddEnvironmentVariables();

            IConfiguration config = builder.Build();
            return config;
        }
    }
}
