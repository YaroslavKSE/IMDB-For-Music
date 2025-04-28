using FluentValidation;
using UserService.Application.Queries.Subscriptions;

namespace UserService.Application.Validators.Subscriptions;

public class CheckBatchSubscriptionStatusQueryValidator : AbstractValidator<CheckBatchSubscriptionStatusQuery>
{
    public CheckBatchSubscriptionStatusQueryValidator()
    {
        RuleFor(x => x.FollowerId)
            .NotEmpty()
            .WithMessage("Follower ID is required");

        RuleFor(x => x.TargetUserIds)
            .NotEmpty()
            .WithMessage("Target user IDs list cannot be empty");

        RuleFor(x => x.TargetUserIds.Count)
            .LessThanOrEqualTo(100)
            .WithMessage("Maximum of 100 target user IDs per request");

        RuleForEach(x => x.TargetUserIds)
            .NotEmpty()
            .WithMessage("Target user ID cannot be empty");
    }
}