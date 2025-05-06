using FluentValidation;
using UserService.Application.Queries.Preferences;

namespace UserService.Application.Validators.Preferences;

public class GetUserPreferencesByUsernameQueryValidator : AbstractValidator<GetUserPreferencesByUsernameQuery>
{
    public GetUserPreferencesByUsernameQueryValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage("Username format is invalid");
    }
}