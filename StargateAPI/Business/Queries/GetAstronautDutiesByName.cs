using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Responses;
using StargateAPI.Exceptions;

namespace StargateAPI.Business.Queries
{
    public record GetAstronautDutiesByName(string Name) : IRequest<GetAstronautDutiesByNameResult>;

    public class GetAstronautDutiesByNameHandler(StargateContext context) : IRequestHandler<GetAstronautDutiesByName, GetAstronautDutiesByNameResult>
    {
        private readonly StargateContext _context = context;

        public async Task<GetAstronautDutiesByNameResult> Handle(GetAstronautDutiesByName request, CancellationToken cancellationToken)
        {
            var query = _context.AstronautDuties
                .AsNoTracking()
                .Where(d => d.Person.Name == request.Name)
                .Select(q => new Dtos.AstronautDutyDto()
                {
                    Id = q.Id,
                    PersonId = q.PersonId,
                    Rank = q.Rank,
                    DutyTitle = q.DutyTitle,
                    DutyStartDate = q.DutyStartDate,
                    DutyEndDate = q.DutyEndDate
                });

            if (!await query.AnyAsync(cancellationToken: cancellationToken))
            {
                throw new HttpResponseException(new NameNotFoundResponse(request.Name));
            }

            return new GetAstronautDutiesByNameResult(query);
        }
    }
}
