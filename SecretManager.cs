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

namespace Console_gsm_poc;

public class SecretManager
{
    private readonly SecretCache _secretCache;
    private PagedEnumerable<ListSecretsResponse, Secret> _secrets;
    private readonly SecretManagerServiceClient _client;
    private FileLogger _logger = FileLogger.Instance;

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
            _logger.Info("fetching all secret meta data from secret manager..");
            _secrets = _client.ListSecrets(projectName);
            // _client.ListSecretsAsync();
            _logger.Info("Successfully fetched all secret meta data from secret manager..");
            IEnumerable<Secrets> secretsEnumerable = GetSecret("client-portal", "env", "dev");
        }
        catch (Exception e)
        {
            _logger.Error("Error in Accessing all Secrets"+e.Message);
            return;
        }
    }

    public IEnumerable<Secrets> GetSecret(string key, string labelKey, string labelValue)
    {
        IEnumerable<Secrets> fromCache = _secretCache.GetFromCache(key);
        if (fromCache != null)
        {
            _logger.Info($"Found {fromCache.Count()} secrets from cache.");
            return fromCache;
        }
        return GroupSecretsByLabel(key,labelKey, labelValue);
    }

    public IEnumerable<Secrets> GroupSecretsByLabel(string key,string labelKey, string labelValue)
    {
        _logger.Info($"fetching and grouping secrets for label {labelKey} with label {labelValue}");
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
                _logger.Error($"Failed to fetch secret for {secret.SecretName.SecretId}");
            }
        }
        _logger.Info($"fetched and grouped all secrets for label {labelKey} with label {labelValue}");
        string str = JsonConvert.SerializeObject(list);
        _logger.Info("Adding all secrets to cache ");
        _secretCache.AddToCache(key,str);
        return list;
    }
    
    /// <summary>
    /// Fetches all secrets from Google Secret Manager that match the specified label key and value
    /// </summary>
    public async Task<Dictionary<string, string>> GetSecretsByLabelAsync(string projectId, string labelKey, string labelValue)
    {
        var secrets = new Dictionary<string, string>();
        _logger.Info("Fetching secrets with label");
        try
        {
            var parent = new ProjectName(projectId);
            
            var request = new ListSecretsRequest
            {
                ParentAsProjectName = parent,
                Filter = $"labels.{labelKey}={labelValue}"
            };
            var secretList = _client.ListSecrets(request);
            foreach (var secret in secretList)
            {
                try
                {
                    var secretValue = await GetLatestSecretVersionAsync(secret.Name);
                    var secretName = ExtractSecretName(secret.Name);
                    secrets.Add(secretName, secretValue);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to retrieve secret {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Error fetching secrets: {ex.Message}");
            throw;
        }
        return secrets;
    }
    
    private async Task<string> GetLatestSecretVersionAsync(string secretName,string version = "latest")
    {
        var versionName = $"{secretName}/versions/{version}";
        try
        {
            var response = _client.AccessSecretVersion(versionName);
            return response.Payload.Data.ToStringUtf8();
        }
        catch (Exception e)
        {
            _logger.Error(string.Format("Error accessing secret version: {0}",version)+ e.Message);
            throw;
        }
    }
    
    private string ExtractSecretName(string fullResourceName)
    {
        var parts = fullResourceName.Split('/');
        return parts.Length >= 4 ? parts[3] : fullResourceName;
    }

    /// <summary>
    /// Fetches all secrets from Google Secret Manager that match the specified label key and value with json type
    /// </summary>
     public async Task<Dictionary<string, SecretContent>> GetAllSecretsByLabel(string projectId, string labelKey,
        string labelValue)
    {
        var secrets = new Dictionary<string, SecretContent>();
        try
        {
            var parent = new ProjectName(projectId);
            var request = new ListSecretsRequest
            {
                ParentAsProjectName = parent,
                Filter = $"labels.{labelKey}={labelValue}"
            };
            var secretList = _client.ListSecrets(request);
            foreach (var secret in secretList)
            {
                try
                {
                    var secretValue = await GetLatestSecretVersion(secret.Name);
                    var secretName = ExtractSecretName(secret.Name);

                    var content = ParseSecretContent(secretValue);
                    secrets.Add(secretName, content);
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
    private SecretContent ParseSecretContent(string secretValue)
    {
        try
        {
            var jsonDocument = JsonDocument.Parse(secretValue);
            return new SecretContent
            {
                IsJson = true,
                JsonDocument = jsonDocument,
                PlainText = secretValue
            };
        }
        catch (JsonException)
        {
            return new SecretContent
            {
                IsJson = false,
                JsonDocument = null,
                PlainText = secretValue
            };
        }
    }
    private async Task<string> GetLatestSecretVersion(string secretName)
    {
        var versionName = $"{secretName}/versions/latest";
        var response =  _client.AccessSecretVersion(versionName);
        return response.Payload.Data.ToStringUtf8();
    }
    
}