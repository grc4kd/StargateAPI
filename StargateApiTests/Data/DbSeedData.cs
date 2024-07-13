using System.Collections.Frozen;
using System.Collections.Immutable;
using StargateAPI.Business.Data;

namespace StargateApiTests.Data;

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
            "Yuri",
            "John Doe",
            "Jane Doe",
            "Roger",
            "Charlie",
            "Fred",
            "Tom", "Richard", "Harry",
            "Mary",
            "Martha",
            "Sierra",
            "Francine",
            "James"
        }.ToFrozenSet();

    public static ImmutableArray<AstronautDetail> AstronautDetails { get; } = [
        new AstronautDetail
        {
            CareerStartDate = DateTime.Now,
            CareerEndDate = null,
            CurrentRank = "1LT",
            CurrentDutyTitle = "Commander"
        },
        new AstronautDetail
        {
            CareerStartDate = new DateTime(1957, 5, 11),
            CareerEndDate = null,
            CurrentRank = "Senior Lieutenant",
            CurrentDutyTitle = "Pilot"
        }
    ];

    public static ImmutableArray<AstronautDuty> AstronautDuties { get; } = [
        new AstronautDuty
        {
            DutyStartDate = DateTime.Now,
            DutyEndDate = null,
            DutyTitle = "Commander",
            Rank = "1LT"
        },
        new AstronautDuty
        {
            DutyStartDate = new DateTime(1960, 3, 15),
            DutyEndDate = null,
            DutyTitle = "Pilot",
            Rank = "Senior Lieutenant"
        }
    ];

    public static IEnumerable<Person> GetFullData()
    {
        var people = People.ToList();
        int idx = 0;

        foreach (var detail in AstronautDetails)
        {
            people.ElementAt(idx).AstronautDetail = detail;
            people.ElementAt(idx).AstronautDuties.Add(AstronautDuties.ElementAt(idx));
        }

        return people;
    }
}