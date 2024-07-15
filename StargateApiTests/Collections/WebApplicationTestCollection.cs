using StargateApiTests.Fixtures;

namespace StargateApiTests.Collections;

[CollectionDefinition("WebApplicationTests")]
public class WebApplicationTestCollection : ICollectionFixture<StargateWebApplicationFactory<Program>>
{
}