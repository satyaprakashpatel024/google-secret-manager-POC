using Console_gsm_poc.gsm.secrets.Models;


namespace Console_gsm_poc.gsm.secrets.Cache;

public interface ISecretCache
{
    List<AppSecrets> GetFromCache(String key);
    void AddToCache(String key, string secrets);
}