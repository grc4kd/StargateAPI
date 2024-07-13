using System.Net;

namespace StargateAPI.Business.Responses
{
    public record CreatePersonResponse(int Id) : BaseResponse("Created person successfully.", Success: true, HttpStatusCode.OK);
}
