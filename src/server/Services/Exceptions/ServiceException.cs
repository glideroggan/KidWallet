namespace server.Services.Exceptions;

public class ServiceException : Exception
{
    public ServiceException(string msg) : base(msg) {}
}