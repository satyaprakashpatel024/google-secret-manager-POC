namespace Console_gsm_poc.gsm.secrets.settings;

public interface IAppSettings
{
    int SecretManagerMaxRetryAttempts { get; }
    int SecretManagerRetryIntervalInMilliseconds { get; }
    int SecretManagerCacheExpirationInHours { get; }
}