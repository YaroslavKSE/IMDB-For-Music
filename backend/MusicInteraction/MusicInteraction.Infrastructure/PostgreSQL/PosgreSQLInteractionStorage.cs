using Microsoft.EntityFrameworkCore;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;
using MusicInteraction.Infrastructure.PostgreSQL.Entities;
using MusicInteraction.Infrastructure.PostgreSQL.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

                // If there's a review, add it
                if (interaction.Review != null)
                {
                    var reviewEntity = ReviewMapper.ToEntity(interaction.Review);
                    await _dbContext.Reviews.AddAsync(reviewEntity);
                }

                // If there's a rating, add it and its gradable component
                if (interaction.Rating != null)
                {
                    var ratingResult = await RatingMapper.ToEntityWithGradables(interaction.Rating, _dbContext);
                    await _dbContext.Ratings.AddAsync(ratingResult.RatingEntity);
                }

                // Save all changes
                await _dbContext.SaveChangesAsync();

                // Commit transaction
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
            var likedInteractions = await _dbContext.Interactions
                .Where(i => i.IsLiked)
                .ToListAsync();

            List<Like> likes = new List<Like>();

            foreach (var entity in likedInteractions)
            {
                var like = new Like(
                    entity.AggregateId,
                    entity.ItemId,
                    entity.CreatedAt,
                    entity.ItemType,
                    entity.UserId
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
                var review = ReviewMapper.ToDomain(entity);
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