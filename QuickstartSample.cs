

using System;
using System.Text;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;
using Google.Protobuf;

namespace Console_gsm_poc;

public class QuickstartSample
{
    // public void Quickstart(string projectId = "galvanized-app-463214-d3", string secretId = "test")
    // {
    //     // Create the client.
    //     SecretManagerServiceClient client = SecretManagerServiceClient.Create();
    //
    //     // Build the parent project name.
    //     ProjectName projectName = new ProjectName(projectId);
    //
    //     Console.WriteLine($"projectName : {projectName}");
    //     
    //     Secret result = client.GetSecret("test");
    //    
    //     string data = result.ToString();
    //     Console.WriteLine($"Plaintext: {data}");
    // }
    
    public void Quickstart(string projectId = "galvanized-app-463214-d3", string secretId="my-secret")
    {
        // Create the client.
        SecretManagerServiceClient client = SecretManagerServiceClient.Create();

        // Build the secret version name.
        // The "latest" alias accesses the most recent version of the secret.
        SecretVersionName secretVersionName = new SecretVersionName(projectId, secretId, "latest");

        // Access the secret version.
        AccessSecretVersionResponse result = client.AccessSecretVersion(secretVersionName);

        // The payload is Base64-encoded, so we need to convert it to a string.
        string payload = result.Payload.Data.ToStringUtf8();

        Console.WriteLine($"Plaintext: {payload}");
    }
}