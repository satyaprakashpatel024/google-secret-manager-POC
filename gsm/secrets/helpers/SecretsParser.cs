using System.Text.Json;
using Console_gsm_poc.gsm.secrets.Models;
using Console_gsm_poc.helper;

namespace Console_gsm_poc.gsm.secrets.helpers;

public class SecretsParser
{
    public static List<Secrets> ParseJsonSecrets(Dictionary<string, SecretContent> allSecrets)
    {
        var secretsList = new List<Secrets>();

        foreach (var kvp in allSecrets)
        {
            if (kvp.Value.IsJson && kvp.Value.JsonDocument != null)
            {
                try
                {
                    var flattenedProperties = FlattenJsonElement(kvp.Value.JsonDocument.RootElement, kvp.Key);
                    secretsList.AddRange(flattenedProperties);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing JSON secret {kvp.Key}: {ex.Message}");
                    // Add as raw JSON if parsing fails
                    secretsList.Add(new Secrets (
                    
                        kvp.Key, 
                        kvp.Value.AsString() 
                    ));
                }
            }
        }

        return secretsList;
    }
    
    public static List<Secrets> ParsePlainTextSecrets(Dictionary<string, SecretContent> allSecrets)
    {
        var secretsList = new List<Secrets>();

        foreach (var kvp in allSecrets)
        {
            if (!kvp.Value.IsJson)
            {
                secretsList.Add(new Secrets (
                    kvp.Key, 
                    kvp.Value.PlainText 
                ));
            }
        }

        return secretsList;
    }

    /// <summary>
    /// Alternative: Parse JSON secrets but keep them as single entries (not flattened)
    /// </summary>
    /// <param name="allSecrets">Dictionary containing secret names and their content</param>
    /// <returns>List of Secrets with complete JSON as values</returns>
    public static List<Secrets> ParseJsonSecretsAsWhole(Dictionary<string, SecretContent> allSecrets)
    {
        var secretsList = new List<Secrets>();

        foreach (var kvp in allSecrets)
        {
            if (kvp.Value.IsJson)
            {
                secretsList.Add(new Secrets 
                (
                    kvp.Key, 
                    kvp.Value.AsString() 
                ));
            }
        }

        return secretsList;
    }
    
    private static List<Secrets> FlattenJsonElement(JsonElement element, string prefix = "")
    {
        var secrets = new List<Secrets>();

        switch (element.ValueKind)
        {

            case JsonValueKind.String:
                secrets.Add(new Secrets(prefix,element.GetString()));
                break;

            case JsonValueKind.Number:
                secrets.Add(new Secrets(prefix,element.GetString()));
                break;

            case JsonValueKind.True:
                secrets.Add(new Secrets(prefix,element.GetString()));
                break;

            case JsonValueKind.False:
                secrets.Add(new Secrets(prefix,element.GetString()));
                break;

            case JsonValueKind.Null:
                secrets.Add(new Secrets(prefix,element.GetString()));
                break;
        }

        return secrets;
    }
}