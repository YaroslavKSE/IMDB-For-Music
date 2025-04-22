using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MusicLists.Infrastructure.Entities;

namespace MusicLists.Infrastructure.DBConfig;

public class MusicListsDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    private readonly DbContextOptions<MusicListsDbContext> _options;

    // Constructor for use with DI in application
    public MusicListsDbContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // Constructor for use with specified options (used by design-time factory and tests)
    public MusicListsDbContext(DbContextOptions<MusicListsDbContext> options)
        : base(options)
    {
        _options = options;
    }

    // Constructor for use with both configuration and options
    public MusicListsDbContext(IConfiguration configuration, DbContextOptions<MusicListsDbContext> options)
        : base(options)
    {
        _configuration = configuration;
        _options = options;
    }

    public DbSet<ListEntity> Lists { get; set; }
    public DbSet<ListItemEntity> ListItems { get; set; }
    public DbSet<ListLikeEntity> ListLikes { get; set; }
    public DbSet<ListCommentEntity> ListComments { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            string connectionString = _configuration?.GetConnectionString("PostgreSQL") ??
                                      "Host=localhost;Database=MusicInteraction;Username=qualiaaa;Password=password";

            optionsBuilder.UseNpgsql(connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure ListEntity
        modelBuilder.Entity<ListEntity>(entity =>
        {
            entity.HasKey(e => e.ListId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.IsScoreDirty);
            entity.HasIndex(e => e.HotScore);
            entity.HasIndex(e => e.ListType);

            // Configure one-to-many relationship with ListItems with cascade delete
            entity.HasMany(e => e.Items)
                  .WithOne(i => i.List)
                  .HasForeignKey(i => i.ListId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationship with ListLikes with cascade delete
            entity.HasMany(e => e.Likes)
                  .WithOne(l => l.List)
                  .HasForeignKey(l => l.ListId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Configure one-to-many relationship with ListComments with cascade delete
            entity.HasMany(e => e.Comments)
                  .WithOne(c => c.List)
                  .HasForeignKey(c => c.ListId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure ListItemEntity
        modelBuilder.Entity<ListItemEntity>(entity =>
        {
            entity.HasKey(e => e.ListItemId);
            entity.HasIndex(e => e.ItemId);
            entity.HasIndex(e => e.ListId);
        });

        // Configure ListLikeEntity
        modelBuilder.Entity<ListLikeEntity>(entity =>
        {
            entity.HasKey(e => e.LikeId);
            entity.HasIndex(e => e.ListId);
            entity.HasIndex(e => e.UserId);
        });

        // Configure ListCommentEntity
        modelBuilder.Entity<ListCommentEntity>(entity =>
        {
            entity.HasKey(e => e.CommentId);
            entity.HasIndex(e => e.ListId);
            entity.HasIndex(e => e.UserId);
        });

        // Configure composite indexes where needed
        modelBuilder.Entity<ListLikeEntity>()
            .HasIndex(e => new { e.ListId, e.UserId })
            .IsUnique(); // Ensuring a user can only like a list once

        modelBuilder.Entity<ListCommentEntity>()
            .HasIndex(e => new { e.ListId, e.CommentedAt }); // For efficiently retrieving comments by date
    }
}