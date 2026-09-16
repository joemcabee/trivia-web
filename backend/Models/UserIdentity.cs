namespace TriviaApp.API.Models;

public class UserIdentity
{
    public long UserIdentityId { get; set; }
    public Guid UserAccountId { get; set; }
    public string Provider { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string? LegacyUserId { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public UserAccount UserAccount { get; set; } = null!;
}
