using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using StargateApiTests.Fixtures;
using StargateApiTests.Specifications;

namespace StargateApiTests.App;

public class WebApplicationTests(StargateWebApplicationFactory factory, DbSetTestData dbTestData)
    : IntegrationTest, IClassFixture<StargateWebApplicationFactory>, IClassFixture<DbSetTestData>
{
    private readonly StargateWebApplicationFactory _factory = factory;
    private readonly DbSetTestData _dbTestData = dbTestData;

    #region Retrieve a person by name

    [Theory]
    [InlineData("/Person")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }

    [Fact]
    public async Task Get_PersonByMissingName_ReturnsNotFound()
    {
        var missingName = "slartibartfast";
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Person/{missingName}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_PersonByName_ReturnsPerson()
    {
        var personName = "John Doe";
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Person/{personName}");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetPersonByNameResult>();
        Assert.IsType<GetPersonByNameResult>(result);
        Assert.NotNull(result.Person);
        Assert.Equal(personName, result.Person.Name);
    }

    #endregion

    #region Retrieve all people

    [Fact]
    public async Task Get_People_ReturnsGetPeopleResult()
    {
        IEnumerable<string> expectedNames = DbSeedData.People.Select(p => p.Name);
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Person");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetPeopleResult>();
        Assert.IsType<GetPeopleResult>(result);
        Assert.All(expectedNames, action: (name) =>
        {
            Assert.Contains(result.People, p => p.Name == name);
        });
    }

    #endregion

    #region Add/update a person by name

    [Fact]
    public async Task Post_CreatePerson_TestNewName()
    {
        var person = _dbTestData.GetNewPerson();
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/Person", JsonContent.Create(person.Name));

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreatePersonResult>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotEqual(0, result.Id);
    }

    [Fact]
    public async Task Post_CreatePerson_DuplicateNameReturnsError()
    {
        var person = _dbTestData.GetNewPerson();
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/Person", JsonContent.Create(person.Name));
        var response2 = await client.PostAsync("/Person", JsonContent.Create(person.Name));

        response.EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.InternalServerError, response2.StatusCode);
        var result2 = await response2.Content.ReadFromJsonAsync<CreatePersonResult>();
        Assert.NotNull(result2);
        Assert.False(result2.Success);
        Assert.Equal(0, result2.Id);
    }

    #endregion

    #region Retrieve Astronaut Duty by name

    [Fact]
    public async Task Get_AstronautDutiesByName_ReturnsGetAstronautDutiesByNameResult()
    {
        var personName = "Yuri";
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/AstronautDuty/{personName}");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetAstronautDutiesByNameResult>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotEmpty(result.AstronautDuties);
    }

    [Fact]
    public async Task Get_AstronautDutiesByName_NoAstronautDetail_ReturnsNotFound()
    {
        var personName = "Roger";
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/AstronautDuty/{personName}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetAstronautDutiesByNameResult>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Empty(result.AstronautDuties);
    }

    #endregion

    #region Add an Astronaut Duty   

    [Fact]
    public async Task Post_AddAstronautDuty_ReturnsOk()
    {
        var request = new CreateAstronautDuty
        {
            Name = "Yuri",
            DutyTitle = "Designer",
            Rank = "Lieutenant Colonel",
            DutyStartDate = new DateTime(1962, 6, 12)
        };

        var client = _factory.CreateClient();

        var response = await client.PostAsync("/AstronautDuty", JsonContent.Create(request));

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateAstronautDutyResult>();
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.ResponseCode);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task Post_AddAstronautDutyForMissingPerson_ReturnsNotFound()
    {
        var personName = "test name, missing person";
        var dutyStartDate = new DateTime(1962, 6, 12);

        var request = new CreateAstronautDuty
        {
            Name = personName,
            DutyTitle = "Engineer",
            Rank = "Private First Class",
            DutyStartDate = dutyStartDate
        };

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/AstronautDuty", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateAstronautDutyResult>();
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.NotFound, result.ResponseCode);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Post_AddAstronautDutyForNewAstronaut_CreatesNewAstronautDetail()
    {
        var request = new CreateAstronautDuty
        {
            Name = "James",
            DutyTitle = "Crew Member",
            Rank = "Admiral",
            DutyStartDate = new DateOnly(2021, 10, 13).ToDateTime(new TimeOnly(11, 30))
        };

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/AstronautDuty", request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateAstronautDutyResult>();
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.ResponseCode);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task Post_AddAstronautDutyForRetirement_UpdatesCareerEndDate()
    {
        var request = new CreateAstronautDuty
        {
            Name = "Yuri",
            DutyTitle = "RETIRED",
            Rank = "Colonel",
            DutyStartDate = new DateTime(1968, 3, 28)
        };

        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/AstronautDuty", request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateAstronautDutyResult>();
        Assert.NotNull(result);
        Assert.Equal((int)HttpStatusCode.OK, result.ResponseCode);
        Assert.True(result.Success);
    }

    [Fact]
    public async Task Post_AddConsecutiveAstronautDuty_UpdatesPriorDutyAssignment()
    {
        var personName = "Yuri";
        var firstDutyStartDate = new DateTime(1962, 6, 12);
        var secondDutyStartDate = new DateTime(1962, 7, 1);

        var request = new CreateAstronautDuty
        {
            Name = personName,
            DutyTitle = "Designer",
            Rank = "Lieutenant Colonel",
            DutyStartDate = firstDutyStartDate
        };

        var request2 = new CreateAstronautDuty
        {
            Name = personName,
            DutyTitle = "Pilot",
            Rank = "Lieutenant Colonel",
            DutyStartDate = secondDutyStartDate
        };

        var client = _factory.CreateClient();

        var response = await client.PostAsync("/AstronautDuty", JsonContent.Create(request));
        var response2 = await client.PostAsync("/AstronautDuty", JsonContent.Create(request2));

        response.EnsureSuccessStatusCode();

        var result2 = await response2.Content.ReadFromJsonAsync<CreateAstronautDutyResult>();
        Assert.NotNull(result2);
        Assert.True(result2.Success);
        Assert.Equal((int)HttpStatusCode.OK, result2.ResponseCode);

        await using var db = _factory.CreateContext();
        var astronautDetails = await db.AstronautDetails
            .OrderByDescending(d => d.Id)
            .FirstAsync(d => d.Person.Name == personName);
        var lastTwoAstronautDuties = db.AstronautDuties
            .Where(d => d.Person.Name == personName)
            .OrderByDescending(d => d.DutyStartDate)
            .Take(2);

        Assert.Equal(2, lastTwoAstronautDuties.Count());
        Assert.Equal(secondDutyStartDate.AddDays(-1), lastTwoAstronautDuties.Last().DutyEndDate);
    }

    #endregion

    #region Log exception responses

    [Fact]
    public async Task HttpResponseExceptionFilter_HandlesException_NotFound()
    {
        var invalidPersonName = "--'";

        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Person/{invalidPersonName}");

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion
}