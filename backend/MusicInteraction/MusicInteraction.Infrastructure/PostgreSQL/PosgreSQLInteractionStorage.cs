using Microsoft.EntityFrameworkCore;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;
using MusicInteraction.Infrastructure.PostgreSQL.Mapping;

namespace MusicInteraction.Infrastructure.PostgreSQL
{
    public class PostgreSQLInteractionStorage : IInteractionStorage
    {
        private readonly MusicInteractionDbContext _dbContext;

        public PostgreSQLInteractionStorage(MusicInteractionDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddInteractionAsync(InteractionsAggregate interaction)
        {
            // Begin transaction to ensure all related entities are saved together
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Convert the domain model to entity
                var interactionEntity = InteractionMapper.ToEntity(interaction);

                // Add the interaction entity
                await _dbContext.Interactions.AddAsync(interactionEntity);

                // Save changes to ensure the interaction is created with its ID
                await _dbContext.SaveChangesAsync();

                if (interaction.IsLiked)
                {
                    // Create a new Like entity
                    var likeEntity = new LikeEntity
                    {
                        LikeId = Guid.NewGuid(),
                        AggregateId = interactionEntity.AggregateId
                    };
                    await _dbContext.Likes.AddAsync(likeEntity);
                }

                if (interaction.Review != null)
                {
                    var reviewEntity = ReviewMapper.ToEntity(interaction.Review);
                    // Ensure the AggregateId is set correctly
                    reviewEntity.AggregateId = interactionEntity.AggregateId;
                    await _dbContext.Reviews.AddAsync(reviewEntity);
                }

                if (interaction.Rating != null)
                {
                    await RatingMapper.ToEntityWithGradables(interaction.Rating, _dbContext);
                }

                // Save all changes
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                // Rollback transaction if any error occurs
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<InteractionsAggregate>> GetInteractions()
        {
            var interactionEntities = await _dbContext.Interactions
                .Include(i => i.Rating)
                .Include(i => i.Review)
                .Include(i => i.Like)
                .ToListAsync();

            List<InteractionsAggregate> result = new List<InteractionsAggregate>();

            foreach (var entity in interactionEntities)
            {
                var interaction = await InteractionMapper.ToDomain(entity, _dbContext);
                result.Add(interaction);
            }

            return result;
        }

        public async Task<List<Like>> GetLikes()
        {
            var likeEntities = await _dbContext.Likes
                .Include(l => l.Interaction)
                .ToListAsync();

            List<Like> likes = new List<Like>();

            foreach (var entity in likeEntities)
            {
                var like = new Like(
                    entity.Interaction.AggregateId,
                    entity.Interaction.ItemId,
                    entity.Interaction.CreatedAt,
                    entity.Interaction.ItemType,
                    entity.Interaction.UserId
                );

                likes.Add(like);
            }

            return likes;
        }

        public async Task<List<Review>> GetReviews()
        {
            var reviewEntities = await _dbContext.Reviews.ToListAsync();
            List<Review> reviews = new List<Review>();

            foreach (var entity in reviewEntities)
            {
                var review = await ReviewMapper.ToDomain(entity, _dbContext);
                reviews.Add(review);
            }

            return reviews;
        }

        public async Task<List<Rating>> GetRatings()
        {
            var ratingEntities = await _dbContext.Ratings.ToListAsync();
            List<Rating> ratings = new List<Rating>();

            foreach (var entity in ratingEntities)
            {
                var rating = await RatingMapper.ToDomain(entity, _dbContext);
                ratings.Add(rating);
            }

            return ratings;
        }

        public async Task<bool> IsEmpty()
        {
            return !await _dbContext.Interactions.AnyAsync();
        }
    }
}