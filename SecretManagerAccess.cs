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

        // int count = 0;
        // // Iterate over each secret found.
        // foreach (Secret secret in secrets)
        // {
        //     // count++;
        //     Console.WriteLine($"Found secret: {secret}");
        //
        //     // Build the secret version name for the 'latest' version.
        //     SecretVersionName secretVersionName = new SecretVersionName(
        //         secret.SecretName.ProjectId,
        //         secret.SecretName.SecretId,
        //         "latest");
        //
        //     // Access the secret version's payload.
        //     AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);
        //
        //     // Decode the payload from Base64.
        //     string payload = result.Payload.Data.ToStringUtf8();
        //
        //     Console.WriteLine($" -> Plaintext value: {result.Payload.Data.ToStringUtf8()}\n");
        //     // break;
        // }
        //
        // if (count == 0)
        // {
        //     Console.WriteLine("No secrets found in this project.");
        // }

        Dictionary<string, string> data = GroupSecretsByLabel(client, secrets, "env", "dev");
        // Console.WriteLine("secrets fetched by label ",data.Keys);
        // Console.WriteLine(string.Join(", ", data.Select(kvp => $"{kvp.Key}: {kvp.Value}")));
        Console.WriteLine(JsonConvert.SerializeObject(data));

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
}