using System.Collections.Frozen;
using StargateAPI.Business.Data;

namespace StargateApiTests.Helpers;

public class DbSeeder
{
    public static void SeedDatabase(StargateContext context)
    {
        context.People.AddRange(GetSeededPeople());
        context.SaveChanges();
    }

    public static IEnumerable<Person> GetSeededPeople()
    {
        return new List<Person>{
            new() { Name = "Roger" },
            new() { Name = "Charlie" },
            new() { Name = "Fred" },
            new() { Name = "Tom" },
            new() { Name = "Richard" },
            new() { Name = "Harry" },
            new() { Name = "Mary" },
            new() { Name = "Martha" },
            new() { Name = "Sierra" },
            new() { Name = "Francine" },
        }.ToFrozenSet();
    }
}