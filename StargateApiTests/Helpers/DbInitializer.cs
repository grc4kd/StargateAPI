using StargateAPI.Business.Data;
using StargateApiTests.Data;

namespace StargateApiTests.Helpers;

public static class DbInitializer
{
    public static void InitializeDbForTests(StargateContext db)
    {
        db.AddRange(DbSeedData.GetFullData());
        db.SaveChanges();
    }

    public static void ReinitializeDbForTests(StargateContext db)
    {
        db.RemoveRange(db.People);
        db.SaveChanges();
        InitializeDbForTests(db);
    }
}
