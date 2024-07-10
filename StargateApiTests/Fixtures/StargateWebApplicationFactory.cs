using System.Data.Common;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StargateAPI.Business.Data;
using StargateApiTests.Helpers;

namespace StargateApiTests.Fixtures;

public class StargateWebApplicationFactory
{
    private WebApplicationFactory<Program> _webApplicationFactory;
    private readonly object _lock = new();
    private bool _databaseCreated = false;
    private bool _databaseSeeded = false;

    public StargateWebApplicationFactory()
    {
        _webApplicationFactory = new WebApplicationFactory<Program>();
        _webApplicationFactory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services => 
            {
                // remove any existing DbContext services
                var dbContextDescriptor = services.Single(
                    d => d.ServiceType == typeof(DbContextOptions<StargateContext>));
                services.Remove(dbContextDescriptor);

                var dbConnectionDescriptor = services.Single(
                    d => d.ServiceType == typeof(DbConnection)
                );
                services.Remove(dbConnectionDescriptor);

                // add testing DbContext using a different database file
                services.AddDbContext<StargateContext>((container, options) =>
                    options.UseSqlite("Filename=StargateTestDatabase.db"));
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
                db.Database.Migrate();

                _databaseCreated = true;
            }            

            // reset and seed the database once
            if (!_databaseSeeded)
            {
                using var scope = _webApplicationFactory.Services.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<StargateContext>();

                // remove any data persisted by last test run
                if (db.People.Any())
                {
                    db.People.ExecuteDelete();
                }

                DbSeeder.SeedDatabase(db);

                _databaseSeeded = true;
            }
        }

        return _webApplicationFactory.CreateClient();
    }

    public IMediator GetMediator() {
        using var scope = _webApplicationFactory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IMediator>();
    }
}