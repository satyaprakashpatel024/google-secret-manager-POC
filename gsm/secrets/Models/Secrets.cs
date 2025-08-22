namespace Console_gsm_poc.gsm.secrets.Models;

public class Secrets
{
    public String Key { get; set; }
    public String Value { get; set; }

    public Secrets(string key, string value)
    {
        Key = key;
        Value = value;
    }
}