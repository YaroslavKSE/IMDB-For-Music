using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;

namespace UserService.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserSubscription> UserSubscriptions { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Auth0Id).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Surname).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Auth0Id).IsRequired();
        });
        modelBuilder.Entity<UserSubscription>(entity =>
        {
            entity.ToTable("user_subscriptions");
            entity.HasKey(e => e.Id);

            // Create a unique constraint for follower_id and followed_id combination
            entity.HasIndex(e => new {e.FollowerId, e.FollowedId}).IsUnique();

            // Configure relationships
            entity.HasOne(s => s.Followed)
                .WithMany(u => u.Followers)
                .HasForeignKey(s => s.FollowedId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(s => s.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}