using FluentValidation;
using UserService.Application.Queries;
using UserService.Application.Queries.Users;

namespace UserService.Application.Validators.Users;

public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID cannot be empty");
    }
}