using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class GetFollowingInteractionsUseCase : IRequestHandler<GetFollowingInteractionsCommand, GetInteractionsResult>
{
    private readonly IInteractionStorage _interactionStorage;
    private static readonly HttpClient _httpClient = new HttpClient()
    {
        BaseAddress = new Uri("http://user-service/")
    };

    public GetFollowingInteractionsUseCase(IInteractionStorage interactionStorage)
    {
        _interactionStorage = interactionStorage;
    }

    public async Task<GetInteractionsResult> Handle(GetFollowingInteractionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Collect all following user IDs
            var followingIds = new List<string>();
            int currentPage = 1;
            bool hasNextPage = true;

            while (hasNextPage)
            {
                var response = await _httpClient.GetAsync(
                    $"api/v1/internal/following?userId={request.UserId}&page={currentPage}&pageSize=2000",
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to fetch following users. Status code: {response.StatusCode}");
                    return new GetInteractionsResult
                    {
                        InteractionsEmpty = true,
                        TotalCount = 0
                    };
                }

                var result = await response.Content.ReadFromJsonAsync<FollowingResponse>(cancellationToken: cancellationToken);

                if (result == null || result.FollowingIds == null)
                {
                    break;
                }

                followingIds.AddRange(result.FollowingIds);
                hasNextPage = result.HasNextPage;
                currentPage++;
            }

            if (followingIds.Count == 0)
            {
                return new GetInteractionsResult
                {
                    InteractionsEmpty = true,
                    TotalCount = 0
                };
            }

            // Get interactions from users this user follows
            var paginatedResult = await _interactionStorage.GetInteractionsByUserIds(
                followingIds,
                request.Limit,
                request.Offset);

            if (paginatedResult.Items.Count == 0)
            {
                return new GetInteractionsResult
                {
                    InteractionsEmpty = true,
                    TotalCount = paginatedResult.TotalCount
                };
            }

            // Map to DTOs
            List<InteractionAggregateShowDto> interactionAggregateDtos = new List<InteractionAggregateShowDto>();

            foreach (var interaction in paginatedResult.Items)
            {
                InteractionAggregateShowDto interactionShowDto = new InteractionAggregateShowDto
                {
                    AggregateId = interaction.AggregateId,
                    UserId = interaction.UserId,
                    ItemId = interaction.ItemId,
                    ItemType = interaction.ItemType,
                    CreatedAt = interaction.CreatedAt,
                    IsLiked = interaction.IsLiked
                };

                if (interaction.Rating != null)
                {
                    interactionShowDto.Rating = new RatingNormalizedDTO
                    {
                        RatingId = interaction.Rating.RatingId,
                        NormalizedGrade = interaction.Rating.Grade.getNormalizedGrade(),
                        IsComplex = interaction.Rating.IsComplex
                    };
                }

                if (interaction.Review != null)
                {
                    interactionShowDto.Review = new ReviewDTO
                    {
                        ReviewId = interaction.Review.ReviewId,
                        ReviewText = interaction.Review.ReviewText,
                        Likes = interaction.Review.Likes,
                        Comments = interaction.Review.Comments
                    };
                }

                interactionAggregateDtos.Add(interactionShowDto);
            }

            return new GetInteractionsResult
            {
                InteractionsEmpty = false,
                Interactions = interactionAggregateDtos,
                TotalCount = paginatedResult.TotalCount
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching following interactions for user {request.UserId}: {ex.Message}");
            throw;
        }
    }

    // Class to deserialize the response from the Users Service
    private class FollowingResponse
    {
        public List<string> FollowingIds { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
    }
}