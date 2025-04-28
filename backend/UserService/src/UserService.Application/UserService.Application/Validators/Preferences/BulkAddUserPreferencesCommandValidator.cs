using FluentValidation;
using UserService.Application.Commands;
using UserService.Application.Queries.Preferences;

namespace UserService.Application.Validators.Preferences;

public class BulkAddUserPreferencesCommandValidator : AbstractValidator<BulkAddUserPreferencesCommand>
{
    public BulkAddUserPreferencesCommandValidator()
    {
        RuleFor(x => x.Auth0UserId)
            .NotEmpty()
            .WithMessage("Auth0 user ID is required");

        // Validate each artist ID if provided
        RuleForEach(x => x.Artists)
            .NotEmpty()
            .WithMessage("Artist IDs cannot be empty");

        // Validate each album ID if provided
        RuleForEach(x => x.Albums)
            .NotEmpty()
            .WithMessage("Album IDs cannot be empty");

        // Validate each track ID if provided
        RuleForEach(x => x.Tracks)
            .NotEmpty()
            .WithMessage("Track IDs cannot be empty");

        // Ensure at least one preference is provided
        RuleFor(x => x)
            .Must(cmd => cmd.Artists.Count > 0 || cmd.Albums.Count > 0 || cmd.Tracks.Count > 0)
            .WithMessage("At least one preference must be provided");
    }
}