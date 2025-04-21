using MediatR;
using MusicLists.Application.Commands;
using MusicLists.Application.DTOs;
using MusicLists.Application.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MusicLists.Application.UseCases;

public class AddListLikeUseCase : IRequestHandler<AddListLikeCommand, AddListLikeResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public AddListLikeUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<AddListLikeResult> Handle(AddListLikeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var listLike = await _musicListsStorage.AddListLikeAsync(request.ListId, request.UserId);

            return new AddListLikeResult
            {
                Success = true,
                Like = new ListLikeDto
                {
                    LikeId = listLike.LikeId,
                    ListId = listLike.ListId,
                    UserId = listLike.UserId,
                    LikedAt = listLike.LikedAt
                }
            };
        }
        catch (KeyNotFoundException ex)
        {
            return new AddListLikeResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (InvalidOperationException ex)
        {
            return new AddListLikeResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new AddListLikeResult
            {
                Success = false,
                ErrorMessage = $"Error adding list like: {ex.Message}"
            };
        }
    }
}