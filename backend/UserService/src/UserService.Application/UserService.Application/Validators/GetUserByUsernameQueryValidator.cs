using FluentValidation;
using UserService.Application.Queries;

namespace UserService.Application.Validators;

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