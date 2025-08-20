namespace Console_gsm_poc;

using Google.Cloud.SecretManager.V1;
using System;
using Google.Api.Gax.ResourceNames;

public class SecretManagerAccess
{
    /// <summary>
    /// Finds and prints the value of every secret in a project.
    /// </summary>
    /// <param name="projectId">Your Google Cloud project ID.</param>
    public void AccessAllSecrets(string projectId = "your-project-id")
    {
        // Create the client.
        SecretManagerServiceClient client = SecretManagerServiceClient.Create();

        // Build the parent project name.
        ProjectName projectName = new ProjectName(projectId);
        Console.WriteLine($"Searching for all secrets in project {projectId}...");

        // Call the API to list all secrets. No filter is needed.
        var secrets = client.ListSecrets(projectName);

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
            Console.WriteLine("No secrets found in this project.");
        }
    }
}