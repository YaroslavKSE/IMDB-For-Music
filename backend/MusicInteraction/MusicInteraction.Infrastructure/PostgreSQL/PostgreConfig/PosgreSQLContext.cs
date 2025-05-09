using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;

namespace MusicInteraction.Infrastructure.PostgreSQL
{
    public class MusicInteractionDbContext : DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly DbContextOptions<MusicInteractionDbContext> _options;

        // Constructor for use with DI in application
        public MusicInteractionDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Constructor for use with specified options (used by design-time factory and tests)
        public MusicInteractionDbContext(DbContextOptions<MusicInteractionDbContext> options)
            : base(options)
        {
            _options = options;
        }

        // Constructor for use with both configuration and options
        public MusicInteractionDbContext(IConfiguration configuration, DbContextOptions<MusicInteractionDbContext> options)
            : base(options)
        {
            _configuration = configuration;
            _options = options;
        }

        // Tables
        public DbSet<InteractionAggregateEntity> Interactions { get; set; }
        public DbSet<RatingEntity> Ratings { get; set; }
        public DbSet<LikeEntity> Likes { get; set; }
        public DbSet<ReviewEntity> Reviews { get; set; }
        public DbSet<ReviewLikeEntity> ReviewLikes { get; set; }
        public DbSet<ReviewCommentEntity> ReviewComments { get; set; }

        public DbSet<ItemStatsEntity> ItemStats { get; set; }
        public DbSet<GradeEntity> Grades { get; set; }
        public DbSet<GradingMethodInstanceEntity> GradingMethodInstances { get; set; }
        public DbSet<GradingBlockEntity> GradingBlocks { get; set; }
        public DbSet<GradingMethodComponentEntity> GradingMethodComponents { get; set; }
        public DbSet<GradingBlockComponentEntity> GradingBlockComponents { get; set; }
        public DbSet<GradingMethodActionEntity> GradingMethodActions { get; set; }
        public DbSet<GradingBlockActionEntity> GradingBlockActions { get; set; }

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

            // Configure original relationships
            modelBuilder.Entity<InteractionAggregateEntity>()
                .HasOne(i => i.Rating)
                .WithOne(r => r.Interaction)
                .HasForeignKey<RatingEntity>(r => r.AggregateId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InteractionAggregateEntity>()
                .HasOne(i => i.Review)
                .WithOne(r => r.Interaction)
                .HasForeignKey<ReviewEntity>(r => r.AggregateId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure new one-to-one relationship between Interaction and Like
            modelBuilder.Entity<InteractionAggregateEntity>()
                .HasOne(i => i.Like)
                .WithOne(l => l.Interaction)
                .HasForeignKey<LikeEntity>(l => l.AggregateId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-one between Rating and Grade (for simple ratings)
            modelBuilder.Entity<RatingEntity>()
                .HasOne(r => r.SimpleGrade)
                .WithOne(g => g.Rating)
                .HasForeignKey<GradeEntity>(g => g.RatingId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-one between Rating and GradingMethodInstance (for complex ratings)
            modelBuilder.Entity<RatingEntity>()
                .HasOne(r => r.ComplexGrade)
                .WithOne(g => g.Rating)
                .HasForeignKey<GradingMethodInstanceEntity>(g => g.RatingId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // GradingMethodComponent relationships
            modelBuilder.Entity<GradingMethodComponentEntity>()
                .HasOne(c => c.GradingMethod)
                .WithMany(m => m.Components)
                .HasForeignKey(c => c.GradingMethodId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GradingMethodComponentEntity>()
                .HasOne(c => c.BlockComponent)
                .WithMany(b => b.MethodComponents)
                .HasForeignKey(c => c.BlockComponentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GradingMethodComponentEntity>()
                .HasOne(c => c.GradeComponent)
                .WithMany(g => g.MethodComponents)
                .HasForeignKey(c => c.GradeComponentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // GradingBlockComponent relationships
            modelBuilder.Entity<GradingBlockComponentEntity>()
                .HasOne(c => c.GradingBlock)
                .WithMany(b => b.Components)
                .HasForeignKey(c => c.GradingBlockId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GradingBlockComponentEntity>()
                .HasOne(c => c.BlockComponent)
                .WithMany(b => b.ParentBlockComponents)
                .HasForeignKey(c => c.BlockComponentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GradingBlockComponentEntity>()
                .HasOne(c => c.GradeComponent)
                .WithMany(g => g.BlockComponents)
                .HasForeignKey(c => c.GradeComponentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);

            // Action relationships
            modelBuilder.Entity<GradingMethodActionEntity>()
                .HasOne(a => a.GradingMethod)
                .WithMany(m => m.Actions)
                .HasForeignKey(a => a.GradingMethodId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GradingBlockActionEntity>()
                .HasOne(a => a.GradingBlock)
                .WithMany(b => b.Actions)
                .HasForeignKey(a => a.GradingBlockId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InteractionAggregateEntity>()
                .HasIndex(i => i.UserId);

            modelBuilder.Entity<InteractionAggregateEntity>()
                .HasIndex(i => i.ItemId);

            modelBuilder.Entity<InteractionAggregateEntity>()
                .HasIndex(i => i.CreatedAt);

            // Add composite index for (UserId, ItemId)
            modelBuilder.Entity<InteractionAggregateEntity>()
                .HasIndex(i => new { i.UserId, i.ItemId, i.CreatedAt});

            // Set up unique constraints
            modelBuilder.Entity<GradingMethodComponentEntity>()
                .HasIndex(c => new { c.GradingMethodId, c.ComponentNumber })
                .IsUnique();

            modelBuilder.Entity<GradingBlockComponentEntity>()
                .HasIndex(c => new { c.GradingBlockId, c.ComponentNumber })
                .IsUnique();

            modelBuilder.Entity<GradingMethodActionEntity>()
                .HasIndex(a => new { a.GradingMethodId, a.ActionNumber })
                .IsUnique();

            modelBuilder.Entity<GradingBlockActionEntity>()
                .HasIndex(a => new { a.GradingBlockId, a.ActionNumber })
                .IsUnique();

            // Configure ReviewLike relationship
            modelBuilder.Entity<ReviewLikeEntity>()
                .HasOne(rl => rl.Review)
                .WithMany(r => r.Likes)
                .HasForeignKey(rl => rl.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ReviewComment relationship
            modelBuilder.Entity<ReviewCommentEntity>()
                .HasOne(rc => rc.Review)
                .WithMany(r => r.Comments)
                .HasForeignKey(rc => rc.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // Create indexes for faster lookups
            modelBuilder.Entity<ReviewLikeEntity>()
                .HasIndex(rl => new { rl.ReviewId, rl.UserId })
                .IsUnique();

            modelBuilder.Entity<ReviewLikeEntity>()
                .HasIndex(rl => rl.UserId);

            modelBuilder.Entity<ReviewCommentEntity>()
                .HasIndex(rc => rc.ReviewId);

            modelBuilder.Entity<ReviewCommentEntity>()
                .HasIndex(rc => rc.UserId);

            modelBuilder.Entity<ReviewEntity>()
                .HasIndex(r => r.HotScore)
                .HasDatabaseName("IX_Reviews_HotScore")
                .IsDescending();

            // Index for IsScoreDirty for efficient querying reviews that need HotScore recalculation
            modelBuilder.Entity<ReviewEntity>()
                .HasIndex(r => r.IsScoreDirty)
                .HasDatabaseName("IX_Reviews_IsScoreDirty")
                .HasFilter("\"IsScoreDirty\" = true");
        }
    }
}