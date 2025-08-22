using Google.Cloud.SecretManager.V1;
using Console_gsm_poc.gsm.secrets.Cache;
using Console_gsm_poc.gsm.secrets.Logger;
using Console_gsm_poc.gsm.secrets.settings;
using Microsoft.Extensions.Configuration;

namespace Console_gsm_poc;

public class GoogleSecretHandler
{
    private readonly string _projectId;
    private SecretManager _secretManager;
    private FileLogger logger = FileLogger.Instance;

    public GoogleSecretHandler(IConfiguration  config)
    {
        logger.Info("Connecting to gcp to fetch secrets from secret manager ");
        var client = SecretManagerServiceClient.Create();
        SecretCache secretCache = new SecretCache(new AppSettings(config));
        _projectId = config["GcpSettings:ProjectId"];
        _secretManager = new SecretManager(secretCache,client);
        logger.Info("Successfully connected to gcp...");
    }

    public void ConfigCredentialsFromSecretManager()
    {
        if (string.IsNullOrEmpty(this._projectId))
        {
            logger.Error("No project ID found. Please check your configuration file.");
            return;
        }
        _secretManager.AccessAllSecrets(_projectId);
    }
}
