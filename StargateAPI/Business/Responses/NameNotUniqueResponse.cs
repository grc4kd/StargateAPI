using System.Net;

namespace StargateAPI.Business.Responses
{
    public record NameNotUniqueResponse(string Name) : IBaseResponse
    {
        public string Message => $"The name {Name} already exists in the database.";

        public bool Success => false;

        public HttpStatusCode StatusCode => HttpStatusCode.Conflict;
    }
}
