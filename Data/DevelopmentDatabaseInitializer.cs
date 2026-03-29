using DotNetCoreSqlDb.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreSqlDb.Data;

public static class DevelopmentDatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        var connectionString = DevelopmentConnectionStringSelector.GetDevelopmentConnectionString(configuration);

        if (!DevelopmentConnectionStringSelector.IsLocalConnectionString(connectionString))
        {
            logger.LogWarning(
                "Skipping development database migration and seed because the configured connection string is not local.");
            return;
        }

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDatabaseContext>();

        try
        {
            await dbContext.Database.MigrateAsync();
        }
        catch (SqlException exception)
        {
            throw new InvalidOperationException(
                "The local development database could not be initialized. Start SQL Server LocalDB, or override ConnectionStrings:LocalMyDbConnection with another local SQL Server instance.",
                exception);
        }

        if (await dbContext.Todo.AnyAsync())
        {
            return;
        }

        dbContext.Todo.AddRange(CreateSeedTodos());
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Seeded local development database with sample todo data.");
    }

    private static IEnumerable<Todo> CreateSeedTodos()
    {
        return
        [
            new Todo
            {
                Description = "Verify the local SQL Server schema matches the current EF Core migration set",
                CreatedDate = new DateTime(2026, 1, 8)
            },
            new Todo
            {
                Description = "Exercise the create, edit, details, and delete screens with fake data",
                CreatedDate = new DateTime(2026, 1, 12)
            },
            new Todo
            {
                Description = "Check cache invalidation after write operations in development",
                CreatedDate = new DateTime(2026, 1, 18)
            },
            new Todo
            {
                Description = "Validate local testing before promoting changes to a shared environment",
                CreatedDate = new DateTime(2026, 1, 24)
            }
        ];
    }
}
