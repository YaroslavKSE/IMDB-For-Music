using MediatR;
using UserService.Application.DTOs;

namespace UserService.Application.Queries.Users;

public class GetUsersQuery : IRequest<PaginatedUsersResponse>
{
    public int Page { get; }
    public int PageSize { get; }
    public string SearchTerm { get; }

    public GetUsersQuery(int page, int pageSize, string searchTerm = null)
    {
        Page = page;
        PageSize = pageSize;
        SearchTerm = searchTerm;
    }
}