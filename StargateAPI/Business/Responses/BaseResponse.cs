using System.Net;

namespace StargateAPI.Business.Responses;

public record BaseResponse(string Message, bool Success, HttpStatusCode StatusCode) : IBaseResponse;

