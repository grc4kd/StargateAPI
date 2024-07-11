using System.Net;
using Dapper;
using MediatR;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Queries
{
    public class GetAstronautDutiesByName : IRequest<GetAstronautDutiesByNameResult>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class GetAstronautDutiesByNameHandler : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context;

        public GetAstronautDutiesByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
            var parameters = new { request.Name };
            var query = @"SELECT a.Id as PersonId, a.Name, b.CurrentRank, b.CurrentDutyTitle, b.CareerStartDate, b.CareerEndDate 
            FROM [Person] a INNER JOIN [AstronautDetail] b on b.PersonId = a.Id WHERE @Name = a.Name";

            var person = await _context.Connection.QueryFirstOrDefaultAsync<PersonAstronaut>(query, parameters);

            if (person == null)
            {
                return new GetAstronautDutiesByNameResult
                {
                    Success = false,
                    Message = $"Requested Person: {request.Name} does not have any AstronautDetail records on file.",
                    ResponseCode = (int)HttpStatusCode.NotFound
                };
            }

            var result = new GetAstronautDutiesByNameResult {
                Person = person
            };

            query = $"SELECT * FROM [AstronautDuty] WHERE @PersonId = PersonId Order By DutyStartDate Desc";

            var duties = await _context.Connection.QueryAsync<AstronautDuty>(query, new { person.PersonId });

            result.AstronautDuties = duties.ToList();

            return result;

        }
    }

    public class GetAstronautDutiesByNameResult : BaseResponse
    {
        public PersonAstronaut Person { get; set; } = null!;
        public List<AstronautDuty> AstronautDuties { get; set; } = [];
    }
}
