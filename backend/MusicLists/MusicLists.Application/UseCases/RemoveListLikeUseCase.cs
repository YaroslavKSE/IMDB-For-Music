using MediatR;
using MusicLists.Application.Commands;
using MusicLists.Application.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MusicLists.Application.UseCases;

public class RemoveListLikeUseCase : IRequestHandler<RemoveListLikeCommand, RemoveListLikeResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public RemoveListLikeUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<RemoveListLikeResult> Handle(RemoveListLikeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool result = await _musicListsStorage.RemoveListLike(request.ListId, request.UserId);

            return new RemoveListLikeResult
            {
                Success = result,
                ErrorMessage = result ? null : "List like not found"
            };
        }
        catch (Exception ex)
        {
            return new RemoveListLikeResult
            {
                Success = false,
                ErrorMessage = $"Error removing list like: {ex.Message}"
            };
        }
    }
}