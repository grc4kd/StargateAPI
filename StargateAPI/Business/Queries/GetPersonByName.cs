using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Responses;
using StargateAPI.Exceptions;

namespace StargateAPI.Business.Queries
{
    public record GetPersonByName(string Name) : IRequest<GetPersonByNameResponse>;

    public class GetPersonByNameHandler : IRequestHandler<GetPersonByName, GetPersonByNameResponse>
    {
        private readonly StargateContext _context;
        public GetPersonByNameHandler(StargateContext context)
        {
            _context = context;
        }

        public async Task<GetPersonByNameResponse> Handle(GetPersonByName request, CancellationToken cancellationToken)
        {
            var person = await _context.People
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.Name == request.Name, cancellationToken)
                    ?? throw new HttpResponseException(
                        new NameNotFoundResponse(request.Name)
                    );

            return new GetPersonByNameResponse(person);
        }
    }
}
