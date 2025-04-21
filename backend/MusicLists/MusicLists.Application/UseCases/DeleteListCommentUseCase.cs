using MediatR;
using MusicLists.Application.Commands;
using MusicLists.Application.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MusicLists.Application.UseCases;

public class DeleteListCommentUseCase : IRequestHandler<DeleteListCommentCommand, DeleteListCommentResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public DeleteListCommentUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<DeleteListCommentResult> Handle(DeleteListCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            bool result = await _musicListsStorage.DeleteListComment(request.CommentId, request.UserId);

            return new DeleteListCommentResult
            {
                Success = result,
                ErrorMessage = result ? null : "List comment not found or you don't have permission to delete it"
            };
        }
        catch (Exception ex)
        {
            return new DeleteListCommentResult
            {
                Success = false,
                ErrorMessage = $"Error deleting list comment: {ex.Message}"
            };
        }
    }
}