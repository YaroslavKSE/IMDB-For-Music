using MediatR;
using MusicLists.Application.Commands;
using MusicLists.Application.DTOs;
using MusicLists.Application.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MusicLists.Application.UseCases;

public class AddListCommentUseCase : IRequestHandler<AddListCommentCommand, AddListCommentResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public AddListCommentUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<AddListCommentResult> Handle(AddListCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var listComment = await _musicListsStorage.AddListCommentAsync(request.ListId, request.UserId, request.CommentText);

            return new AddListCommentResult
            {
                Success = true,
                Comment = new ListCommentDto
                {
                    CommentId = listComment.CommentId,
                    ListId = listComment.ListId,
                    UserId = listComment.UserId,
                    CommentedAt = listComment.CommentedAt,
                    CommentText = listComment.CommentText
                }
            };
        }
        catch (KeyNotFoundException ex)
        {
            return new AddListCommentResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new AddListCommentResult
            {
                Success = false,
                ErrorMessage = $"Error adding list comment: {ex.Message}"
            };
        }
    }
}