using System.Net;

namespace StargateAPI.Business.Responses
{
    public record CreateAstronautDutyResponse(int Id) : IBaseResponse
    {
        public string Message => "Created astronaut duty successfully.";

        public bool Success => true;

        public HttpStatusCode StatusCode => HttpStatusCode.OK;
    }
}
