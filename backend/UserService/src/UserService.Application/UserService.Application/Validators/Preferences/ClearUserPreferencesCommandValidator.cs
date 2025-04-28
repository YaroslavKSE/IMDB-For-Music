using FluentValidation;
using UserService.Application.Commands;
using UserService.Application.Queries.Preferences;

namespace UserService.Application.Validators.Preferences;

public class ClearUserPreferencesCommandValidator : AbstractValidator<ClearUserPreferencesCommand>
{
    public ClearUserPreferencesCommandValidator()
    {
        RuleFor(x => x.Auth0UserId)
            .NotEmpty()
            .WithMessage("Auth0 user ID is required");

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Type is required")
            .Must(BeValidItemType)
            .WithMessage("Type must be 'artist', 'album', or 'track'");
    }

    private bool BeValidItemType(string itemType)
    {
        return itemType.ToLower() switch
        {
            "artist" => true,
            "album" => true,
            "track" => true,
            _ => false
        };
    }
}