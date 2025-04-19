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
                var interactionEntity = InteractionMapper.ToEntity(interaction);

                await _dbContext.Interactions.AddAsync(interactionEntity);

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
                    reviewEntity.AggregateId = interactionEntity.AggregateId;
                    await _dbContext.Reviews.AddAsync(reviewEntity);
                }

                if (interaction.Rating != null)
                {
                    await RatingMapper.ToEntityAsync(interaction.Rating, _dbContext);
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

        public async Task UpdateInteractionAsync(InteractionsAggregate interaction)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            //Implement like changes
            var likeEntity = await _dbContext.Likes.FirstOrDefaultAsync(i => i.AggregateId == interaction.AggregateId);
            if (likeEntity == null && interaction.IsLiked)
            {
                likeEntity = new LikeEntity
                {
                    LikeId = Guid.NewGuid(),
                    AggregateId = interaction.AggregateId
                };
                await _dbContext.Likes.AddAsync(likeEntity);
            }
            else if(likeEntity != null && !interaction.IsLiked)
            {
                _dbContext.Likes.Remove(likeEntity);
            }

            //Implement review changes
            var reviewEntity =
                await _dbContext.Reviews.FirstOrDefaultAsync(i => i.AggregateId == interaction.AggregateId);
            if (reviewEntity != null)
            {
                if (interaction.Review == null || interaction.Review.ReviewText == "")
                {
                    _dbContext.Reviews.Remove(reviewEntity);
                }
                else if(interaction.Review.ReviewText != reviewEntity.ReviewText)
                {
                    reviewEntity.ReviewText = interaction.Review.ReviewText;
                    _dbContext.Reviews.Update(reviewEntity);
                }
            }
            else if (reviewEntity == null && interaction.Review != null && interaction.Review.ReviewText != "")
            {
                reviewEntity = ReviewMapper.ToEntity(interaction.Review);
                reviewEntity.AggregateId = interaction.AggregateId;
                await _dbContext.Reviews.AddAsync(reviewEntity);
            }

            //Implement rating changes
            var ratingEntity =
                await _dbContext.Ratings.FirstOrDefaultAsync(i => i.AggregateId == interaction.AggregateId);
            if (ratingEntity != null)
            {
                _dbContext.Ratings.Remove(ratingEntity);
            }
            if (interaction.Rating != null)
            {
                await RatingMapper.ToEntityAsync(interaction.Rating, _dbContext);
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task DeleteInteractionAsync(Guid interactionId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                // Find the interaction
                var interactionEntity = await _dbContext.Interactions
                    .FirstOrDefaultAsync(i => i.AggregateId == interactionId);

                if (interactionEntity == null)
                {
                    throw new KeyNotFoundException($"Interaction with ID {interactionId} not found");
                }

                // Simply remove the interaction - EF Core will handle cascade deletes
                _dbContext.Interactions.Remove(interactionEntity);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error deleting interaction: {ex.Message}");
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

        public async Task<InteractionsAggregate> GetInteractionById(Guid interactionId)
        {
            try
            {
                var interactionEntity = await _dbContext.Interactions
                    .Include(i => i.Rating)
                    .Include(i => i.Review)
                    .Include(i => i.Like)
                    .FirstOrDefaultAsync(i => i.AggregateId == interactionId);

                if (interactionEntity == null)
                {
                    return null;
                }
                InteractionsAggregate interaction = await InteractionMapper.ToDomain(interactionEntity, _dbContext);
                return interaction;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving interaction with ID {interactionId}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<InteractionsAggregate>> GetInteractionsByUserId(string userId)
        {
            var interactionEntities = await _dbContext.Interactions
                .Where(i => i.UserId == userId)
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

        public async Task<List<InteractionsAggregate>> GetInteractionsByItemId(string itemId)
        {
            var interactionEntities = await _dbContext.Interactions
                .Where(i => i.ItemId == itemId)
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

        public async Task<List<InteractionsAggregate>> GetInteractionsByUserAndItem(string userId, string itemId)
        {
            var interactionEntities = await _dbContext.Interactions
                .Where(i => i.UserId == userId && i.ItemId == itemId)
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
                var rating = await RatingMapper.ToDomainAsync(entity, _dbContext);
                ratings.Add(rating);
            }

            return ratings;
        }

        public async Task<Rating> GetRatingById(Guid ratingId)
        {
            try
            {
                var ratingEntity = await _dbContext.Ratings
                    .Include(r => r.Interaction)
                    .FirstOrDefaultAsync(r => r.RatingId == ratingId);

                if (ratingEntity == null)
                {
                    return null;
                }

                return await RatingMapper.ToDomainAsync(ratingEntity, _dbContext);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving rating with ID {ratingId}: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> IsEmpty()
        {
            return !await _dbContext.Interactions.AnyAsync();
        }
    }
}