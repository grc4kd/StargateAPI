using StargateAPI.Business.Data;

namespace StargateApiTests.Fixtures;

public class DbSetTestData
{
    private int nameIndex = 0;
    private static readonly string[] newNames = ["Harold", "Jeff", "Grace", "Ada"];
    public Person GetNewPerson() => new() { Name = newNames[nameIndex++] };
}