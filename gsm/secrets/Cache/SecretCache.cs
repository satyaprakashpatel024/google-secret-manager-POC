using System.Runtime.Caching;
using Console_gsm_poc.gsm.secrets.Logger;
using Console_gsm_poc.gsm.secrets.Models;
using Console_gsm_poc.gsm.secrets.settings;
using Newtonsoft.Json;

namespace Console_gsm_poc.gsm.secrets.Cache;

public class SecretCache : ISecretCache
{
    private IAppSettings _AppSettings;
    private FileLogger logger = FileLogger.Instance;
    public SecretCache(AppSettings appSettings)
    {
        _AppSettings = appSettings;
    }

    public List<AppSecrets> GetFromCache(string key)
    {
        Object obj = MemoryCache.Default.Get(key);
        if (obj != null)
        {
            return JsonConvert.DeserializeObject<List<AppSecrets>>(obj as string);
        }
        return null;
    }

    public void AddToCache(string key, string secrets)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
        }

        MemoryCache @Default = MemoryCache.Default;
        CacheItemPolicy cachePolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddHours(_AppSettings.SecretManagerCacheExpirationInHours)
        };
        @Default.Set(key, secrets, cachePolicy);
        logger.Info("successfully Added the secrets to cache...");
    }
}
