using System.Collections.Frozen;

namespace StargateAPI.Business.Data;

public class DbSeedData
{
    public static IEnumerable<Person> People 
        => from idx in Enumerable.Range(0, Names.Count)
        select new Person
        {
            Id = idx + 1,
            Name = Names.ElementAt(idx)
        };

    public static ISet<string> Names => new List<string> {
            "John Doe",
            "Jane Doe",
            "Yuri",
            "Roger",
            "Charlie",
            "Fred",
            "Tom", "Richard", "Harry",
            "Mary",
            "Martha",
            "Sierra",
            "Francine"
        }.ToFrozenSet();
}