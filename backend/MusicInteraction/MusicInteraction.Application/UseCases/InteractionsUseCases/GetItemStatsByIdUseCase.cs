using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

namespace MusicInteraction.Application;

public class GetItemStatsByIdUseCase : IRequestHandler<GetItemStatsByIdCommand, GetItemStatsResult>
{
    private readonly IItemStatsStorage _itemStatsStorage;

    public GetItemStatsByIdUseCase(IItemStatsStorage itemStatsStorage)
    {
        _itemStatsStorage = itemStatsStorage;
    }

    public async Task<GetItemStatsResult> Handle(GetItemStatsByIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ItemId))
            {
                return new GetItemStatsResult
                {
                    Success = false,
                    ErrorMessage = "Item ID is required"
                };
            }

            // Check if stats exist for this item
            bool exists = await _itemStatsStorage.ItemStatsExistsAsync(request.ItemId);

            if (!exists)
            {
                // Return empty stats
                return new GetItemStatsResult
                {
                    Success = true,
                    Stats = new ItemStatsDto
                    {
                        ItemId = request.ItemId,
                        TotalUsersInteracted = 0,
                        TotalLikes = 0,
                        TotalReviews = 0,
                        RatingDistribution = new int[10],
                        AverageRating = 0,
                        HasRatings = false
                    }
                };
            }

            // Get item stats from storage
            ItemStats stats = await _itemStatsStorage.GetItemStatsAsync(request.ItemId);

            // Map to DTO
            var dto = new ItemStatsDto
            {
                ItemId = stats.ItemId,
                TotalUsersInteracted = stats.TotalUsersInteracted,
                TotalLikes = stats.TotalLikes,
                TotalReviews = stats.TotalReviews,
                RatingDistribution = new int[]
                {
                    stats.TotalOneRatings,
                    stats.TotalTwoRatings,
                    stats.TotalThreeRatings,
                    stats.TotalFourRatings,
                    stats.TotalFiveRatings,
                    stats.TotalSixRatings,
                    stats.TotalSevenRatings,
                    stats.TotalEightRatings,
                    stats.TotalNineRatings,
                    stats.TotalTenRatings
                },
                AverageRating = stats.Rating,
                HasRatings = stats.TotalOneRatings + stats.TotalTwoRatings + stats.TotalThreeRatings +
                             stats.TotalFourRatings + stats.TotalFiveRatings + stats.TotalSixRatings +
                             stats.TotalSevenRatings + stats.TotalEightRatings + stats.TotalNineRatings +
                             stats.TotalTenRatings > 0
            };

            return new GetItemStatsResult
            {
                Success = true,
                Stats = dto
            };
        }
        catch (Exception ex)
        {
            return new GetItemStatsResult
            {
                Success = false,
                ErrorMessage = $"Error retrieving item stats: {ex.Message}"
            };
        }
    }
}