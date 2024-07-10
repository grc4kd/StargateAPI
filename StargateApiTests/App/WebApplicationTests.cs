using System.Net;
using System.Net.Http.Json;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using StargateApiTests.Fixtures;
using StargateApiTests.Helpers;
using StargateApiTests.Specifications;

namespace StargateApiTests.App;

public class WebApplicationTests(StargateWebApplicationFactory factory, DbSetTestData dbTestData) 
    : IntegrationTest, IClassFixture<StargateWebApplicationFactory>, IClassFixture<DbSetTestData>
{
    private readonly StargateWebApplicationFactory _factory = factory;
    private readonly DbSetTestData _dbTestData = dbTestData;

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

    [Fact]
    public async Task Post_CreatePerson_TestNewName()
    {
        var person = _dbTestData.GetNewPerson();   
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/Person", JsonContent.Create(person.Name));

        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<CreatePersonResult>();
        Assert.IsType<CreatePersonResult>(result);
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
    }

    [Fact]
    public async Task Get_AstronautDutiesByName_ReturnsGetAstronautDutiesByNameResult()
    {
        var personName = "Yuri";
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/AstronautDuty/{personName}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_AstronautDutiesByName_NoAstronautDetail_ReturnsError()
    {
        var personName = "Roger";
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/AstronautDuty/{personName}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}