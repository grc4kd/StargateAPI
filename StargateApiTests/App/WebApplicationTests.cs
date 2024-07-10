using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Net;
using System.Net.Http.Json;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using StargateAPI.Business.Queries;
using StargateApiTests.Fixtures;
using StargateApiTests.Helpers;
using StargateApiTests.Specifications;

namespace StargateApiTests.App;

public class WebApplicationTests(StargateWebApplicationFactory factory) : IntegrationTest, IClassFixture<StargateWebApplicationFactory>
{
    private readonly StargateWebApplicationFactory _factory = factory;

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
        var missingName = "not found";
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Person/{missingName}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_PersonByName_ReturnsPerson()
    {
        var personName = "Roger";
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/Person/{personName}");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetPersonByNameResult>();
        Assert.IsType<GetPersonByNameResult>(result);
        Assert.NotNull(result.Person);
        Assert.Equal(personName, result.Person.Name);
    }

    [Fact]
    public async Task Get_People_ReturnsGetPeopleResult()
    {
        IEnumerable<string> expectedNames = DbSeeder.GetSeededPeople().Select(p => p.Name);
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Person");

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetPeopleResult>();
        Assert.IsType<GetPeopleResult>(result);
        Assert.All(expectedNames, action: (name) =>
        {
            var _ = result.People.Any(p => p.Name == name);
        });
    }
}