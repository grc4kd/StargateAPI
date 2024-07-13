using System.Net;
using System.Net.Http.Json;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateApiTests.Specifications;
using StargateApiTests.Fixtures;
using StargateAPI.Business.Responses;
using StargateApiTests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using StargateApiTests.Data;
using Microsoft.AspNetCore.Mvc.Testing;

namespace StargateApiTests.App;

public class WebApplicationTests : IntegrationTest, IClassFixture<StargateWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly StargateWebApplicationFactory<Program> _factory;

    public WebApplicationTests(StargateWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<StargateContext>();
        db.Database.EnsureCreated();
    }

    [Theory]
    [InlineData("/Person")]
    [InlineData("/Person/John Doe")]
    [InlineData("/AstronautDuty/Yuri")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        ReinitializeDatabase();

        var response = await _client.GetAsync(url);

        response.EnsureSuccessStatusCode();
        Assert.NotNull(response.Content.Headers.ContentType);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    }

    #region Retrieve a person by name

    [Fact]
    public async Task Get_PersonByMissingName_ReturnsNotFound()
    {
        var missingName = "slartibartfast";

        var response = await _client.GetAsync($"/Person/{missingName}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetPersonByNameResponse>();
        Assert.IsType<GetPersonByNameResponse>(result);
        Assert.False(result.Success);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Null(result.Person);
    }

    [Fact]
    public async Task Get_PersonByName_ReturnsPerson()
    {
        var personName = "John Doe";

        var response = await _client.GetAsync($"/Person/{personName}");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetPersonByNameResponse>();
        Assert.IsType<GetPersonByNameResponse>(result);
        Assert.NotNull(result.Person);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(result.Success);
        Assert.Equal(personName, result.Person.Name);
    }

    #endregion

    #region Retrieve all people

    [Fact]
    public async Task Get_People_ReturnsGetPeopleResult()
    {
        IEnumerable<string> expectedNames = DbSeedData.People.Select(p => p.Name);

        var response = await _client.GetAsync("/Person");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetPeopleResponse>();
        Assert.IsType<GetPeopleResponse>(result);
        Assert.All(expectedNames, action: (name) =>
        {
            Assert.Contains(result.PersonAstronauts, p => p.Name == name);
        });
        Assert.True(result.Success);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    #endregion

    #region Add/update a person by name

    [Fact]
    public async Task Post_CreatePerson_TestNewName()
    {
        ReinitializeDatabase();

        var response = await _client.PostAsJsonAsync("/Person", DbInsertTestData.NewName);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreatePersonResponse>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotEqual(0, result.Id);
    }

    [Fact]
    public async Task Post_CreatePersonWithDuplicateName_ReturnsConflict()
    {
        ReinitializeDatabase();

        var name = DbInsertTestData.NewName;
        var response = await _client.PostAsJsonAsync("/Person", name);
        var response2 = await _client.PostAsJsonAsync("/Person", name);

        response.EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
        var result2 = await response2.Content.ReadFromJsonAsync<NameNotUniqueResponse>();
        Assert.NotNull(result2);
        Assert.False(result2.Success);
        Assert.Equal(HttpStatusCode.Conflict, result2.StatusCode);
        Assert.Equal(name, result2.Name);
    }

    #endregion

    #region Retrieve Astronaut Duty by name

    [Fact]
    public async Task Get_AstronautDutiesByName_ReturnsGetAstronautDutiesByNameResult()
    {
        var personName = "Yuri";

        var response = await _client.GetAsync($"/AstronautDuty/{personName}");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetAstronautDutiesByNameResult>();
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotEmpty(result.AstronautDuties);
    }

    [Fact]
    public async Task Get_AstronautDutiesByName_PersonWithNoAstronautDetail_ReturnsNotFound()
    {
        var personName = "Roger";

        var response = await _client.GetAsync($"/AstronautDuty/{personName}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<NameNotFoundResponse>();
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    #endregion

    #region Add an Astronaut Duty   

    [Fact]
    public async Task Post_CreateAstronautDuty_ReturnsOk()
    {
        var request = new CreateAstronautDuty
        (
            Name: "Yuri",
            Rank: "Lieutenant Colonel",
            DutyTitle: "Designer",
            DutyStartDate: new DateTime(1962, 6, 12)
        );

        var response = await _client.PostAsync("/AstronautDuty", JsonContent.Create(request));

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateAstronautDutyResponse>();
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(result.Success);
        Assert.NotEqual(0, result.Id);
    }

    [Fact]
    public async Task Post_CreateAstronautDutyForMissingPerson_ReturnsNotFound()
    {
        var personName = "test name, missing person";
        var dutyStartDate = new DateTime(1962, 6, 12);

        var request = new CreateAstronautDuty
        (
            Name: personName,
            Rank: "Private First Class",
            DutyTitle: "Engineer",
            DutyStartDate: dutyStartDate
        );

        var response = await _client.PostAsJsonAsync("/AstronautDuty", request);
        var result = await response.Content.ReadFromJsonAsync<NameNotFoundResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Message);
        Assert.False(result.Success);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task Post_CreateAstronautDutyForNewAstronaut_CreatesNewAstronautDetail()
    {
        var request = new CreateAstronautDuty
        (
            Name: "James",
            Rank: "Admiral",
            DutyTitle: "Crew Member",
            DutyStartDate: new DateTime(2021, 10, 13)
        );

        var response = await _client.PostAsJsonAsync("/AstronautDuty", request);

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<CreateAstronautDutyResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(result.Success);
        Assert.NotEqual(0, result.Id);
    }

    [Fact]
    public async Task Post_CreateAstronautDutyForRetirement_UpdatesCareerEndDate()
    {
        var request = new CreateAstronautDuty
        (
            Name: "Yuri",
            Rank: "Colonel",
            DutyTitle: "RETIRED",
            DutyStartDate: new DateTime(1968, 3, 28)
        );

        var response = await _client.PostAsJsonAsync("/AstronautDuty", request);

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreateAstronautDutyResponse>();
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(result.Success);
        Assert.NotEqual(0, result.Id);
    }

    [Fact]
    public async Task Post_CreateConsecutiveAstronautDuty_UpdatesPriorDutyAssignment()
    {
        ReinitializeDatabase();
        
        var personName = "Yuri";
        var rank = "Lieutenant Colonel";

        var request = new CreateAstronautDuty
        (
            Name: personName,
            Rank: rank,
            DutyTitle: "Designer",
            DutyStartDate: new DateTime(1962, 6, 12)
        );

        var request2 = new CreateAstronautDuty
        (
            Name: personName,
            Rank: rank,
            DutyTitle: "Pilot",
            DutyStartDate: new DateTime(1962, 7, 1)
        );

        var response = await _client.PostAsJsonAsync("/AstronautDuty", request);
        var response2 = await _client.PostAsJsonAsync("/AstronautDuty", request2);

        response.EnsureSuccessStatusCode();

        var result2 = await response2.Content.ReadFromJsonAsync<CreateAstronautDutyResponse>();
        Assert.NotNull(result2);
        Assert.True(result2.Success);
        Assert.Equal(HttpStatusCode.OK, result2.StatusCode);
    }

    [Fact]
    public async Task Post_CreateAstronautDutyWithSameStartDate_ReturnsConflict()
    {
        ReinitializeDatabase();

        var personName = "Yuri";
        var firstDutyStartDate = new DateTime(1962, 6, 12);

        var request = new CreateAstronautDuty
        (
            Name: personName,
            Rank: "Lieutenant Colonel",
            DutyTitle: "Designer",
            DutyStartDate: firstDutyStartDate
        );

        var request2 = new CreateAstronautDuty
        (
            Name: request.Name,
            Rank: "Lieutenant Colonel",
            DutyTitle: "Pilot",
            DutyStartDate: firstDutyStartDate
        );

        var response = await _client.PostAsJsonAsync("/AstronautDuty", request);
        var response2 = await _client.PostAsJsonAsync("/AstronautDuty", request2);
        var result2 = await response2.Content.ReadFromJsonAsync<AstronautDutyStartDateConflictResponse>();

        response.EnsureSuccessStatusCode();

        Assert.Equal(HttpStatusCode.Conflict, response2.StatusCode);
        Assert.NotNull(result2);
        Assert.Equal(personName, result2.Name);
        Assert.Equal(firstDutyStartDate, result2.DutyStartDate);
    }

    #endregion

    #region Log exception responses

    [Fact]
    public async Task HttpResponseExceptionFilter_HandlesException_ReturnsNotFound()
    {
        var invalidPersonName = "--'";

        var response = await _client.GetAsync($"/Person/{invalidPersonName}");

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    private void ReinitializeDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<StargateContext>();

        DbInitializer.ReinitializeDbForTests(db);
    }
}