using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class GetReviewedInteractionsByItemIdUseCase: IRequestHandler<GetReviewedInteractionsByItemIdCommand, GetInteractionsResult>
{
    private readonly IInteractionStorage interactionStorage;

    public GetReviewedInteractionsByItemIdUseCase(IInteractionStorage interactionStorage)
    {
        this.interactionStorage = interactionStorage;
    }

    public async Task<GetInteractionsResult> Handle(GetReviewedInteractionsByItemIdCommand request, CancellationToken cancellationToken)
    {
        var paginatedResult = await interactionStorage.GetReviewedInteractionsByItemId(request.ItemId, request.UseHotScore, request.Limit, request.Offset);

        if (paginatedResult.Items.Count == 0)
        {
            return new GetInteractionsResult() {
                InteractionsEmpty = true,
                TotalCount = paginatedResult.TotalCount
            };
        }

        List<InteractionAggregateShowDto> interactionAggregateDtos = new List<InteractionAggregateShowDto>();

        foreach (var interaction in paginatedResult.Items)
        {
            InteractionAggregateShowDto interactionShowDto = new InteractionAggregateShowDto();
            interactionShowDto.AggregateId = interaction.AggregateId;
            interactionShowDto.UserId = interaction.UserId;
            interactionShowDto.ItemId = interaction.ItemId;
            interactionShowDto.ItemType = interaction.ItemType;
            interactionShowDto.CreatedAt = interaction.CreatedAt;
            interactionShowDto.IsLiked = interaction.IsLiked;

            if (interaction.Rating != null)
            {
                interactionShowDto.Rating = new RatingNormalizedDTO()
                    {RatingId = interaction.Rating.RatingId, NormalizedGrade = interaction.Rating.Grade.getNormalizedGrade(), IsComplex = interaction.Rating.IsComplex};
            }

            if (interaction.Review != null)
            {
                interactionShowDto.Review = new ReviewDTO()
                    {ReviewId = interaction.Review.ReviewId, ReviewText = interaction.Review.ReviewText, Likes = interaction.Review.Likes, Comments = interaction.Review.Comments};
            }

            interactionAggregateDtos.Add(interactionShowDto);
        }

        return new GetInteractionsResult() {
            InteractionsEmpty = false,
            Interactions = interactionAggregateDtos,
            TotalCount = paginatedResult.TotalCount
        };
    }
}