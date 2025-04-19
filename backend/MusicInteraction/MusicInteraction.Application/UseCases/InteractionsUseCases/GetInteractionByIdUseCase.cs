using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class GetInteractionByIdUseCase : IRequestHandler<GetInteractionByIdCommand, GetInteractionDetailResult>
{
    private readonly IInteractionStorage interactionStorage;

    public GetInteractionByIdUseCase(IInteractionStorage interactionStorage)
    {
        this.interactionStorage = interactionStorage;
    }

    public async Task<GetInteractionDetailResult> Handle(GetInteractionByIdCommand request, CancellationToken cancellationToken)
    {
        if (await interactionStorage.IsEmpty())
        {
            return new GetInteractionDetailResult() {Success = false, ErrorMessage = "no interactions found"};
        }

        var interaction = interactionStorage.GetInteractionById(request.InteractionId).Result;

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

        return new GetInteractionDetailResult() {Success = true, Interaction = interactionShowDto};
    }
}