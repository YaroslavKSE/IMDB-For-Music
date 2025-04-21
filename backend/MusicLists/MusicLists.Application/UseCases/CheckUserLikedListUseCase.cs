using MediatR;
using MusicLists.Application.Commands;
using MusicLists.Application.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MusicLists.Application.UseCases;

public class CheckUserLikedListUseCase : IRequestHandler<CheckUserLikedListCommand, CheckUserLikedListResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public CheckUserLikedListUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<CheckUserLikedListResult> Handle(CheckUserLikedListCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool hasLiked = await _musicListsStorage.HasUserLikedList(request.ListId, request.UserId);

            return new CheckUserLikedListResult
            {
                Success = true,
                HasLiked = hasLiked
            };
        }
        catch (Exception ex)
        {
            return new CheckUserLikedListResult
            {
                Success = false,
                ErrorMessage = $"Error checking if user liked list: {ex.Message}"
            };
        }
    }
}