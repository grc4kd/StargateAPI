using System.Net;
using Dapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;
using StargateAPI.Exceptions;

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

            if (!person.Any())
            {
                throw new HttpResponseException(new GetPersonByNameResult
                {
                    Person = null,
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Message = "Person not found.",
                    Success = false
                })
                ;
            }

            return new GetPersonByNameResult()
            {
                Person = person?.First()
            };
        }
    }

    public class GetPersonByNameResult : BaseResponse
    {
        public Person? Person { get; set; }
    }
}
