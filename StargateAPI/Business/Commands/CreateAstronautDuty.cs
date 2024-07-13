using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Responses;
using StargateAPI.Exceptions;

namespace StargateAPI.Business.Commands
{
    public record CreateAstronautDuty(string Name, string Rank, string DutyTitle, DateTime DutyStartDate)
        : IRequest<CreateAstronautDutyResponse>;

    public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyPreProcessor(StargateContext context)
        {
            _context = context;
        }

        public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_context.People
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Name == request.Name, cancellationToken: cancellationToken)
                ?? throw new HttpResponseException(
                    new NameNotFoundResponse(request.Name)
                ));
        }
    }

    public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResponse>
    {
        private readonly StargateContext _context;

        public CreateAstronautDutyHandler(StargateContext context)
        {
            _context = context;
        }
        public async Task<CreateAstronautDutyResponse> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
        {
            var person = await _context.People
                .Include(p => p.AstronautDetail)
                .Where(p => p.Name == request.Name)
                .SingleOrDefaultAsync(cancellationToken: cancellationToken);

            if (person == null)
            {
                throw new HttpResponseException(new NameNotFoundResponse(request.Name));
            }

            var currentAstronautDuty = await _context.AstronautDuties
                .Where(d => d.PersonId == person.Id)
                .SingleOrDefaultAsync(d => d.DutyStartDate <= request.DutyStartDate
                    && (d.DutyEndDate >= request.DutyStartDate || d.DutyEndDate == null), cancellationToken);

            if (currentAstronautDuty?.DutyStartDate.Date == request.DutyStartDate.Date)
            {
                throw new HttpResponseException(
                    new AstronautDutyStartDateConflictResponse(currentAstronautDuty.Person.Name, currentAstronautDuty.DutyStartDate));
            }

            var requestDutyStartDateOnly = request.DutyStartDate.Date;

            if (person.AstronautDetail == null)
            {
                person.AstronautDetail = new AstronautDetail
                {
                    CareerStartDate = requestDutyStartDateOnly
                };
            };

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

            return new CreateAstronautDutyResponse(newAstronautDuty.Id);
        }
    }
}
