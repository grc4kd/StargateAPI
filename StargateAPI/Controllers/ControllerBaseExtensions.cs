using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Responses;

namespace StargateAPI.Controllers
{
    public static class ControllerBaseExtensions
    {
        public static IActionResult GetResponse(IBaseResponse response)
            => new ObjectResult(response)
            {
                StatusCode = (int)response.StatusCode
            };
    }
}