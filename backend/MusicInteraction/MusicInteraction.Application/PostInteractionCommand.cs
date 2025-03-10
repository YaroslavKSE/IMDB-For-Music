using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;

public class PostInteractionCommand : IRequest<PostInteractionResult>
{
    public string UserId;
    public string ItemId;
    public string ItemType;
    public bool IsLiked;
    public string ReviewText;
    public float? Grade;
}

public class PostInteractionResult
{
    public bool InteractionCreated;
    public bool Liked;
    public bool ReviewCreated;
    public bool Graded;
    public Guid InteractionId;
}

public class PostInteractionRequest
{
    public string UserId;
    public string ItemId;
    public string ItemType;
    public bool IsLiked;
    public string ReviewText;
    public float? Grade;
}

public class PostInteractionUseCase : IRequestHandler<PostInteractionCommand, PostInteractionResult>
{
    private readonly IInteractionStorage interactionStorage;

    public PostInteractionUseCase(IInteractionStorage interactionStorage)
    {
        this.interactionStorage = interactionStorage;
    }

    public async Task<PostInteractionResult> Handle(PostInteractionCommand request, CancellationToken cancellationToken)
    {
        var result = new PostInteractionResult(){InteractionCreated = false, Liked = false, ReviewCreated = false, Graded = false};
        var interaction = new InteractionsAggregate(request.UserId, request.ItemId, request.ItemType);
        result.InteractionCreated = true;
        result.InteractionId = interaction.AggregateId;

        if (request.Grade.HasValue)
        {
            Grade grade = new Grade();
            grade.updateGrade(request.Grade.Value);
            interaction.AddRating(grade);
            result.Graded = true;
        }

        if (request.IsLiked)
        {
            interaction.AddLike();
            result.Liked = true;
        }

        if (!string.IsNullOrEmpty(request.ReviewText))
        {
            interaction.AddReview(request.ReviewText);
            result.ReviewCreated = true;
        }

        await interactionStorage.AddInteractionAsync(interaction);

        return result;
    }
}


