using System.Net;

namespace StargateAPI.Business.Responses;

public record NameNotFoundResponse(string Name)
    : BaseResponse("The element was not found.", Success: false, HttpStatusCode.NotFound);