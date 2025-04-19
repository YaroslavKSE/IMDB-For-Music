using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class GetInteractionsUseCase : IRequestHandler<GetInteractionsCommand, GetInteractionsResult>
{
    private readonly IInteractionStorage interactionStorage;

    public GetInteractionsUseCase(IInteractionStorage interactionStorage)
    {
        this.interactionStorage = interactionStorage;
    }

    public async Task<GetInteractionsResult> Handle(GetInteractionsCommand request, CancellationToken cancellationToken)
    {
        if (await interactionStorage.IsEmpty())
        {
            return new GetInteractionsResult() {InteractionsEmpty = true};
        }

        var interactions = interactionStorage.GetInteractions().Result;
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
                interactionShowDto.Rating = new RatingNormalizedDTO()
                    {RatingId = interaction.Rating.RatingId, NormalizedGrade = interaction.Rating.Grade.getNormalizedGrade(), IsComplex = interaction.Rating.IsComplex};
            }

            if (interaction.Review != null)
            {
                interactionShowDto.Review = new ReviewDTO()
                    {ReviewId = interaction.Review.ReviewId, ReviewText = interaction.Review.ReviewText};
            }

            interactionAggregateDtos.Add(interactionShowDto);
        }
        return new GetInteractionsResult() {InteractionsEmpty = false, Interactions = interactionAggregateDtos};
    }
}