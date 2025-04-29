using FluentValidation;
using UserService.Application.Commands;
using UserService.Application.Queries.Preferences;

namespace UserService.Application.Validators.Preferences;

public class AddUserPreferenceCommandValidator : AbstractValidator<AddUserPreferenceCommand>
{
    public AddUserPreferenceCommandValidator()
    {
        RuleFor(x => x.Auth0UserId)
            .NotEmpty()
            .WithMessage("Auth0 user ID is required");

        RuleFor(x => x.ItemType)
            .NotEmpty()
            .WithMessage("Item type is required")
            .Must(BeValidItemType)
            .WithMessage("Item type must be 'artist', 'album', or 'track'");

        RuleFor(x => x.SpotifyId)
            .NotEmpty()
            .WithMessage("Spotify ID is required");
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