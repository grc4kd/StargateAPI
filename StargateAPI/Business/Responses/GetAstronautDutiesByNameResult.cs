using System.Net;
using StargateAPI.Business.Data;

namespace StargateAPI.Business.Responses;

public record GetAstronautDutiesByNameResult(IEnumerable<AstronautDuty> AstronautDuties)
    : BaseResponse("Got astronaut duties by name successfully.", Success: true, HttpStatusCode.OK);
