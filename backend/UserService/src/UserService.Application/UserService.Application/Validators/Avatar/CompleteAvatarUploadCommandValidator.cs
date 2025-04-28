using FluentValidation;
using UserService.Application.Commands;

namespace UserService.Application.Validators.Avatar;

public class CompleteAvatarUploadCommandValidator : AbstractValidator<CompleteAvatarUploadCommand>
{
    public CompleteAvatarUploadCommandValidator()
    {
        RuleFor(x => x.Auth0UserId)
            .NotEmpty()
            .WithMessage("Auth0 user ID is required");

        RuleFor(x => x.ObjectKey)
            .NotEmpty()
            .WithMessage("Object key is required");

        RuleFor(x => x.AvatarUrl)
            .NotEmpty()
            .WithMessage("Avatar URL is required");
    }
}