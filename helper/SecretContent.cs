using System.Text.Json;

namespace Console_gsm_poc.helper;

public class SecretContent : IDisposable
{
    public bool IsJson { get; set; }
    public JsonDocument JsonDocument { get; set; }
    public string PlainText { get; set; }

    public void Dispose()
    {
        JsonDocument?.Dispose();
    }
    
    public string AsString()
    {
        return IsJson ? JsonDocument?.RootElement.GetRawText() : PlainText;
    }

    public string GetJsonProperty(string propertyPath)
    {
        if (!IsJson || JsonDocument == null) return null;

        var pathParts = propertyPath.Split('.');
        JsonElement current = JsonDocument.RootElement;

        foreach (var part in pathParts)
        {
            if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(part, out var property))
            {
                current = property;
            }
            else
            {
                return null;
            }
        }

        return current.ValueKind switch
        {
            JsonValueKind.String => current.GetString(),
            JsonValueKind.Number => current.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => null,
            _ => current.GetRawText()
        };
    }
}

