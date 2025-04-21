namespace MusicLists.Application.UseCases;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MusicLists.Application.Commands;
using MusicLists.Application.Results;
using MusicLists.Domain;

public class InsertListItemUseCase : IRequestHandler<InsertListItemCommand, InsertListItemResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public InsertListItemUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<InsertListItemResult> Handle(InsertListItemCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Call the storage to insert the item
            var result = await _musicListsStorage.InsertListItemAsync(
                request.ListId,
                request.SpotifyId,
                request.Position);

            return new InsertListItemResult
            {
                Success = true,
                NewPosition = request.Position,
                TotalItems = result
            };
        }
        catch (KeyNotFoundException ex)
        {
            return new InsertListItemResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new InsertListItemResult
            {
                Success = false,
                ErrorMessage = $"Error inserting list item: {ex.Message}"
            };
        }
    }
}