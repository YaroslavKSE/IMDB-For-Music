using FluentValidation;
using UserService.Application.Queries.Subscriptions;

namespace UserService.Application.Validators.Subscriptions;

public class CheckSubscriptionStatusQueryValidator : AbstractValidator<CheckSubscriptionStatusQuery>
{
    public CheckSubscriptionStatusQueryValidator()
    {
        RuleFor(x => x.FollowerId)
            .NotEmpty()
            .WithMessage("Follower ID is required");

        RuleFor(x => x.FollowedId)
            .NotEmpty()
            .WithMessage("Followed user ID is required");
    }
}