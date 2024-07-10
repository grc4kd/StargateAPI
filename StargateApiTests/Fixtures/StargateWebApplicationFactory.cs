using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StargateAPI.Business.Data;

namespace StargateApiTests.Fixtures;

public class StargateWebApplicationFactory
{
    private readonly string _connectionString = "Filename=StargateTestDatabase.db";
    private readonly WebApplicationFactory<Program> _webApplicationFactory;
    private readonly object _lock = new();
    private bool _databaseCreated = false;

    public StargateWebApplicationFactory()
    {
        _webApplicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
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

                    // add testing DbContext using a different database file
                    services.AddDbContext<StargateContext>((container, options) =>
                        options.UseSqlite(_connectionString));
                });

                builder.UseEnvironment("Development");
            });
    }

    public HttpClient CreateClient()
    {
        // lock test runner to create client thread until the database has been created once
        lock (_lock)
        {
            // run database migrations the first time creating client during each test run
            if (!_databaseCreated)
            {
                using var scope = _webApplicationFactory.Services.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<StargateContext>();

                // EnsureCreated and Migrations don't work well together.
                // Use EnsureDeleted to drop the database between test runs.
                db.Database.EnsureDeleted();
        
                // Then programmatically apply migrations to the test database, includes seed data from context class
                db.Database.Migrate();

                _databaseCreated = true;
            }
        }

        return _webApplicationFactory.CreateClient();
    }

    public StargateContext CreateContext() => 
        new(new DbContextOptionsBuilder<StargateContext>()
            .UseSqlite(_connectionString)
            .Options);
}