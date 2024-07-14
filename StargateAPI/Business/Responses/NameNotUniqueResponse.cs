using System.Net;

namespace StargateAPI.Business.Responses;

public record NameNotUniqueResponse(string Name) 
    : BaseResponse($"The name {Name} already exists in the database.", Success: false, HttpStatusCode.Conflict);
