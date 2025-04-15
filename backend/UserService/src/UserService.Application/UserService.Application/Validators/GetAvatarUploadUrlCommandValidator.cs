using FluentValidation;
using UserService.Application.Commands;

namespace UserService.Application.Validators;

public class GetAvatarUploadUrlCommandValidator : AbstractValidator<GetAvatarUploadUrlCommand>
{
    public GetAvatarUploadUrlCommandValidator()
    {
        RuleFor(x => x.Auth0UserId)
            .NotEmpty()
            .WithMessage("Auth0 user ID is required");
            
        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Content type is required")
            .Must(BeValidImageType)
            .WithMessage("Content type must be a valid image type (JPEG, PNG, or GIF)");
    }

    private bool BeValidImageType(string contentType)
    {
        return contentType == "image/jpeg" ||
               contentType == "image/png" ||
               contentType == "image/gif" ||
               contentType == "image/heic" ||
               contentType == "image/heif";
    }
}