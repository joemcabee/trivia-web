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

        modelBuilder.HasDefaultSchema("trivia");

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200).HasColumnName("name");
            entity.Property(e => e.Description).IsRequired().HasColumnName("description");
            entity.Property(e => e.CreatedOn).HasColumnName("created_on");
            entity.Property(e => e.UpdatedOn).HasColumnName("updated_on");
            entity.Property(e => e.UserId).IsRequired().HasColumnName("user_id");
            entity.HasMany(e => e.Rounds)
                  .WithOne(r => r.Event)
                  .HasForeignKey(r => r.EventId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Round>(entity =>
        {
            entity.ToTable("rounds");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).HasColumnName("id");
            entity.Property(r => r.Name).IsRequired().HasMaxLength(200).HasColumnName("name");
            entity.Property(r => r.EventId).HasColumnName("event_id");
            entity.Property(r => r.Order).HasColumnName("order");
            entity.Property(r => r.CreatedOn).HasColumnName("created_on");
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
            entity.ToTable("categories");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("id");
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200).HasColumnName("name");
            entity.Property(c => c.RoundId).HasColumnName("round_id");
            entity.Property(c => c.Order).HasColumnName("order");
            entity.Property(c => c.CreatedOn).HasColumnName("created_on");
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
            entity.ToTable("questions");
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Id).HasColumnName("id");
            entity.Property(q => q.QuestionText).IsRequired().HasColumnName("question_text");
            entity.Property(q => q.Answer).IsRequired().HasColumnName("answer");
            entity.Property(q => q.ImageUrl).HasColumnName("image_url");
            entity.Property(q => q.CategoryId).HasColumnName("category_id");
            entity.Property(q => q.Order).HasColumnName("order");
            entity.Property(q => q.CreatedOn).HasColumnName("created_on");
            entity.HasOne(q => q.Category)
                  .WithMany(c => c.Questions)
                  .HasForeignKey(q => q.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
