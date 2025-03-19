using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

public class GetInteractionsCommand: IRequest<GetInteractionsResult>
{ }

public class GetInteractionsResult
{
    public bool InteractionsEmpty { get; set; }
    public List<InteractionAggregateDto> Interactions { get; set; }
}

public class InteractionAggregateDto
{
    public Guid AggregateId { get; set; }
    public string UserId { get; set; }
    public string ItemId { get; set; }
    public string ItemType { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual RatingNormalizedDTO? Rating { get; set; }
    public virtual ReviewDTO? Review { get; set; }
    public bool IsLiked { get; set; }
}

public class RatingNormalizedDTO
{
    public Guid RatingId { get; set; }
    public float? NormalizedGrade { get; set; }
}

public class ReviewDTO
{
    public Guid ReviewId { get; set; }
    public string ReviewText { get; set; }
}

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
        List<InteractionAggregateDto> interactionAggregateDtos = new List<InteractionAggregateDto>();

        foreach (var interaction in interactions)
        {
            InteractionAggregateDto interactionDTO = new InteractionAggregateDto();
            interactionDTO.AggregateId = interaction.AggregateId;
            interactionDTO.UserId = interaction.UserId;
            interactionDTO.ItemId = interaction.ItemId;
            interactionDTO.ItemType = interaction.ItemType;
            interactionDTO.CreatedAt = interaction.CreatedAt;
            interactionDTO.IsLiked = interaction.IsLiked;

            if (interaction.Rating != null)
            {
                interactionDTO.Rating = new RatingNormalizedDTO()
                    {RatingId = interaction.Rating.RatingId, NormalizedGrade = interaction.Rating.Grade.getNormalizedGrade()};
            }

            if (interaction.Review != null)
            {
                interactionDTO.Review = new ReviewDTO()
                    {ReviewId = interaction.Review.ReviewId, ReviewText = interaction.Review.ReviewText};
            }

            interactionAggregateDtos.Add(interactionDTO);
        }
        return new GetInteractionsResult() {InteractionsEmpty = false, Interactions = interactionAggregateDtos};
    }
}
