using System.Net;

namespace StargateAPI.Business.Responses
{
    public record AstronautDutyStartDateConflictResponse(string Name, DateTime DutyStartDate) : IBaseResponse
    {
        public string Message => $"{Name} already has a duty with the same start date {DutyStartDate.ToShortDateString()}.";

        public bool Success => false;

        public HttpStatusCode StatusCode => HttpStatusCode.Conflict;
    }
}
