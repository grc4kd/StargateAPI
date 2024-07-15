using System.Collections.Immutable;
using System.Data.Common;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using StargateApiTests.Helpers;
using StargateApiTests.Specifications;

namespace StargateApiTests.Data;

public class SqliteDataTests : IntegrationTest, IDisposable
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<StargateContext> _contextOptions;

    private readonly IServiceProvider _serviceProvider;
    private readonly IMediator _mediator;

    public SqliteDataTests()
    {
        // create and open a connection in memory, persisting until the connection is disposed
        _connection = new SqliteConnection("Filename=:memory:");
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

    StargateContext CreateContext() => new(_contextOptions);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _connection.Dispose();
    }

    [Fact]
    public async Task Person_GetPeople_TestPersistedData()
    {
        using var context = CreateContext();
        var controller = new PersonController(_mediator);
        var getPeopleResponse = await controller.GetPeople();
        var entities = await context.People.ToListAsync();

        Assert.True(getPeopleResponse.Success);
        Assert.NotEmpty(entities);
        Assert.Equal(DbSeedData.People.Count(), entities.Count);
        // test database entity fields against seed data in any order
        var seedSet = DbSeedData.Names.ToImmutableHashSet();
        var dbSet = entities.Select(p => p.Name).ToImmutableHashSet();
        Assert.Equivalent(seedSet, dbSet);
    }

    [Fact]
    public async Task Person_GetPersonByName_TestPersistedData()
    {
        using var context = CreateContext();
        var controller = new PersonController(_mediator);
        var name = DbSeedData.Names.First();

        var getPersonResponse = await controller.GetPersonByName(name);
        var entity = await context.People.SingleAsync(p => p.Name == name);

        Assert.True(getPersonResponse.Success);
        Assert.NotNull(entity);
        Assert.NotEqual(0, entity.Id);
        Assert.Equal(name, entity.Name);
        Assert.NotNull(entity.AstronautDuties);
    }

    [Fact]
    public async Task Person_CreatePerson_TestPersistedData()
    {
        using var context = CreateContext();
        var controller = new PersonController(_mediator);
        // rollback transaction at end of test, prevent interference with other test methods
        context.Database.BeginTransaction();

        var name = DbInsertTestData.NewName;

        var createPersonResponse = await controller.CreatePerson(name);

        // clear the change tracker to simulate a commit of data to the target database
        context.ChangeTracker.Clear();

        var entity = context.People.Single(p => p.Name == name);

        Assert.True(createPersonResponse.Success);
        Assert.NotNull(entity);
        Assert.NotEqual(0, entity.Id);
        Assert.Equal(name, entity.Name);
        Assert.Null(entity.AstronautDetail);
        Assert.Empty(entity.AstronautDuties);
    }

    [Fact]
    public async Task AstronautDuty_GetAstronautDutiesByName_TestPersistedData()
    {
        using var context = CreateContext();
        var controller = new AstronautDutyController(_mediator);
        var personWithDuty = DbSeedData.GetFullData().First(d => d.AstronautDuties.Count != 0);

        var response = await controller.GetAstronautDutiesByName(personWithDuty.Name);
        var entities = await context.AstronautDuties.AsNoTracking()
            .Include(d => d.Person)
            .Where(d => d.Person.Name == personWithDuty.Name).ToListAsync();

        Assert.True(response.Success);
        Assert.NotEmpty(entities);
        Assert.Equal(personWithDuty.AstronautDuties.Count, entities.Count);
        // order the sample and actual sets, then test the first element for value equality
        var seedSet = personWithDuty.AstronautDuties
            .OrderBy(p => p.PersonId)
            .ThenBy(d => d.DutyStartDate)
            .ToImmutableHashSet();
        var dbSet = entities
            .OrderBy(d => d.PersonId)
            .ThenBy(d => d.DutyStartDate)
            .ToImmutableHashSet();
        Assert.NotEqual(0, dbSet.First().Id);
        Assert.Equal(seedSet.First().DutyTitle, dbSet.First().DutyTitle);
        Assert.Equal(seedSet.First().Rank, dbSet.First().Rank);
        Assert.Equal(seedSet.First().DutyStartDate, dbSet.First().DutyStartDate);
        Assert.Equal(seedSet.First().DutyEndDate, dbSet.First().DutyEndDate);
        Assert.NotEqual(0, dbSet.First().PersonId);
        Assert.Equal(seedSet.First().Person.Name, dbSet.First().Person.Name);
    }

    [Fact]
    public async Task AstronautDuty_CreateAstronautDuty_TestPersistedData()
    {
        using var context = CreateContext();
        var controller = new AstronautDutyController(_mediator);
        var name = DbSeedData.Names.First();
        var request = new CreateAstronautDuty(name, Rank: "Rank 1", DutyTitle: "Duty I", new DateTime(2023, 2, 1));

        var createAstronautDutyResponse = await controller.CreateAstronautDuty(request);

        var entity = context.AstronautDuties.AsNoTracking()
            .Include(d => d.Person)
            .Single(d => d.Person.Name == name && d.DutyStartDate == request.DutyStartDate);

        Assert.True(createAstronautDutyResponse.Success);
        Assert.NotNull(entity);
        Assert.NotEqual(0, entity.Id);
        Assert.Equal(name, entity.Person.Name);
        Assert.Equal(request.Rank, entity.Rank);
        Assert.Equal(request.DutyTitle, entity.DutyTitle);
        Assert.Equal(request.DutyStartDate, entity.DutyStartDate);
        Assert.Null(entity.DutyEndDate);
    }
}