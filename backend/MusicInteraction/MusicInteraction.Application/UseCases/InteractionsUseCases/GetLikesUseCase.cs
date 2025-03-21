using MediatR;
using MusicInteraction.Application.Interfaces;

namespace MusicInteraction.Application;

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