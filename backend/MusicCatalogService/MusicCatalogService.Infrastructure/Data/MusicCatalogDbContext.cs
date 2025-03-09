using Microsoft.EntityFrameworkCore;
using MusicCatalogService.Core.Models;

namespace MusicCatalogService.Infrastructure.Data;

public class MusicCatalogDbContext : DbContext
{
    public DbSet<CatalogItem> CatalogItems { get; set; }

    public MusicCatalogDbContext(DbContextOptions<MusicCatalogDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CatalogItem>(entity =>
        {
            entity.ToTable("catalog_items");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SpotifyId, e.Type }).IsUnique();
            entity.Property(e => e.SpotifyId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ArtistName).HasMaxLength(255);
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(1000);
        });
    }
}