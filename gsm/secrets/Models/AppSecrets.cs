namespace Console_gsm_poc.gsm.secrets.Models;

public class AppSecrets
{
    public String Key { get; set; }
    public String Value { get; set; }

    public AppSecrets()
    {
    }

    public AppSecrets(string key, string value)
    {
        Key = key;
        Value = value;
    }
}