using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DotNetCoreSqlDb.Data;

public static class ProductionDatabaseMigrator
{
    public static async Task MigrateAsync(IServiceProvider services, ILogger logger)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToArray();
        if (pendingMigrations.Length == 0)
        {
            logger.LogInformation("No pending EF Core migrations detected for AppDbContext in non-development startup.");
            return;
        }

        logger.LogInformation(
            "Applying {Count} pending EF Core migration(s) for AppDbContext: {Migrations}",
            pendingMigrations.Length,
            string.Join(", ", pendingMigrations));

        try
        {
            await dbContext.Database.MigrateAsync();
        }
        catch (Exception exception) when (exception is SqlException or DbUpdateException or InvalidOperationException)
        {
            throw new InvalidOperationException(
                "The application database could not be migrated during non-development startup.",
                exception);
        }

        logger.LogInformation("EF Core migrations applied successfully for AppDbContext.");
    }
}
