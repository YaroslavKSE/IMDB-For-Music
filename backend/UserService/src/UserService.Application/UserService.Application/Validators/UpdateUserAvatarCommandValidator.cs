using FluentValidation;
using UserService.Application.Commands;

namespace UserService.Application.Validators;
public class UpdateUserAvatarCommandValidator : AbstractValidator<UpdateUserAvatarCommand>
{
    public UpdateUserAvatarCommandValidator()
    {
        RuleFor(x => x.Auth0UserId)
            .NotEmpty()
            .WithMessage("Auth0 user ID is required");

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("Avatar file is required");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(5 * 1024 * 1024) // 5MB max
            .When(x => x.File != null)
            .WithMessage("File size must not exceed 5MB");

        RuleFor(x => x.File.ContentType)
            .Must(BeValidImageType)
            .When(x => x.File != null)
            .WithMessage("File must be a valid image type (JPEG, PNG, or GIF)");
    }

    private bool BeValidImageType(string contentType)
    {
        return contentType == "image/jpeg" ||
               contentType == "image/png" ||
               contentType == "image/gif";
    }
}
