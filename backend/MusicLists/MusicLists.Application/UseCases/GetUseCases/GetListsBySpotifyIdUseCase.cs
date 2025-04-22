namespace MusicLists.Application.UseCases;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Commands;
using DTOs;
using Results;

public class GetListsBySpotifyIdUseCase : IRequestHandler<GetListsBySpotifyIdCommand, GetListsOverviewResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public GetListsBySpotifyIdUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<GetListsOverviewResult> Handle(GetListsBySpotifyIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var paginatedLists = await _musicListsStorage.GetListsBySpotifyIdAsync(request.SpotifyId, request.Limit, request.Offset, request.ListType);

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
                ErrorMessage = $"Error retrieving lists containing item: {ex.Message}",
                Lists = new List<ListOverviewDto>()
            };
        }
    }
}