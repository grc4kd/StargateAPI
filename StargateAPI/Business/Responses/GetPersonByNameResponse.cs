using System.Net;
using StargateAPI.Business.Dtos;

namespace StargateAPI.Business.Responses;

public record GetPersonByNameResponse(PersonAstronautDto PersonAstronaut)
    : BaseResponse("Got person by name successfully.", Success: true, HttpStatusCode.OK);
