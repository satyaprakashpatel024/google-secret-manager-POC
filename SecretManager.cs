using Console_gsm_poc.gsm.secrets.Cache;
using Console_gsm_poc.gsm.secrets.Gateway;
using Console_gsm_poc.gsm.secrets.Logger;
using Console_gsm_poc.gsm.secrets.Models;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;

namespace Console_gsm_poc;

public class SecretManager
{
    private readonly SecretCache _secretCache;
    private SecretMediator _secretMediator;
    private FileLogger _logger = FileLogger.Instance;

    public SecretManager(SecretCache secretCache, SecretManagerServiceClient client, string projectId)
    {
        _secretCache = secretCache;
        _secretMediator = new SecretMediator(client, secretCache,projectId);
    }
    
    public void AccessAllSecrets( )
    {
        try
        {
            _logger.Info("fetching all secret meta data from secret manager..");
            // _client.ListSecretsAsync();
            _logger.Info("Successfully fetched all secret meta data from secret manager..");
            IEnumerable<AppSecrets> secretsEnumerable = GetSecrets("client-portal", "env", "dev");
        }
        catch (Exception e)
        {
            _logger.Error("Error in Accessing all AppSecrets"+e.Message);
            return;
        }
    }
    
    public IEnumerable<AppSecrets> GetSecrets(string CacheKey, string labelKey, string labelValue)
    {
        IEnumerable<AppSecrets> fromCache = _secretCache.GetFromCache(CacheKey);
        if (fromCache != null)
        {
            _logger.Info($"Found {fromCache.Count()} secrets from cache.");
            return fromCache;
        }
        return _secretMediator.getAllSecretsByLabel(CacheKey,labelKey, labelValue);
    } 
    
    public IEnumerable<AppSecrets> GetJsonSecrets(string CacheKey, string labelKey, string labelValue)
    {
        IEnumerable<AppSecrets> fromCache = _secretCache.GetFromCache(CacheKey);
        if (fromCache != null)
        {
            _logger.Info($"Found {fromCache.Count()} secrets from cache.");
            return fromCache;
        }
        return _secretMediator.GetAllJsonSecretsByLabel(CacheKey,labelKey, labelValue) as IEnumerable<AppSecrets>;
    }
    
    

    public string getSecret(string key,IEnumerable<AppSecrets> secrets)
    {
        if (string.IsNullOrEmpty(key) || secrets == null)
        {
            return null;
        }
        return secrets.FirstOrDefault(s => s.Key == key)?.Value;
    }
    
    
    
}