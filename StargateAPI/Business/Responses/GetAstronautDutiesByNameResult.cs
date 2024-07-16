using System.Net;
using StargateAPI.Business.Dtos;

namespace StargateAPI.Business.Responses;

public record GetAstronautDutiesByNameResult(IEnumerable<AstronautDutyDto> AstronautDuties)
    : BaseResponse("Got astronaut duties by name successfully.", Success: true, HttpStatusCode.OK);
