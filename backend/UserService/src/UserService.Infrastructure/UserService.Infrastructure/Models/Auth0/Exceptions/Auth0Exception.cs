using UserService.Infrastructure.Models.Auth0.Errors;

namespace UserService.Infrastructure.Models.Auth0.Exceptions;

public class Auth0Exception : Exception
{
    public Auth0Error Error { get; }

    public Auth0Exception(string message) : base(message)
    {
        Error = Auth0Error.Parse(message);
    }

    public Auth0Exception(string message, Exception innerException) 
        : base(message, innerException)
    {
        Error = Auth0Error.Parse(message);
    }
    
    public override string Message => Error.Message ?? base.Message;
}