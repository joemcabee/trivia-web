using Microsoft.EntityFrameworkCore;
using TriviaApp.API.Models;

namespace TriviaApp.API.Data;

public class TriviaDbContext : DbContext
{
    public TriviaDbContext(DbContextOptions<TriviaDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Round> Rounds { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Question> Questions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set default schema to "trivia"
        modelBuilder.HasDefaultSchema("trivia");

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasMany(e => e.Rounds)
                  .WithOne(r => r.Event)
                  .HasForeignKey(r => r.EventId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Round>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(r => r.Event)
                  .WithMany(e => e.Rounds)
                  .HasForeignKey(r => r.EventId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(r => r.Categories)
                  .WithOne(c => c.Round)
                  .HasForeignKey(c => c.RoundId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.HasOne(c => c.Round)
                  .WithMany(r => r.Categories)
                  .HasForeignKey(c => c.RoundId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(c => c.Questions)
                  .WithOne(q => q.Category)
                  .HasForeignKey(q => q.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.QuestionText).IsRequired();
            entity.Property(q => q.Answer).IsRequired();
            entity.HasOne(q => q.Category)
                  .WithMany(c => c.Questions)
                  .HasForeignKey(q => q.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

