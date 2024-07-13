using System.Net;

namespace StargateAPI.Business.Responses;

public interface IBaseResponse
{
    abstract public string Message { get; }
    abstract public bool Success { get; }
    abstract public HttpStatusCode StatusCode { get; }

    virtual void Deconstruct(out string message, out bool success, out HttpStatusCode statusCode)
    {
        message = Message;
        success = Success;
        statusCode = StatusCode;
    }
}