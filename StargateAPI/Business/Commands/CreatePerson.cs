using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Business.Responses;
using StargateAPI.Exceptions;

namespace StargateAPI.Business.Commands
{
    public class CreatePerson : IRequest<CreatePersonResponse>
    {
        public required string Name { get; set; } = string.Empty;
    }

    public class CreatePersonPreProcessor(StargateContext context) : IRequestPreProcessor<CreatePerson>
    {
        private readonly StargateContext _context = context;

        public async Task Process(CreatePerson request, CancellationToken cancellationToken)
        {
            var extantPerson = await _context.People
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == request.Name, cancellationToken: cancellationToken);

            if (extantPerson != null)
            {
                throw new HttpResponseException(new NameNotUniqueResponse(extantPerson.Name));
            }
        }
    }

    public class CreatePersonHandler(StargateContext context) : IRequestHandler<CreatePerson, CreatePersonResponse>
    {
        private readonly StargateContext _context = context;

        public async Task<CreatePersonResponse> Handle(CreatePerson request, CancellationToken cancellationToken)
        {
            var newPerson = new Person()
            {
                Name = request.Name
            };

            await _context.People.AddAsync(newPerson, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            return new CreatePersonResponse(newPerson.Id);

        }
    }
}
