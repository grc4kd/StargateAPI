using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;
using StargateApiTests.Fixtures;
using StargateApiTests.Helpers;
using StargateApiTests.Specifications;

namespace StargateApiTests.Data;

public class SqliteDataTests : IntegrationTest, IClassFixture<SqliteDataTestFixture>, IDisposable
{
    private readonly SqliteDataTestFixture _fixture;
    private readonly StargateContext _context;
    private readonly PersonController _personController;
    private readonly AstronautDutyController _astronautDutyController;

    public SqliteDataTests(SqliteDataTestFixture fixture)
    {
        _fixture = fixture;
        _context = _fixture.CreateContext();
        _personController = _fixture.CreatePersonController();
        _astronautDutyController = fixture.CreateAstronautDutyController();
        DbInitializer.ReinitializeDbForTests(_context);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _context.Dispose();
    }

    [Fact]
    public async Task Person_GetPeople_TestPersistedData()
    {
        var getPeopleResponse = await _personController.GetPeople();
        var entities = await _context.People.ToListAsync();

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
        var name = DbSeedData.Names.First();

        var getPersonResponse = await _personController.GetPersonByName(name);
        var entity = await _context.People.SingleAsync(p => p.Name == name);

        Assert.True(getPersonResponse.Success);
        Assert.NotNull(entity);
        Assert.NotEqual(0, entity.Id);
        Assert.Equal(name, entity.Name);
        Assert.NotNull(entity.AstronautDuties);
    }

    [Fact]
    public async Task Person_CreatePerson_TestPersistedData()
    {
        var name = DbInsertTestData.NewName;

        var createPersonResponse = await _personController.CreatePerson(name);

        var entity = _context.People.Single(p => p.Name == name);

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
        var personWithDuty = DbSeedData.GetFullData().First(d => d.AstronautDuties.Count != 0);

        var response = await _astronautDutyController.GetAstronautDutiesByName(personWithDuty.Name);
        var entities = await _context.AstronautDuties.AsNoTracking()
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
        var name = DbSeedData.Names.First();
        var request = new CreateAstronautDuty(name, Rank: "Rank 1", DutyTitle: "Duty I", new DateTime(2023, 2, 1));

        var createAstronautDutyResponse = await _astronautDutyController.CreateAstronautDuty(request);

        var entity = _context.AstronautDuties.AsNoTracking()
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