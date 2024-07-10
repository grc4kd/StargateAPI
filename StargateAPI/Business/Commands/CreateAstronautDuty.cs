using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using System.Net;

namespace StargateAPI.Business.Commands
{
    public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
    {
        public required string Name { get; set; }

        public required string Rank { get; set; }

        public required string DutyTitle { get; set; }

        public DateTime DutyStartDate { get; set; }
    }

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

            if (person is null) throw new BadHttpRequestException("Bad Request");

            return Task.CompletedTask;
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var parameters = new { request.Name };
            var query = $"SELECT * FROM [Person] WHERE @Name = Name";

            var person = await _context.Connection.QueryFirstOrDefaultAsync<Person>(query, parameters);

            if (person == null)
            {
                return new CreateAstronautDutyResult
                {
                    Success = false,
                    Message = $"{nameof(Person)} was not found by {nameof(Person.Name)} {request.Name}",
                    ResponseCode = (int)HttpStatusCode.NotFound
                };
            }

            query = @"SELECT * FROM [AstronautDetail] WHERE @PersonId = PersonId;";

            var astronautDetail = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDetail>(query, new { PersonId = person.Id });

            query = @"SELECT * FROM [AstronautDuty] WHERE @PersonId = PersonId 
                AND ((@DutyStartDate BETWEEN DutyStartDate AND DutyEndDate)
                    OR DutyEndDate IS NULL)
            ORDER BY DutyStartDate DESC;";

            var currentAstronautDuty = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDuty>(query, new { PersonId = person.Id, request.DutyStartDate });

            query = @"SELECT * FROM [AstronautDuty] WHERE @PersonId = PersonId
                ORDER BY DutyStartDate DESC
                LIMIT 1;";

            var priorAstronautDuty = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDuty>(query, new { PersonId = person.Id });
            
            var dutyStartDateOnly = request.DutyStartDate.Date;
            var priorDutyEndDateOnly = priorAstronautDuty?.DutyEndDate?.Date;

            if (astronautDetail == null && currentAstronautDuty == null)
            {
                astronautDetail = new AstronautDetail
                {
                    PersonId = person.Id,
                    CurrentDutyTitle = request.DutyTitle,
                    CurrentRank = request.Rank,
                    CareerStartDate = dutyStartDateOnly
                };

                if (request.DutyTitle == "RETIRED")
                {
                    astronautDetail.CareerEndDate = dutyStartDateOnly.AddDays(-1);
                }

                await _context.AstronautDetails.AddAsync(astronautDetail, cancellationToken);
            }
            
            if (astronautDetail != null && currentAstronautDuty == null)
            {
                astronautDetail.CurrentDutyTitle = request.DutyTitle;
                astronautDetail.CurrentRank = request.Rank;
                if (request.DutyTitle == "RETIRED")
                {
                    astronautDetail.CareerEndDate = dutyStartDateOnly.AddDays(-1);
                }
                _context.AstronautDetails.Update(astronautDetail);
            }

            if (currentAstronautDuty != null)
            {
                currentAstronautDuty.DutyEndDate = dutyStartDateOnly.AddDays(-1);
                _context.AstronautDuties.Update(currentAstronautDuty);
            }

            var newAstronautDuty = new AstronautDuty()
            {
                PersonId = person.Id,
                Rank = request.Rank,
                DutyTitle = request.DutyTitle,
                DutyStartDate = dutyStartDateOnly,
                DutyEndDate = null
            };

            await _context.AstronautDuties.AddAsync(newAstronautDuty);

            await _context.SaveChangesAsync();

            return new CreateAstronautDutyResult()
            {
                Id = newAstronautDuty.Id
            };
        }
    }

    public class CreateAstronautDutyResult : BaseResponse
    {
        public int? Id { get; set; }
    }
}
