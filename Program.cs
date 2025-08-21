using Console_gsm_poc;

using Microsoft.Extensions.Configuration;

class Program
{
    static void Main(string[] args)
    {
        // 1. Build the configuration
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) 
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Loads the file
            .Build();

        // 2. Read the ProjectId from the configuration
        //    The colon ":" is used to access nested sections in the JSON.
        string projectId = configuration["GcpSettings:ProjectId"];

        if (string.IsNullOrEmpty(projectId))
        {
            Console.WriteLine("ProjectId not found in appsettings.json. Please check your configuration.");
            return;
        }

        // 3. Pass the projectId to your method
        // Console.WriteLine($"Using Project ID from configuration: {projectId}");
        //
        var secretManager = new SecretManagerAccess();
        secretManager.AccessAllSecrets(projectId);
    }
}

