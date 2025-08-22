using Console_gsm_poc.gsm.secrets;
using Console_gsm_poc.gsm.secrets.Cache;
using Console_gsm_poc.gsm.secrets.Logger;
using Console_gsm_poc.gsm.secrets.Models;
using Google.Api.Gax;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;
using Newtonsoft.Json;

namespace Console_gsm_poc;

public class SecretManager
{
    private readonly SecretCache _secretCache;
    private PagedEnumerable<ListSecretsResponse, Secret> _secrets;
    private readonly SecretManagerServiceClient _client;
    private FileLogger logger = FileLogger.Instance;

    public SecretManager(SecretCache secretCache, SecretManagerServiceClient client)
    {
        _secretCache = secretCache;
        _client = client;
    }

    public void AccessAllSecrets(string projectId)
    {
        try
        {
            ProjectName projectName = new ProjectName(projectId);
            logger.Info("fetching all secret meta data from secret manager..");
            _secrets = _client.ListSecrets(projectName);
            logger.Info("Successfully fetched all secret meta data from secret manager..");
            GetSecret("client-portal", "env", "dev");
        }
        catch (Exception e)
        {
            logger.Error("Error in Accessing all Secrets"+e.Message);
            return;
        }
    }

    public IEnumerable<Secrets> GetSecret(string key, string labelKey, string labelValue)
    {
        IEnumerable<Secrets> fromCache = _secretCache.GetFromCache(key);
        if (fromCache != null)
        {
            logger.Info($"Found {fromCache.Count()} secrets from cache.");
            return fromCache;
        }
        return GroupSecretsByLabel(key,labelKey, labelValue);
    }

    public IEnumerable<Secrets> GroupSecretsByLabel(string key,string labelKey, string labelValue)
    {
        logger.Info($"fetching and grouping secrets for label {labelKey} with label {labelValue}");
        List<Secrets> list = new List<Secrets>();
        foreach (Secret secret in _secrets)
        {
            try
            {
                if (secret.Labels.TryGetValue(labelKey, out var value) && value == labelValue)
                {
                    SecretVersionName secretVersionName = new SecretVersionName(
                        secret.SecretName.ProjectId,
                        secret.SecretName.SecretId,
                        "latest");
                    AccessSecretVersionResponse result = _client.AccessSecretVersion(secretVersionName);
                    string payload = result.Payload.Data.ToStringUtf8();
                    list.Add(
                        new Secrets(secret.SecretName.SecretId, payload)
                    );
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to fetch secret for {secret.SecretName.SecretId}");
            }
        }
        logger.Info($"fetched and grouped all secrets for label {labelKey} with label {labelValue}");
        string str = JsonConvert.SerializeObject(list);
        logger.Info("Adding all secrets to cache ");
        _secretCache.AddToCache(key,str);
        return list;
    }
}