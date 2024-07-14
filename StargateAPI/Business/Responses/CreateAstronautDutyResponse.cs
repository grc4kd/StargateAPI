using System.Net;

namespace StargateAPI.Business.Responses;

public record CreateAstronautDutyResponse(int Id) 
    : BaseResponse("Created astronaut duty successfully.", Success: true, HttpStatusCode.OK);
