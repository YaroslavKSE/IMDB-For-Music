using FluentValidation;
using UserService.Application.Commands;

namespace UserService.Application.Validators;

public class SubscribeToUserCommandValidator : AbstractValidator<SubscribeToUserCommand>
{
    public SubscribeToUserCommandValidator()
    {
        RuleFor(x => x.FollowerId)
            .NotEmpty()
            .WithMessage("Follower ID is required");

        RuleFor(x => x.FollowedId)
            .NotEmpty()
            .WithMessage("Followed user ID is required");

        RuleFor(x => x)
            .Must(x => x.FollowerId != x.FollowedId)
            .WithMessage("Users cannot follow themselves");
    }
}