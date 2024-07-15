using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Responses;
using StargateAPI.Exceptions;

namespace StargateAPI.Business.Queries
{
    public record GetPersonByName(string Name) : IRequest<GetPersonByNameResponse>;

    public class GetPersonByNameHandler(StargateContext context) : IRequestHandler<GetPersonByName, GetPersonByNameResponse>
    {
        private readonly StargateContext _context = context;

        public async Task<GetPersonByNameResponse> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            var person = await _context.People
                .AsNoTracking()
                .Select(p => new PersonAstronaut{
                    PersonId = p.Id,
                    Name = p.Name,
                    CareerEndDate = p.AstronautDetail!.CareerEndDate,
                    CareerStartDate = p.AstronautDetail.CareerStartDate,
                    CurrentDutyTitle = p.AstronautDetail.CurrentDutyTitle,
                    CurrentRank = p.AstronautDetail.CurrentRank
                })
                .SingleOrDefaultAsync(p => p.Name == request.Name, cancellationToken)
                    ?? throw new HttpResponseException(
                        new NameNotFoundResponse(request.Name)
                    );

            return new GetPersonByNameResponse(person);
        }
    }
}
