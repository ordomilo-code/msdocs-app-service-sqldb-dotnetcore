namespace DotNetCoreSqlDb.Infrastructure.Http;

public sealed class ApiException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}
