using System.Net;
using StargateAPI.Business.Dtos;

namespace StargateAPI.Business.Responses
{
    public record GetPeopleResponse(IEnumerable<PersonAstronaut> PersonAstronauts)
        : BaseResponse("Got people successfully.", Success: true, HttpStatusCode.OK);
}
