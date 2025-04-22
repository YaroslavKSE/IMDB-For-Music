using MediatR;
using MusicLists.Application.Commands;
using MusicLists.Application.DTOs;
using MusicLists.Application.Results;
using MusicLists.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MusicLists.Application.UseCases;

public class CreateMusicListUseCase : IRequestHandler<CreateMusicListCommand, CreateMusicListResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public CreateMusicListUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<CreateMusicListResult> Handle(CreateMusicListCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create domain entities
            var listItems = new List<ListItem>();
            foreach (var item in request.Items)
            {
                listItems.Add(new ListItem(item.SpotifyId, item.Number));
            }

            var list = new List(
                request.UserId,
                request.ListType,
                request.ListName,
                request.ListDescription,
                request.IsRanked
            )
            {
                Items = listItems
            };

            // Save to storage
            await _musicListsStorage.CreateListAsync(list);

            return new CreateMusicListResult
            {
                Success = true,
                ListId = list.ListId
            };
        }
        catch (Exception ex)
        {
            return new CreateMusicListResult
            {
                Success = false,
                ErrorMessage = $"Error creating list: {ex.Message}"
            };
        }
    }
}