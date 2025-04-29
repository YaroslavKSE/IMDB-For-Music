using FluentValidation;
using UserService.Application.Commands;

namespace UserService.Application.Validators.Avatar;

public class DeleteUserAvatarCommandValidator : AbstractValidator<DeleteUserAvatarCommand>
{
    public DeleteUserAvatarCommandValidator()
    {
        RuleFor(x => x.Auth0UserId)
            .NotEmpty()
            .WithMessage("Auth0 user ID is required");
    }
}