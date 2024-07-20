using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StargateAPI.Business.Data;
using StargateApiTests.Data;

namespace StargateApiTests.Fixtures;

public class StargateWebApplicationPostFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class
{
    private readonly DbConnection _connection;
    private readonly DbContextOptions<StargateContext> _contextOptions;
    private static readonly object _lock = new();

    public StargateWebApplicationPostFactory()
    {
        lock (_lock)
        {
            // Create open SqliteConnection so EF won't automatically close it.
            _connection = new SqliteConnection($"Filename={nameof(StargateWebApplicationPostFactory<StargateContext>)}.test.db");
            _connection.Open();

            _contextOptions = new DbContextOptionsBuilder<StargateContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new StargateContext(_contextOptions);

            lock (_lock)
            {
                if (context.Database.EnsureCreated())
                {
                    context.AddRange(DbSeedData.GetFullData());
                    context.SaveChanges();
                }
            }
        }
    }

    public async Task ReinitializeDbForTestsAsync()
    {
        using var context = new StargateContext(_contextOptions);

        context.People.RemoveRange(context.People);
        await context.SaveChangesAsync();

        await context.AddRangeAsync(DbSeedData.GetFullData());
        await context.SaveChangesAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // remove any existing DbContext services
            var dbContextDescriptor = services.Single(
                d => d.ServiceType == typeof(DbContextOptions<StargateContext>));
            services.Remove(dbContextDescriptor);

            var dbConnectionDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbConnection)
            );

            if (dbConnectionDescriptor != null)
            {
                services.Remove(dbConnectionDescriptor);
            }

            services.AddSingleton(_connection);

            services.AddDbContext<StargateContext>((container, options) =>
            {
                var connection = container.GetRequiredService<DbConnection>();
                options.UseSqlite(connection);
            });
        });

        builder.UseEnvironment("Development");
    }
}