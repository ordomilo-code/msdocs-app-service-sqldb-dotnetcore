using System.ComponentModel.DataAnnotations;

namespace DotNetCoreSqlDb.Features.Account.Requests;

public sealed class UpdateProfileRequest
{
    [MaxLength(150)]
    public string? DisplayName { get; init; }
}
