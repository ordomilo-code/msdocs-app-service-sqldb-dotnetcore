using System.Security.Cryptography;
using System.Text;

namespace DotNetCoreSqlDb.Infrastructure.Security;

public sealed class TokenGenerator : ITokenGenerator
{
    public string GenerateToken()
    {
        Span<byte> bytes = stackalloc byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes);
    }

    public string ComputeSha256(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }
}
