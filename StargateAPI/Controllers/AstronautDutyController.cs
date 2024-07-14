using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StargateAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AstronautDutyController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("{name}")]
        public async Task<GetAstronautDutiesByNameResult> GetAstronautDutiesByName(string name)
        {
            return await _mediator.Send(new GetAstronautDutiesByName(name));
        }

        [HttpPost]
        public async Task<CreateAstronautDutyResponse> CreateAstronautDuty([FromBody] CreateAstronautDuty request)
        {
            return await _mediator.Send(request);
        }
    }
}