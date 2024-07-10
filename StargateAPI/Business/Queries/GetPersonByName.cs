using System.Net;
using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetPersonByName : IRequest<GetPersonByNameResult>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResult>
    {
        private readonly StargateContext _context;
        public GetPersonByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetPersonByNameResult> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            var parameters = new { request.Name };
            var query = @"SELECT Id as PersonId, Name
            FROM [Person]
            WHERE @Name = Name 
            ORDER BY Id DESC 
            LIMIT 1";

            var person = await _context.Connection.QueryAsync<Person>(query, parameters);

            if (person.Any())
            {
                return new GetPersonByNameResult()
                {
                    Person = person?.First()
                };
            }

            return new GetPersonByNameResult
            {
                Person = null,
                Success = false,
                Message = $"Failed to locate {nameof(Person)} by {nameof(request.Name)}: {request.Name}",
                ResponseCode = (int)HttpStatusCode.NotFound
            };
        }
    }

    public class GetPersonByNameResult : BaseResponse
    {
        public Person? Person { get; set; }
    }
}
