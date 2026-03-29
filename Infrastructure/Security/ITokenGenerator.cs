namespace DotNetCoreSqlDb.Infrastructure.Security;

public interface ITokenGenerator
{
    string GenerateToken();

    string ComputeSha256(string rawToken);
}
