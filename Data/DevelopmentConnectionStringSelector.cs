using Microsoft.Data.SqlClient;

namespace DotNetCoreSqlDb.Data;

public static class DevelopmentConnectionStringSelector
{
    private const string DefaultDevelopmentConnectionName = "MyDbConnection";
    private const string WindowsLocalDevelopmentConnectionName = "LocalMyDbConnection";

    public static string GetDevelopmentConnectionString(IConfiguration configuration)
    {
        if (OperatingSystem.IsWindows())
        {
            var windowsLocalConnectionString = configuration.GetConnectionString(WindowsLocalDevelopmentConnectionName);
            if (!string.IsNullOrWhiteSpace(windowsLocalConnectionString))
            {
                return windowsLocalConnectionString;
            }
        }

        return configuration.GetConnectionString(DefaultDevelopmentConnectionName)
            ?? throw new InvalidOperationException(
                $"Missing connection string '{DefaultDevelopmentConnectionName}' for development.");
    }

    public static bool IsLocalConnectionString(string connectionString)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var dataSource = (builder.DataSource ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(dataSource))
        {
            return false;
        }

        return dataSource.StartsWith("(localdb)", StringComparison.OrdinalIgnoreCase)
            || dataSource.Equals(".", StringComparison.OrdinalIgnoreCase)
            || dataSource.Equals("(local)", StringComparison.OrdinalIgnoreCase)
            || dataSource.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || dataSource.StartsWith("localhost\\", StringComparison.OrdinalIgnoreCase)
            || dataSource.StartsWith("localhost,", StringComparison.OrdinalIgnoreCase)
            || dataSource.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase)
            || dataSource.StartsWith("127.0.0.1\\", StringComparison.OrdinalIgnoreCase)
            || dataSource.StartsWith("127.0.0.1,", StringComparison.OrdinalIgnoreCase);
    }
}
