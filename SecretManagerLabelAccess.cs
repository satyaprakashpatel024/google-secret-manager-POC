namespace Console_gsm_poc;

using Google.Cloud.SecretManager.V1;
using System;
using Google.Api.Gax.ResourceNames;

public class SecretManagerLabelAccess
{
    /// <summary>
    /// Finds all secrets with a specific label and prints their latest version's value.
    /// </summary>
    /// <param name="projectId">Your Google Cloud project ID.</param>
    /// <param name="labelKey">The key of the label to filter by (e.g., "env").</param>
    /// <param name="labelValue">The value of the label to filter by (e.g., "production").</param>
    // public void AccessSecretsByLabel(
    //     string projectId = "galvanized-app-463214-d3",
    //     string labelKey = "env",
    //     string labelValue = "dev")
    // {
    //     // Create the client.
    //     SecretManagerServiceClient client = SecretManagerServiceClient.Create();
    //
    //     // Build the parent project name.
    //     ProjectName projectName = new ProjectName(projectId);
    //
    //     // Build the filter string.
    //     string filter = $"labels.{labelKey}={labelValue}";
    //     Console.WriteLine($"Searching for secrets with filter: '{filter}'...");
    //
    //     // Call the API to list secrets with the filter.
    //     var secrets = client.ListSecrets(projectName, filter);
    //
    //     int count = 0;
    //     // Iterate over each secret found.
    //     foreach (Secret secret in secrets)
    //     {
    //         count++;
    //         Console.WriteLine($"Found secret: {secret.SecretName.SecretId}");
    //
    //         // Build the secret version name for the 'latest' version.
    //         SecretVersionName secretVersionName = new SecretVersionName(
    //             secret.SecretName.ProjectId,
    //             secret.SecretName.SecretId,
    //             "latest");
    //
    //         // Access the secret version's payload.
    //         AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);
    //
    //         // Decode the payload from Base64.
    //         string payload = result.Payload.Data.ToStringUtf8();
    //
    //         Console.WriteLine($" -> Plaintext value: {payload}\n");
    //     }
    //
    //     if (count == 0)
    //     {
    //         Console.WriteLine("No secrets found matching the specified label.");
    //     }
    // }
    
    public void AccessSecretsByLabel(
        string projectId = "galvanized-app-463214-d3",
        string labelKey = "env",
        string labelValue = "dev")
    {
        // Create the client.
        SecretManagerServiceClient client = SecretManagerServiceClient.Create();

        // Build the parent project name.
        ProjectName projectName = new ProjectName(projectId);

        // Build the filter string.
        string filter = $"labels.{labelKey}={labelValue}";
        Console.WriteLine($"Searching for secrets with filter: '{filter}'...");

        // Create a request object to explicitly specify the filter.
        ListSecretsRequest request = new ListSecretsRequest
        {
            Parent = projectName.ToString(),
            Filter = filter
        };

        // Call the API to list secrets with the correctly formed request.
        var secrets = client.ListSecrets(request);

        int count = 0;
        // Iterate over each secret found.
        foreach (Secret secret in secrets)
        {
            count++;
            Console.WriteLine($"Found secret: {secret.SecretName.SecretId}");

            // Build the secret version name for the 'latest' version.
            SecretVersionName secretVersionName = new SecretVersionName(
                secret.SecretName.ProjectId,
                secret.SecretName.SecretId,
                "latest");

            // Access the secret version's payload.
            AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);

            // Decode the payload from Base64.
            string payload = result.Payload.Data.ToStringUtf8();

            Console.WriteLine($" -> Plaintext value: {payload}\n");
        }

        if (count == 0)
        {
            Console.WriteLine("No secrets found matching the specified label.");
        }
    }
}