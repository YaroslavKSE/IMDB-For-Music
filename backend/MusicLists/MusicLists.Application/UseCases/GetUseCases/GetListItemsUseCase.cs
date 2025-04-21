namespace MusicLists.Application.UseCases;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Commands;
using DTOs;
using Results;

public class GetListItemsUseCase : IRequestHandler<GetListItemsCommand, GetListItemsResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public GetListItemsUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<GetListItemsResult> Handle(GetListItemsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var paginatedItems = await _musicListsStorage.GetListItemsByIdAsync(request.ListId, request.Limit, request.Offset);

            var itemDtos = paginatedItems.Items.Select(i => new ListItemDto
            {
                SpotifyId = i.SpotifyId,
                Number = i.Number
            }).ToList();

            return new GetListItemsResult
            {
                Success = true,
                Items = itemDtos,
                TotalCount = paginatedItems.TotalCount
            };
        }
        catch (KeyNotFoundException ex)
        {
            return new GetListItemsResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Items = new List<ListItemDto>()
            };
        }
        catch (Exception ex)
        {
            return new GetListItemsResult
            {
                Success = false,
                ErrorMessage = $"Error retrieving list items: {ex.Message}",
                Items = new List<ListItemDto>()
            };
        }
    }
}