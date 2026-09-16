namespace TriviaApp.API.Models;

public class UserAccount
{
    public Guid UserAccountId { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset? DisabledOn { get; set; }
    public ICollection<UserIdentity> Identities { get; set; } = new List<UserIdentity>();
}
