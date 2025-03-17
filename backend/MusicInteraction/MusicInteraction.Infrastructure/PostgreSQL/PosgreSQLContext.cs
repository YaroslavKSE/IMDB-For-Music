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

        public DbSet<InteractionAggregateEntity> Interactions { get; set; }
        public DbSet<ReviewEntity> Reviews { get; set; }
        public DbSet<RatingEntity> Ratings { get; set; }
        public DbSet<GradeEntity> Grades { get; set; }
        public DbSet<GradingMethodInstanceEntity> GradingMethodInstances { get; set; }
        public DbSet<GradingBlockEntity> GradingBlocks { get; set; }

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

            // Configure relationships
            modelBuilder.Entity<InteractionAggregateEntity>()
                .HasOne(i => i.Rating)
                .WithOne(r => r.Interaction)
                .HasForeignKey<RatingEntity>(r => r.AggregateId);

            modelBuilder.Entity<InteractionAggregateEntity>()
                .HasOne(i => i.Review)
                .WithOne(r => r.Interaction)
                .HasForeignKey<ReviewEntity>(r => r.AggregateId);

            // Configure JSON columns for PostgreSQL
            modelBuilder.Entity<GradingMethodInstanceEntity>()
                .Property(g => g.ComponentsJson)
                .HasColumnType("jsonb");

            modelBuilder.Entity<GradingMethodInstanceEntity>()
                .Property(g => g.ActionsJson)
                .HasColumnType("jsonb");

            modelBuilder.Entity<GradingBlockEntity>()
                .Property(g => g.ComponentsJson)
                .HasColumnType("jsonb");

            modelBuilder.Entity<GradingBlockEntity>()
                .Property(g => g.ActionsJson)
                .HasColumnType("jsonb");
        }
    }
}