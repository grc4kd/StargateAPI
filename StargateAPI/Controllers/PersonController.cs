using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using StargateAPI.Business.Responses;

namespace StargateAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PersonController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("")]
        public async Task<GetPeopleResponse> GetPeople()
        {
            return await _mediator.Send(new GetPeople());
        }

        [HttpGet("{name}")]
        public async Task<GetPersonByNameResponse> GetPersonByName(string name)
        {
            return await _mediator.Send(new GetPersonByName(name));
        }

        [HttpPost("")]
        public async Task<ActionResult<CreatePersonResponse>> CreatePerson([FromBody] string name)
        {
            return await _mediator.Send(new CreatePerson()
            {
                Name = name
            });
        }
    }
}