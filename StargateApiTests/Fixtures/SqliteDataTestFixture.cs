using System.Data.Common;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using StargateApiTests.Data;

namespace StargateApiTests.Fixtures;

public class SqliteDataTestFixture
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<StargateContext> _contextOptions;

    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;

    public SqliteDataTestFixture()
    {
        // create and open a connection in memory, persisting until the connection is disposed
        _connection = new SqliteConnection("Filename=SqliteDataTests.db");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<StargateContext>()
            .UseSqlite(_connection)
            .Options;

        using (var context = new StargateContext(_contextOptions))
        {
            context.Database.EnsureCreated();

            context.People.RemoveRange(context.People);
            context.SaveChanges();

            context.AddRange(DbSeedData.GetFullData());
            context.SaveChanges();    
        }

        var services = new ServiceCollection();
        services.AddDbContext<StargateContext>(options =>
            options.UseSqlite(_connection));
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly);
            cfg.AddRequestPreProcessor<CreatePersonPreProcessor>();
            cfg.AddRequestPreProcessor<CreateAstronautDutyPreProcessor>();
        });
        _serviceProvider = services.BuildServiceProvider();
        _mediator = _serviceProvider.GetRequiredService<IMediator>();
    }

    public StargateContext CreateContext() => new StargateContext(_contextOptions);
    public PersonController CreatePersonController() => new PersonController(_mediator);
    public AstronautDutyController CreateAstronautDutyController() => new AstronautDutyController(_mediator);
}