using Microsoft.EntityFrameworkCore;
using TriviaApp.API.Models;

namespace TriviaApp.API.Data;

/// <summary>
/// Read/write access to the shared Slacker identity registry
/// (<c>public.user_account</c> / <c>public.user_identity</c>).
/// These tables are owned outside this application: never generate or run
/// EF migrations for this context.
/// </summary>
public class SharedIdentityDbContext : DbContext
{
    public SharedIdentityDbContext(DbContextOptions<SharedIdentityDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserAccount> UserAccounts { get; set; }
    public DbSet<UserIdentity> UserIdentities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.HasKey(e => e.UserAccountId);
            entity.ToTable("user_account", "public");
            entity.Property(e => e.UserAccountId).HasColumnName("user_account_id");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(320);
            entity.Property(e => e.DisplayName).HasColumnName("display_name").HasMaxLength(200);
            entity.Property(e => e.CreatedOn).HasColumnName("created_on");
            entity.Property(e => e.DisabledOn).HasColumnName("disabled_on");
        });

        modelBuilder.Entity<UserIdentity>(entity =>
        {
            entity.HasKey(e => e.UserIdentityId);
            entity.ToTable("user_identity", "public");
            entity.Property(e => e.UserIdentityId).HasColumnName("user_identity_id");
            entity.Property(e => e.UserAccountId).HasColumnName("user_account_id");
            entity.Property(e => e.Provider).HasColumnName("provider").HasMaxLength(50);
            entity.Property(e => e.Issuer).HasColumnName("issuer").HasMaxLength(500);
            entity.Property(e => e.Subject).HasColumnName("subject").HasMaxLength(500);
            entity.Property(e => e.LegacyUserId).HasColumnName("legacy_user_id").HasMaxLength(450);
            entity.Property(e => e.CreatedOn).HasColumnName("created_on");
            entity.HasIndex(e => new { e.Provider, e.Issuer, e.Subject }).IsUnique();
            entity.HasOne(e => e.UserAccount).WithMany(e => e.Identities).HasForeignKey(e => e.UserAccountId);
        });
    }
}
