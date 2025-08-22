using Microsoft.Extensions.Configuration;

namespace Console_gsm_poc.gsm.secrets.settings;

public class AppSettings : IAppSettings
{
    private readonly IConfiguration _configuration;
    private static readonly int DefaultMaxRetryAttempts = 3;
    private static readonly int DefaultRetryInterval = 1000;
    private static readonly int DefaultCacheExpiration = 1;
    
    public AppSettings(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public int SecretManagerMaxRetryAttempts
    {
        get
        {
            string value = _configuration["GcpSettings:SecretManagerMaxRetryAttempts"];
            if (!int.TryParse(value, out var result))
            {
                return DefaultMaxRetryAttempts;
            }
            return result;
        }
    }
    
    public int SecretManagerRetryIntervalInMilliseconds
    {
        get
        {
            string value = _configuration["GcpSettings:SecretManagerRetryIntervalInMilliseconds"];
            if (!int.TryParse(value, out var result))
            {
                return DefaultRetryInterval; 
            }
            return result;
        }
    }
    
    public int SecretManagerCacheExpirationInHours
    {
        get
        {
            string value = _configuration["GcpSettings:SecretManagerCacheExpirationInHours"];
            if (!int.TryParse(value, out var result))
            {
                return DefaultCacheExpiration;
            }
            return result;
        }
    }
}