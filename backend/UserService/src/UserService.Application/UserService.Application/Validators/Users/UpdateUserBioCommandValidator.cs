using FluentValidation;
using UserService.Application.Commands;

namespace UserService.Application.Validators.Users;

public class UpdateUserBioCommandValidator : AbstractValidator<UpdateUserBioCommand>
{
    public UpdateUserBioCommandValidator()
    {
        RuleFor(x => x.Auth0UserId)
            .NotEmpty()
            .WithMessage("Auth0 user ID is required");

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .WithMessage("Bio must not exceed 500 characters");
    }
}