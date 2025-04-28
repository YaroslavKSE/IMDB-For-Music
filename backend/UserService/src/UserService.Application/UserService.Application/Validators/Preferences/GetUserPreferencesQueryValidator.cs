using FluentValidation;
using UserService.Application.Queries.Preferences;

namespace UserService.Application.Validators.Preferences;

public class GetUserPreferencesQueryValidator : AbstractValidator<GetUserPreferencesQuery>
{
    public GetUserPreferencesQueryValidator()
    {
        RuleFor(x => x.Auth0UserId)
            .NotEmpty()
            .WithMessage("Auth0 user ID is required");
    }
}