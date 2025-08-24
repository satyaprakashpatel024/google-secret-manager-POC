using Console_gsm_poc;
using Console_gsm_poc.gsm.secrets.settings;
using Microsoft.Extensions.Configuration;

class Program
{
    static void Main(string[] args)
    {
        // 1. Build the configuration
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) 
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        string projectId = configuration["GcpSettings:ProjectId"];
        
        // var appSettings = new AppSettings(configuration);

        // 4. You can now access all your settings through the appSettings object
        //Console.WriteLine($"Using Project ID: {projectId}");
        // Console.WriteLine($"Max Retry Attempts: {appSettings.SecretManagerMaxRetryAttempts}");
        // Console.WriteLine($"Retry Interval (ms): {appSettings.SecretManagerRetryIntervalInMilliseconds}");
        // Console.WriteLine($"Cache Expiration (hrs): {appSettings.SecretManagerCacheExpirationInHours}");


        // 3. Pass the projectId to your method
        // Console.WriteLine($"Using Project ID from configuration: {projectId}");
        //
        GoogleSecretHandler c = new GoogleSecretHandler(configuration);
        c.ConfigCredentialsFromSecretManager();

    }
}

