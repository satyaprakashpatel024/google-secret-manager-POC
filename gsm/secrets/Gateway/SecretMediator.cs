using Console_gsm_poc.gsm.secrets.Logger;
using Console_gsm_poc.gsm.secrets.Models;
using Google.Api.Gax;
using Google.Cloud.SecretManager.V1;
using System.Text.Json;
using Console_gsm_poc.gsm.secrets.Cache;
using Console_gsm_poc.gsm.secrets.Logger;
using Console_gsm_poc.gsm.secrets.Models;
using Console_gsm_poc.helper;
using Google.Api.Gax;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;
using Newtonsoft.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace Console_gsm_poc.gsm.secrets.Gateway;

public class SecretMediator
{
    private PagedEnumerable<ListSecretsResponse, Secret> _secrets;
    private readonly FileLogger _logger = FileLogger.Instance;
    private readonly SecretManagerServiceClient _client;
    private readonly SecretCache _secretCache;
    private readonly string _projectId;

    public SecretMediator(SecretManagerServiceClient client,SecretCache secretCache, string projectId)
    {
        _client = client;
        _secretCache = secretCache;
        _projectId = projectId;
    }

    
    public IEnumerable<AppSecrets> getAllSecretsByLabel(string CacheKey,string labelKey, string labelValue,string version="latest")
    {
        _logger.Info($"fetching and grouping secrets for label {labelKey} with label {labelValue}");
        List<AppSecrets> list = new List<AppSecrets>();
        
        ProjectName projectName = new ProjectName(_projectId);
        _client.ListSecrets(projectName);
        foreach (Secret secret in _secrets)
        {
            try
            {
                if (secret.Labels.TryGetValue(labelKey, out var value) && value == labelValue)
                {
                    SecretVersionName secretVersionName = new SecretVersionName(
                        secret.SecretName.ProjectId, 
                        secret.SecretName.SecretId, 
                        version);
                    AccessSecretVersionResponse result = _client.AccessSecretVersion(secretVersionName);
                    string payload = result.Payload.Data.ToStringUtf8();
                    list.Add(
                        new AppSecrets(secret.SecretName.SecretId, payload)
                    );
                }
            }
            catch (Exception e)
            {
                _logger.Error($"Failed to fetch secret for {secret.SecretName.SecretId}");
            }
        }
        _logger.Info($"fetched and grouped all secrets for label {labelKey} with label {labelValue}");
        string str = JsonConvert.SerializeObject(list);
        _logger.Info("Adding all secrets to cache ");
        _secretCache.AddToCache(CacheKey,str);
        return list;
    }

    private async Task<string> GetLatestSecretVersion(string secretName)
    {
        var versionName = $"{secretName}/versions/latest";
        var response =  _client.AccessSecretVersion(versionName);
        return response.Payload.Data.ToStringUtf8();
    }
    
    public async Task<List<AppSecrets>> GetAllJsonSecretsByLabel(string CacheKey, string labelKey, string labelValue)
    {
        IEnumerable<AppSecrets> fromCache = _secretCache.GetFromCache(CacheKey);
        if (fromCache != null)
        {
            _logger.Info($"Found {fromCache.Count()} secrets from cache.");
            return fromCache as List<AppSecrets>;
        }
        var secrets = new List<AppSecrets>();
        try
        {
            var parent = new ProjectName(_projectId);
            var request = new ListSecretsRequest
            {
                ParentAsProjectName = parent,
                Filter = $"labels.{labelKey}={labelValue}"
            };
            
            PagedEnumerable<ListSecretsResponse, Secret> secretList = _client.ListSecrets(request);
            
            foreach (var secret in secretList)
            {
                try
                {
                    string secretValue = await GetLatestSecretVersion(secret.Name);
                    List<AppSecrets> appSecretsList = MapJsonToAppSecrets(secretValue);
                    _logger.Info($"fetched all secrets for label {labelKey} with label {labelValue}");
                    _secretCache.AddToCache(CacheKey,JsonConvert.SerializeObject(appSecretsList));
                    _logger.Info("Adding all secrets to cache ");
                    return appSecretsList;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to retrieve secret {secret.Name}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching secrets: {ex.Message}");
            throw;
        }
        return secrets;
    }
    private List<AppSecrets> MapJsonToAppSecrets(string jsonString)
    {
        var appSecrets = new List<AppSecrets>();
        
        try
        {
            using var jsonDoc = JsonDocument.Parse(jsonString);
            FlattenJsonElement(jsonDoc.RootElement, "", appSecrets);
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON parsing error: {ex.Message}");
        }
        return appSecrets;
    }
    
    private void FlattenJsonElement(JsonElement element, string prefix, List<AppSecrets> appSecrets)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    // For root level properties, use just the property name as key
                    var key = string.IsNullOrEmpty(prefix) ? property.Name : $"{prefix}.{property.Name}";
                    FlattenJsonElement(property.Value, key, appSecrets);
                }
                break;

            case JsonValueKind.Array:
                for (int i = 0; i < element.GetArrayLength(); i++)
                {
                    FlattenJsonElement(element[i], $"{prefix}[{i}]", appSecrets);
                }
                break;

            case JsonValueKind.String:
                appSecrets.Add(new AppSecrets { Key = prefix, Value = element.GetString() });
                break;

            case JsonValueKind.Number:
                appSecrets.Add(new AppSecrets { Key = prefix, Value = element.GetRawText() });
                break;

            case JsonValueKind.True:
                appSecrets.Add(new AppSecrets { Key = prefix, Value = "true" });
                break;

            case JsonValueKind.False:
                appSecrets.Add(new AppSecrets { Key = prefix, Value = "false" });
                break;

            case JsonValueKind.Null:
                appSecrets.Add(new AppSecrets { Key = prefix, Value = null });
                break;

            default:
                appSecrets.Add(new AppSecrets { Key = prefix, Value = element.GetRawText() });
                break;
        }
    }
    
}