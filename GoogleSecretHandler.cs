using Console_gsm_poc.gsm.secrets.Cache;
using Console_gsm_poc.gsm.secrets.Logger;
using Console_gsm_poc.gsm.secrets.settings;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.Configuration;

namespace Console_gsm_poc;

public class GoogleSecretHandler
{
    private readonly FileLogger _logger = FileLogger.Instance;
    private readonly string _projectId;
    private readonly SecretManager _secretManager;

    public GoogleSecretHandler(IConfiguration config)
    {
        _logger.Info("Connecting to gcp to fetch secrets from secret manager ");
        var keyPath = config["GcpSettings:ServiceAccountKeyFilePath"];

        var credential = GoogleCredential.FromFile(keyPath);
        var clientBuilder = new SecretManagerServiceClientBuilder
        {
            Credential = credential
        };
        var client = clientBuilder.Build();
        var secretCache = new SecretCache(new AppSettings(config));
        _projectId = config["GcpSettings:ProjectId"];
        _secretManager = new SecretManager(secretCache, client);
        _logger.Info("Successfully connected to gcp...");
    }

    public async void ConfigCredentialsFromSecretManager()
    {
        if (string.IsNullOrEmpty(_projectId))
        {
            _logger.Error("No project ID found. Please check your configuration file.");
            return;
        }
        // var secretService = new SecretManager();
        // _secretManager.AccessAllSecrets(_projectId);
        // var secrets = await _secretManager.GetSecretsByLabelAsync(_projectId, "env", "dev");


// 1. UNIFIED APPROACH - Get all secrets (both JSON and plain text)
        var allSecrets = await _secretManager.GetAllSecretsByLabel(_projectId, "database", "sql");
        Console.WriteLine(allSecrets.Count+" secrets found.");
        foreach (var kvp in allSecrets)
        {
            Console.WriteLine($"Secret: {kvp.Key}");
            if (kvp.Value.IsJson)
            {
                Console.WriteLine($"Type: JSON, Content: {kvp.Value.AsString()}");
                // Access JSON properties
                var apiKey = kvp.Value.GetJsonProperty("username");
                if (apiKey != null)
                    Console.WriteLine($"API Key from JSON: {apiKey}");
            }
            else
            {
                Console.WriteLine($"Type: Plain Text, Content: {kvp.Value.PlainText}");
            }

            kvp.Value.Dispose();
        }
    }
}