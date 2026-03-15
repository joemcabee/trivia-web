using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TriviaApp.API.Data;

/// <summary>
/// Used by EF Core design-time tools (e.g. dotnet ef migrations add) to create TriviaDbContext
/// without running the full application.
/// </summary>
public class TriviaDbContextFactory : IDesignTimeDbContextFactory<TriviaDbContext>
{
    public TriviaDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..");
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<TriviaDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("SlackerDB")
            ?? throw new InvalidOperationException("Set ConnectionStrings:SlackerDB (e.g. in user-secrets).");

        var optionsBuilder = new DbContextOptionsBuilder<TriviaDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new TriviaDbContext(optionsBuilder.Options);
    }
}
