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

    public static ImmutableList<Person> GetFullData()
    {
        var people = People.ToList();

        // zip together a shallow list of one detail - one duty - one person
        for (int idx = 0; idx < AstronautDuties.Length; idx++)
        {
            people[idx].AstronautDetail = AstronautDetails[idx];
            people[idx].AstronautDuties.Add(AstronautDuties[idx]);
        }

        return people.ToImmutableList();
    }
}