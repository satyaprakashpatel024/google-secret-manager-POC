using System.Runtime.Serialization;

namespace Console_gsm_poc.gsm.secrets;

public class SecretException :Exception
{
    public SecretException()
    {
    }

    protected SecretException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public SecretException(string? message) : base(message)
    {
    }

    public SecretException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}