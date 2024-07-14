using System.Net;

namespace StargateAPI.Business.Responses;

public record AstronautDutyStartDateConflictResponse(string Name, DateTime DutyStartDate)
    : BaseResponse($"{Name} already has a duty with the same start date {DutyStartDate.ToShortDateString()}.",
        Success: false, HttpStatusCode.Conflict);
