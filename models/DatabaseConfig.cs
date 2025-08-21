namespace Console_gsm_poc.models;

using System.Text.Json.Serialization;

// Model for MongoDB and MySQL secrets
public class DatabaseConfig
{
    [JsonPropertyName("username")]
    public string Username { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("host")]
    public string Host { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("database")]
    public string Database { get; set; }

    public override string ToString()
    {
        return this.Username+":"+this.Password+":"+this.Host+":"+this.Port;
    }
}

// Model for Redis secret
public class RedisConfig
{
    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("host")]
    public string Host { get; set; }

    [JsonPropertyName("port")]
    public int Port { get; set; }

    public override string ToString()
    {
        return this.Password+":"+this.Host+":"+this.Port;
    }
}