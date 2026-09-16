using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TriviaApp.API.Data;
using TriviaApp.API.Models;

namespace TriviaApp.API.Services;

/// <summary>
/// Resolves the authenticated Keycloak identity (iss + sub) to the shared
/// local account UUID, provisioning a new empty account for unknown users.
/// Lookup is by provider + issuer + subject only; never by email.
/// Requires the Keycloak `basic` client scope (oidc-sub-mapper) so access
/// tokens carry `sub`; see keycloak/realms README.
/// </summary>
public sealed class CurrentAccountService(
    IDbContextFactory<SharedIdentityDbContext> dbContextFactory,
    ILogger<CurrentAccountService> logger)
{
    public const string LocalAccountClaimType = "slacker_account_id";
    private const string Provider = "keycloak";

    public async Task<string?> GetOrCreateAccountIdAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true) return null;
        var issuer = principal.FindFirst("iss")?.Value;
        var subject = principal.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(subject))
        {
            var claimTypes = string.Join(",", principal.Claims.Select(c => c.Type).Distinct());
            logger.LogWarning("Authenticated principal is missing Keycloak issuer or subject. Claim types present: {ClaimTypes}", claimTypes);
            return null;
        }

        await using var db = await dbContextFactory.CreateDbContextAsync();
        var existing = await db.UserIdentities
            .Where(i => i.Provider == Provider && i.Issuer == issuer && i.Subject == subject)
            .Select(i => (Guid?)i.UserAccountId)
            .SingleOrDefaultAsync();
        if (existing is not null) return existing.Value.ToString();

        var account = new UserAccount
        {
            UserAccountId = Guid.NewGuid(),
            Email = principal.FindFirst("email")?.Value,
            DisplayName = principal.FindFirst("name")?.Value ?? principal.FindFirst("preferred_username")?.Value,
            CreatedOn = DateTimeOffset.UtcNow
        };
        db.UserAccounts.Add(account);
        db.UserIdentities.Add(new UserIdentity
        {
            UserAccountId = account.UserAccountId,
            Provider = Provider,
            Issuer = issuer,
            Subject = subject,
            CreatedOn = DateTimeOffset.UtcNow
        });

        try
        {
            await db.SaveChangesAsync();
            return account.UserAccountId.ToString();
        }
        catch (DbUpdateException)
        {
            await using var retry = await dbContextFactory.CreateDbContextAsync();
            return (await retry.UserIdentities
                    .Where(i => i.Provider == Provider && i.Issuer == issuer && i.Subject == subject)
                    .Select(i => (Guid?)i.UserAccountId)
                    .SingleOrDefaultAsync())?.ToString()
                ?? throw new DbUpdateException("Could not provision a local account for the Keycloak identity.");
        }
    }
}
