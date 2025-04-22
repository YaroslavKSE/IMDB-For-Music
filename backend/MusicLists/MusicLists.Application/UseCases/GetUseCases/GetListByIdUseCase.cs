namespace MusicLists.Application.UseCases;

using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Commands;
using DTOs;
using Results;

public class GetListByIdUseCase : IRequestHandler<GetListByIdCommand, GetListDetailResult>
{
    private readonly IMusicListsStorage _musicListsStorage;

    public GetListByIdUseCase(IMusicListsStorage musicListsStorage)
    {
        _musicListsStorage = musicListsStorage;
    }

    public async Task<GetListDetailResult> Handle(GetListByIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var list = await _musicListsStorage.GetListByIdAsync(request.ListId);

            var listDetailDto = new ListDetailDto
            {
                ListId = list.ListId,
                UserId = list.UserId,
                ListType = list.ListType,
                CreatedAt = list.CreatedAt,
                ListName = list.ListName,
                ListDescription = list.ListDescription,
                IsRanked = list.IsRanked,
                Items = list.Items.Select(i => new ListItemDto
                {
                    SpotifyId = i.SpotifyId,
                    Number = i.Number
                }).ToList(),
                TotalItems = list.TotalItems,
                Likes = list.Likes,
                Comments = list.Comments
            };

            return new GetListDetailResult
            {
                Success = true,
                List = listDetailDto
            };
        }
        catch (KeyNotFoundException ex)
        {
            return new GetListDetailResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new GetListDetailResult
            {
                Success = false,
                ErrorMessage = $"Error retrieving list: {ex.Message}"
            };
        }
    }
}