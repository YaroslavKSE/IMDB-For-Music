using FluentValidation;
using UserService.Application.Queries;

namespace UserService.Application.Validators;

public class GetUserFollowersQueryValidator : AbstractValidator<GetUserFollowersQuery>
{
    public GetUserFollowersQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(100)
            .WithMessage("Page size must be between 1 and 100");
    }
}