using Console_gsm_poc.helper;
using Console_gsm_poc.models;
using Google.Api.Gax;
using Newtonsoft.Json;

namespace Console_gsm_poc;

using Google.Cloud.SecretManager.V1;
using System;
using Google.Api.Gax.ResourceNames;

public class SecretManagerAccess
{
    public void AccessAllSecrets(string projectId)
    {
        // Create the client.
        SecretManagerServiceClient client = SecretManagerServiceClient.Create();

        // Build the parent project name.
        ProjectName projectName = new ProjectName(projectId);
        Console.WriteLine($"Searching for all secrets in project {projectId}...");

        // Call the API to list all secrets. No filter is needed.
        PagedEnumerable<ListSecretsResponse,Secret> secrets = client.ListSecrets(projectName);

        // printSecrets(secrets,client);

        // Dictionary<string, string> data = GroupSecretsByLabel(client, secrets, "database", "redis");
        // Console.WriteLine("secrets fetched by label ",data.Keys);
        // Console.WriteLine(string.Join(", ", data.Select(kvp => $"{kvp.Key}: {kvp.Value}")));
        // Console.WriteLine(JsonConvert.SerializeObject(data));
        ProcessAndDisplaySecrets(client,secrets);
    }

    private void printSecrets(PagedEnumerable<ListSecretsResponse, Secret> secrets,SecretManagerServiceClient client)
    {
        foreach (Secret secret in secrets)
        {
            Console.WriteLine($"Found secret: {secret}");
        
            // Build the secret version name for the 'latest' version.
            SecretVersionName secretVersionName = new SecretVersionName(
                secret.SecretName.ProjectId,
                secret.SecretName.SecretId,
                "latest");
        
            // Access the secret version's payload.
            AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);
        
            // Decode the payload from Base64.
            string payload = result.Payload.Data.ToStringUtf8();
        
            Console.WriteLine($" -> Plaintext value: {result.Payload.Data.ToStringUtf8()}\n");
            // break;
        }
    }
    
    public Dictionary<string, string> GroupSecretsByLabel(
        SecretManagerServiceClient client,
        PagedEnumerable<ListSecretsResponse, Secret> secrets,
        string labelKey,
        string labelValue)
    {
        var secretDictionary = new Dictionary<string, string>();

        // Iterate over the list of secret metadata.
        foreach (Secret secret in secrets)
        {
            // Check if the secret has the desired label and value.
            if (secret.Labels.TryGetValue(labelKey, out var value) && value == labelValue)
            {

                // Build the resource name for the latest version.
                SecretVersionName secretVersionName = new SecretVersionName(
                    secret.SecretName.ProjectId,
                    secret.SecretName.SecretId,
                    "latest");

                // Access the secret's payload (this makes a new API call).
                AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);
                string payload = result.Payload.Data.ToStringUtf8();

                // Add the secret's name and value to the dictionary.
                secretDictionary.Add(secret.SecretName.SecretId, payload);
            }
        }
        return secretDictionary;
    }
    
    /// <summary>
    /// Parses a pre-fetched list of secrets and displays the structured configuration.
    /// </summary>
    /// <param name="client">The active SecretManagerServiceClient, needed to access secret values.</param>
    /// <param name="allSecrets">The PagedEnumerable list of secrets you have already fetched.</param>
    public void ProcessAndDisplaySecrets(
        SecretManagerServiceClient client,
        PagedEnumerable<ListSecretsResponse, Secret> allSecrets)
    {
        // 1. Parse the secrets into a structured dictionary
        var parser = new SecretParser();
        Dictionary<string, object> applicationConfig = parser.ParseAllSecrets(client, allSecrets);

        // 2. Now you can access your configuration in a type-safe way!
        Console.WriteLine("\n--- Parsed Configuration ---");

        // Example: Accessing a complex object
        if (applicationConfig.TryGetValue("mongodb", out var mongoConfObj) && mongoConfObj is DatabaseConfig mongoConfig)
        {
            Console.WriteLine($"MongoDB Host: {mongoConfig.Host}");
            Console.WriteLine($"MongoDB User: {mongoConfig.Username}");
        }

        // Example: Accessing a simple string value
        if (applicationConfig.TryGetValue("postgres-db-url", out var pgUrlObj) && pgUrlObj is string postgresUrl)
        {
            Console.WriteLine($"Postgres URL: {postgresUrl}");
        }
    
        // Example: Accessing an integer value (by parsing the string)
        if (applicationConfig.TryGetValue("postgres-db-port", out var pgPortObj) && pgPortObj is string postgresPortStr)
        {
            if (int.TryParse(postgresPortStr, out int postgresPort))
            {
                Console.WriteLine($"Postgres Port: {postgresPort}");
            }
        }
    }
}