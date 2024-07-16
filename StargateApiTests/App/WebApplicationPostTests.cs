using System.Net;
using System.Net.Http.Json;
using StargateAPI.Business.Commands;
using StargateApiTests.Specifications;
using StargateApiTests.Fixtures;
using StargateAPI.Business.Responses;
using StargateApiTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace StargateApiTests.App;

[Collection("WebApplicationTests")]
public class WebApplicationPostTests : IntegrationTest, IClassFixture<StargateWebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly StargateWebApplicationFactory<Program> _factory;

    public WebApplicationPostTests(StargateWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _factory.ReinitializeDbForTests();
    }

    #region Add/update a person by name

    [Fact]
    public async Task Post_CreatePerson_TestNewName()
    {
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
}