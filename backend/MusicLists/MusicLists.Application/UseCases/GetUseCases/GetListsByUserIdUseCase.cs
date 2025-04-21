namespace MusicLists.Application.UseCases;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Commands;
using DTOs;
using Results;

public class GetListsByUserIdUseCase : IRequestHandler<GetListsByUserIdCommand, GetListsOverviewResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public GetListsByUserIdUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<GetListsOverviewResult> Handle(GetListsByUserIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var paginatedLists = await _musicListsStorage.GetListsByUserIdAsync(request.UserId, request.Limit, request.Offset);

            var listDtos = paginatedLists.Items.Select(l => new ListOverviewDto
            {
                ListId = l.ListId,
                UserId = l.UserId,
                ListType = l.ListType,
                CreatedAt = l.CreatedAt,
                ListName = l.ListName,
                ListDescription = l.ListDescription,
                IsRanked = l.IsRanked,
                PreviewItems = l.Items.Select(i => new ListItemDto
                {
                    SpotifyId = i.SpotifyId,
                    Number = i.Number
                }).ToList(),
                TotalItems = l.TotalItems,
                Likes = l.Likes,
                Comments = l.Comments
            }).ToList();

            return new GetListsOverviewResult
            {
                Success = true,
                Lists = listDtos,
                TotalCount = paginatedLists.TotalCount
            };
        }
        catch (Exception ex)
        {
            return new GetListsOverviewResult
            {
                Success = false,
                ErrorMessage = $"Error retrieving user lists: {ex.Message}",
                Lists = new List<ListOverviewDto>()
            };
        }
    }
}