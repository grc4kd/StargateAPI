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
            new() {
                AstronautDetail = new AstronautDetail
                {
                    CareerStartDate = DateTime.FromFileTimeUtc(617520204000000000),
                    CareerEndDate = DateTime.FromFileTimeUtc(620799084000000000),
                    CurrentDutyTitle = "RETIRED",
                    CurrentRank = "Colonel",
                    Id = 1
                },
                AstronautDuties = [
                    new AstronautDuty {
                        DutyStartDate = new DateTime(1960, 3, 15),
                        DutyEndDate = new DateTime(1961, 12, 20),
                        DutyTitle = "Pilot",
                        Id = 10,
                        Rank = "Senior Lieutenant"
                    }
                ],
                Name = "Yuri"
            }
        }.ToFrozenSet();
    }
}