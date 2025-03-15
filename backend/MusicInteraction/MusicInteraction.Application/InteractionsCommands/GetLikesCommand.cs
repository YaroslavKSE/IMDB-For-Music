using MediatR;
using MusicInteraction.Application.Interfaces;
using MusicInteraction.Domain;
namespace MusicInteraction.Application;

public class GetLikesCommand: IRequest<GetLikesResult> { }

public class GetLikesResult
{
    public bool LikesEmpty { get; set; }
    public List<Like> Likes { get; set; }
}

public class GetLikesUseCase : IRequestHandler<GetLikesCommand, GetLikesResult>
{
    private readonly IInteractionStorage interactionStorage;

    public GetLikesUseCase(IInteractionStorage interactionStorage)
    {
        this.interactionStorage = interactionStorage;
    }

    public async Task<GetLikesResult> Handle(GetLikesCommand request, CancellationToken cancellationToken)
    {
        if (await interactionStorage.IsEmpty())
        {
            return new GetLikesResult() {LikesEmpty = true};
        }
        return new GetLikesResult() {LikesEmpty = false, Likes = interactionStorage.GetLikes().Result};
    }
}