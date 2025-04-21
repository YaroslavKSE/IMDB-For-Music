using MediatR;
using MusicLists.Application.Commands;
using MusicLists.Application.Results;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MusicLists.Application.UseCases;

public class DeleteMusicListUseCase : IRequestHandler<DeleteMusicListCommand, DeleteMusicListResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public DeleteMusicListUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<DeleteMusicListResult> Handle(DeleteMusicListCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _musicListsStorage.DeleteListAsync(request.ListId);

            return new DeleteMusicListResult
            {
                Success = true
            };
        }
        catch (KeyNotFoundException ex)
        {
            return new DeleteMusicListResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new DeleteMusicListResult
            {
                Success = false,
                ErrorMessage = $"Error deleting list: {ex.Message}"
            };
        }
    }
}