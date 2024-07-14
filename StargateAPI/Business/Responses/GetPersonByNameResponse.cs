using System.Net;
using StargateAPI.Business.Data;

namespace StargateAPI.Business.Responses;

public record GetPersonByNameResponse(Person Person)
    : BaseResponse("Got person by name successfully.", Success: true, HttpStatusCode.OK);
