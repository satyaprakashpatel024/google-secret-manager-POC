using Console_gsm_poc.models;

namespace Console_gsm_poc.helper;

using Google.Cloud.SecretManager.V1;
using System.Collections.Generic;
using System.Text.Json;
using Google.Api.Gax;

public class SecretParser
{
    /// <summary>
    /// Parses a list of secrets, deserializing JSON values into objects
    /// and storing simple strings directly.
    /// </summary>
    /// <param name="client">The active SecretManagerServiceClient.</param>
    /// <param name="secrets">The PagedEnumerable list of secrets already fetched.</param>
    /// <returns>A dictionary mapping secret names to their parsed values (either an object or a string).</returns>
    public Dictionary<string, object> ParseAllSecrets(
        SecretManagerServiceClient client,
        PagedEnumerable<ListSecretsResponse, Secret> secrets)
    {
        var config = new Dictionary<string, object>();
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        foreach (var secret in secrets)
        {
            // Access the secret's value
            var secretVersionName = new SecretVersionName(secret.SecretName.ProjectId, secret.SecretName.SecretId, "latest");
            AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);
            string payload = result.Payload.Data.ToStringUtf8();

            // Use the secret's name to decide how to parse it
            switch (secret.SecretName.SecretId)
            {
                case "mongodb":
                case "mysql":
                    // Deserialize the JSON payload into a DatabaseConfig object
                    var dbConfig = JsonSerializer.Deserialize<DatabaseConfig>(payload, jsonOptions);
                    config[secret.SecretName.SecretId] = dbConfig;
                    break;

                case "redis":
                    // Deserialize the JSON payload into a RedisConfig object
                    var redisConfig = JsonSerializer.Deserialize<RedisConfig>(payload, jsonOptions);
                    config[secret.SecretName.SecretId] = redisConfig;
                    break;

                default:
                    // For all other secrets, treat the payload as a simple string
                    config[secret.SecretName.SecretId] = payload;
                    break;
            }
        }
        return config;
    }
}