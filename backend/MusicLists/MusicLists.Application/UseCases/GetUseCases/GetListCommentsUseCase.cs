namespace MusicLists.Application.UseCases;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Commands;
using DTOs;
using Results;

public class GetListCommentsUseCase : IRequestHandler<GetListCommentsCommand, GetListCommentsResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public GetListCommentsUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<GetListCommentsResult> Handle(GetListCommentsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var paginatedComments = await _musicListsStorage.GetListCommentsByIdAsync(request.ListId, request.Limit, request.Offset);

            var commentDtos = paginatedComments.Items.Select(c => new ListCommentDto
            {
                CommentId = c.CommentId,
                ListId = c.ListId,
                UserId = c.UserId,
                CommentedAt = c.CommentedAt,
                CommentText = c.CommentText
            }).ToList();

            return new GetListCommentsResult
            {
                Success = true,
                Comments = commentDtos,
                TotalCount = paginatedComments.TotalCount
            };
        }
        catch (KeyNotFoundException ex)
        {
            return new GetListCommentsResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Comments = new List<ListCommentDto>()
            };
        }
        catch (Exception ex)
        {
            return new GetListCommentsResult
            {
                Success = false,
                ErrorMessage = $"Error retrieving list comments: {ex.Message}",
                Comments = new List<ListCommentDto>()
            };
        }
    }
}