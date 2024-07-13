using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Responses;

namespace StargateAPI.Business.Queries
{
    public record GetPeople : IRequest<GetPeopleResponse>;

    public class GetPeopleHandler(StargateContext context) : IRequestHandler<GetPeople, GetPeopleResponse>
    {
        public readonly StargateContext _context = context;

        public async Task<GetPeopleResponse> Handle(GetPeople request, CancellationToken cancellationToken)
        {
            return new GetPeopleResponse(await _context.People
                .AsNoTracking()
                .Include(a => a.AstronautDetail)
                .Select(pa => new PersonAstronaut
                {
                    CareerEndDate = pa.AstronautDetail!.CareerEndDate,
                    CareerStartDate = pa.AstronautDetail.CareerStartDate,
                    CurrentDutyTitle = pa.AstronautDetail.CurrentDutyTitle,
                    CurrentRank = pa.AstronautDetail.CurrentRank,
                    Name = pa.Name,
                    PersonId = pa.Id
                }).ToListAsync(cancellationToken: cancellationToken));
        }
    }
}
