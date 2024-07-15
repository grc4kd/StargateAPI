using StargateAPI.Business.Data;
using StargateApiTests.Data;

namespace StargateApiTests.Helpers;

public static class DbInitializer
{
    public static void InitializeDbForTests(StargateContext context)
    {
        context.AddRange(DbSeedData.GetFullData());
        context.SaveChanges();
    }

    public static void ReinitializeDbForTests(StargateContext context)
    {
        context.RemoveRange(context.People);
        context.SaveChanges();

        context.AddRange(DbSeedData.GetFullData());
        context.SaveChanges();
    }
}
