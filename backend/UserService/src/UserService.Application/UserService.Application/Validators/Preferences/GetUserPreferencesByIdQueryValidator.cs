using FluentValidation;
using UserService.Application.Queries.Preferences;

namespace UserService.Application.Validators.Preferences;

public class GetUserPreferencesByIdQueryValidator : AbstractValidator<GetUserPreferencesByIdQuery>
{
    public GetUserPreferencesByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}