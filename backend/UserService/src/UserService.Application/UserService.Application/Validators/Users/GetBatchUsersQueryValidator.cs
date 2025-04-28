using FluentValidation;
using UserService.Application.Handlers.Users;
using UserService.Application.Queries.Users;

namespace UserService.Application.Validators.Users;

public class GetBatchUsersQueryValidator : AbstractValidator<GetBatchUsersQuery>
{
    public GetBatchUsersQueryValidator()
    {
        RuleFor(x => x.UserIds)
            .NotEmpty()
            .WithMessage("User IDs list cannot be empty");

        RuleFor(x => x.UserIds.Count)
            .LessThanOrEqualTo(100)
            .WithMessage("Maximum of 100 user IDs per request");

        RuleForEach(x => x.UserIds)
            .NotEmpty()
            .WithMessage("User ID cannot be empty");
    }
}