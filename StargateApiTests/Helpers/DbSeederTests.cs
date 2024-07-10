using StargateApiTests.Specifications;

namespace StargateApiTests.Helpers;

public class DbSeederTests : UnitTest
{
    [Fact]
    public void DataRules_SeedData_PersonUniqueIdByNameField()
    {
        var duplicates = DbSeeder.GetSeededPeople().GroupBy(
                person => person.Name,
                person => person,
                (name, personGroup) => new
                {
                    Key = name,
                    Count = personGroup.Count()
                })
                .Where(g => g.Count > 1);

        Assert.Empty(duplicates);
    }
}