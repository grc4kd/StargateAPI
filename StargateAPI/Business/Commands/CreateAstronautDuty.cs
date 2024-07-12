using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using StargateAPI.Exceptions;
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

            if (person is null)
            {
                throw new HttpResponseException(new CreateAstronautDutyResult
                {
                    Id = 0,
                    Message = "Person Not Found",
                    ResponseCode = (int)HttpStatusCode.NotFound,
                    Success = false
                });
            }

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
            var person = await _context.People
                .Include(p => p.AstronautDetail)
                .FirstOrDefaultAsync(p => p.Name == request.Name, cancellationToken);

            if (person == null)
            {
                return new CreateAstronautDutyResult
                {
                    Success = false,
                    Message = $"{nameof(Person)} was not found by {nameof(Person.Name)} {request.Name}",
                    ResponseCode = (int)HttpStatusCode.NotFound
                };
            }

            var currentAstronautDuty = await _context.AstronautDuties
                .Where(p => p.Id == person.Id)
                .Where(u => u.DutyStartDate <= request.DutyStartDate
                    && (u.DutyEndDate >= request.DutyStartDate || u.DutyEndDate == null))
                .SingleOrDefaultAsync(cancellationToken);

            var requestDutyStartDateOnly = request.DutyStartDate.Date;

            if (person.AstronautDetail == null)
            {
                person.AstronautDetail = new AstronautDetail()
                {
                    CareerStartDate = requestDutyStartDateOnly
                };
            }

            person.AstronautDetail.CurrentDutyTitle = request.DutyTitle;
            person.AstronautDetail.CurrentRank = request.Rank;
            
            if (currentAstronautDuty != null)
            {
                currentAstronautDuty.DutyEndDate = requestDutyStartDateOnly.AddDays(-1);
            }

            var newAstronautDuty = new AstronautDuty
                {
                    DutyStartDate = requestDutyStartDateOnly,
                    DutyEndDate = null,
                    DutyTitle = request.DutyTitle,
                    Rank = person.AstronautDetail.CurrentRank
                };
            
            person.AstronautDuties.Add(newAstronautDuty);
            
            if (request.DutyTitle == "RETIRED")
            {
                person.AstronautDetail.CareerEndDate = requestDutyStartDateOnly.AddDays(-1);
            }

            _context.People.Update(person);
            await _context.SaveChangesAsync(cancellationToken);

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
