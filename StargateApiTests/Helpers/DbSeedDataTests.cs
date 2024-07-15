using StargateApiTests.Data;
using StargateApiTests.Specifications;

namespace StargateApiTests.Helpers;

public class DbDataTests : UnitTest
{
    [Fact]
    public void DataRules_SeedData_PersonUniqueIdByNameField()
    {
        var duplicates = DbSeedData.People.GroupBy(
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

    [Fact]
    public void DataRules_StaticData_PersonUniqueIdByNameField()
    {
        var names = DbSeedData.Names.Concat([DbInsertTestData.NewName]);

        var duplicates =
            from n in names
            group n by n into nameGroup
            where nameGroup.Count() > 1
            select new
            {
                nameGroup.Key,
                Count = nameGroup.Count()
            };

        Assert.Empty(duplicates);
    }

    [Fact]
    public void TestingRules_StaticData_NoDatabaseGeneratedIds()
    {
        Assert.DoesNotContain(DbSeedData.GetFullData(),
            person => person.Id > 0
            || person.AstronautDetail?.Id > 0
            || person.AstronautDuties.Any(d => d.Id > 0));
    }
}