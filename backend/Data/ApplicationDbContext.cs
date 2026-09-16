using Microsoft.EntityFrameworkCore;
using TriviaApp.API.Models;

namespace TriviaApp.API.Data;

/// <summary>
/// Application-owned tables in the <c>public</c> schema.
/// <c>support.user_id</c> is a shared-registry account UUID (C# stays string,
/// mapped to the <c>uuid</c> column); it no longer references ASP.NET Identity.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Support> Supports { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Support>(entity =>
        {
            entity.ToTable("support", "public");
            entity.HasKey(s => s.SupportId).HasName("support_pkey");

            entity.Property(s => s.SupportId).HasColumnName("support_id");
            entity.Property(s => s.UserId)
                .HasConversion(
                    accountId => string.IsNullOrWhiteSpace(accountId) ? (Guid?)null : Guid.Parse(accountId),
                    accountId => accountId.HasValue ? accountId.Value.ToString() : null)
                .HasColumnType("uuid")
                .HasColumnName("user_id");
            entity.Property(s => s.Application).HasColumnName("application").HasMaxLength(100).IsRequired();
            entity.Property(s => s.MessageText).HasColumnName("message_text").IsRequired();
            entity.Property(s => s.CreatedOn).HasColumnName("created_on");
            entity.Property(s => s.IsOpen).HasColumnName("is_open");
            entity.Property(s => s.ClosedOn).HasColumnName("closed_on");
        });
    }
}
