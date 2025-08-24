namespace Console_gsm_poc.gsm.secrets.helpers;

public static class Retry
{
    
    public static void Do(Action action, TimeSpan retryInterval, int maxAttempts = 3)
    {
        Do<object>(() =>
        {
            action();
            return null;
        }, retryInterval, maxAttempts);
    }
    
    public static T Do<T>(Func<T> action, TimeSpan retryInterval, int maxAttempts = 3)
    {
        List<Exception> exceptions = new List<Exception>();
        for(int i=0; i<maxAttempts; i++)
        {
            try
            {
                if (i > 0)
                {
                    Thread.Sleep(retryInterval);
                }
                return action();
            }
            catch(Exception ex)
            {
                exceptions.Add(ex);
            }
        }
        throw new AggregateException(exceptions);
    }
}