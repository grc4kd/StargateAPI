using System.Net;
using System.Net.Http.Json;
using StargateApiTests.Specifications;
using StargateApiTests.Fixtures;
using StargateAPI.Business.Responses;
using StargateApiTests.Data;
using Microsoft.AspNetCore.Mvc.Testing;

namespace StargateApiTests.App;

[Collection("WebApplicationTests")]
public class WebApplicationGetTests : IntegrationTest, IClassFixture<StargateWebApplicationFactory<Program>>, IDisposable
{
    private readonly HttpClient _client;
    private readonly StargateWebApplicationFactory<Program> _factory;

    public WebApplicationGetTests(StargateWebApplicationFactory<Program> factory)
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

    [Theory]
    [InlineData("/Person")]
    [InlineData("/Person/John Doe")]
    [InlineData("/AstronautDuty/Yuri")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
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
        Assert.Null(result.PersonAstronaut);
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
        Assert.NotNull(result.PersonAstronaut);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(result.Success);
        Assert.Equal(personName, result.PersonAstronaut.Name);
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
        Assert.Null(result.AstronautDuties.First().Person);
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

}