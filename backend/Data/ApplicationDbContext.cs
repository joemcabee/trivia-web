using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TriviaApp.API.Data;

/// <summary>
/// DbContext for ASP.NET Core Identity, using the existing public."AspNetUsers" (and related) tables.
/// Do not run EF migrations for this context; the tables already exist in the database.
/// </summary>
public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Map to existing Identity tables in public schema (PascalCase names).
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUser>(entity =>
        {
            entity.ToTable("AspNetUsers", "public");
        });
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>(entity =>
        {
            entity.ToTable("AspNetRoles", "public");
        });
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>(entity =>
        {
            entity.ToTable("AspNetUserRoles", "public");
        });
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>(entity =>
        {
            entity.ToTable("AspNetUserClaims", "public");
        });
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>(entity =>
        {
            entity.ToTable("AspNetUserLogins", "public");
        });
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>(entity =>
        {
            entity.ToTable("AspNetRoleClaims", "public");
        });
        builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>(entity =>
        {
            entity.ToTable("AspNetUserTokens", "public");
        });
    }
}
