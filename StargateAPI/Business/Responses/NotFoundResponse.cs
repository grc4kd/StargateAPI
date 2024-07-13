using System.Net;

namespace StargateAPI.Business.Responses;

public record NameNotFoundResponse(string Name) : IBaseResponse
{
    public string Message => "The element was not found.";

    public bool Success => false;

    public HttpStatusCode StatusCode => HttpStatusCode.NotFound;
}