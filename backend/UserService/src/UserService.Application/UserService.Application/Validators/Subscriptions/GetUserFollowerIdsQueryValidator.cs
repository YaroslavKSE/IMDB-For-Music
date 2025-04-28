using FluentValidation;
using UserService.Application.Queries.Subscriptions;

namespace UserService.Application.Validators.Subscriptions;

public class GetUserFollowerIdsQueryValidator : AbstractValidator<GetUserFollowerIdsQuery>
{
    public GetUserFollowerIdsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(5000) // Allow larger page sizes for interservice communication
            .WithMessage("Page size must be between 1 and 5000");
    }
}