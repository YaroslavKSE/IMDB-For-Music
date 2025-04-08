using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class GetInteractionsByItemIdUseCase : IRequestHandler<GetInteractionsByItemIdCommand, GetInteractionsResult>
{
    private readonly IInteractionStorage interactionStorage;

    public GetInteractionsByItemIdUseCase(IInteractionStorage interactionStorage)
    {
        this.interactionStorage = interactionStorage;
    }

    public async Task<GetInteractionsResult> Handle(GetInteractionsByItemIdCommand request, CancellationToken cancellationToken)
    {
        if (await interactionStorage.IsEmpty())
        {
            return new GetInteractionsResult() { InteractionsEmpty = true };
        }

        var interactions = await interactionStorage.GetInteractionsByItemId(request.ItemId);
        if (interactions.Count == 0)
        {
            return new GetInteractionsResult() { InteractionsEmpty = true };
        }

        List<InteractionAggregateShowDto> interactionAggregateDtos = new List<InteractionAggregateShowDto>();

        foreach (var interaction in interactions)
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
                bool isComplex = await interactionStorage.GetGradingTypeByInteractionId(interaction.AggregateId);
                interactionShowDto.Rating = new RatingNormalizedDTO()
                    {RatingId = interaction.Rating.RatingId, NormalizedGrade = interaction.Rating.Grade.getNormalizedGrade(), IsComplex = isComplex};
            }

            if (interaction.Review != null)
            {
                interactionShowDto.Review = new ReviewDTO()
                    {ReviewId = interaction.Review.ReviewId, ReviewText = interaction.Review.ReviewText};
            }

            interactionAggregateDtos.Add(interactionShowDto);
        }

        return new GetInteractionsResult() { InteractionsEmpty = false, Interactions = interactionAggregateDtos };
    }
}