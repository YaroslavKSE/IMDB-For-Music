using FluentValidation;
using UserService.Application.Commands;

namespace UserService.Application.Validators;

public class UnsubscribeFromUserCommandValidator : AbstractValidator<UnsubscribeFromUserCommand>
{
    public UnsubscribeFromUserCommandValidator()
    {
        RuleFor(x => x.FollowerId)
            .NotEmpty()
            .WithMessage("Follower ID is required");

        RuleFor(x => x.FollowedId)
            .NotEmpty()
            .WithMessage("Followed user ID is required");

        RuleFor(x => x)
            .Must(x => x.FollowerId != x.FollowedId)
            .WithMessage("Users cannot unsubscribe from themselves");
    }
}