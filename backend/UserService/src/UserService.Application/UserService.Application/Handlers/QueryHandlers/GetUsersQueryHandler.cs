using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Queries;
using UserService.Domain.Interfaces;

namespace UserService.Application.Handlers.QueryHandlers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedUsersResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<GetUsersQueryHandler> _logger;
    private readonly IValidator<GetUsersQuery> _validator;

    public GetUsersQueryHandler(
        IUserRepository userRepository,
        ILogger<GetUsersQueryHandler> logger,
        IValidator<GetUsersQuery> validator)
    {
        _userRepository = userRepository;
        _logger = logger;
        _validator = validator;
    }

    public async Task<PaginatedUsersResponse> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        // Validate the request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        _logger.LogInformation("Getting paginated users: Page {Page}, PageSize {PageSize}, SearchTerm: {SearchTerm}",
            request.Page, request.PageSize, request.SearchTerm ?? "none");

        // Get users with pagination and search
        var (users, totalCount) = await _userRepository.GetPaginatedUsersAsync(
            request.Page,
            request.PageSize,
            request.SearchTerm,
            cancellationToken);

        // Calculate total pages
        var totalPages = (int) Math.Ceiling(totalCount / (double) request.PageSize);

        // Map to response
        var response = new PaginatedUsersResponse
        {
            Items = users.Select(u => new UserSummaryDto
            {
                Id = u.Id,
                Username = u.Username,
                Name = u.Name,
                Surname = u.Surname,
                AvatarUrl = u.AvatarUrl,
            }).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        _logger.LogInformation("Retrieved {Count} users out of {TotalCount} total, {TotalPages} pages",
            users.Count, totalCount, totalPages);

        return response;
    }
}