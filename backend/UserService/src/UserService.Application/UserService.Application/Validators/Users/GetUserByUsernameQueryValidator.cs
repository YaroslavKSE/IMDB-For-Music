using FluentValidation;
using UserService.Application.Queries;
using UserService.Application.Queries.Users;

namespace UserService.Application.Validators.Users;

public class GetUserByUsernameQueryValidator : AbstractValidator<GetUserByUsernameQuery>
{
    public GetUserByUsernameQueryValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage("Username format is invalid");
    }
}