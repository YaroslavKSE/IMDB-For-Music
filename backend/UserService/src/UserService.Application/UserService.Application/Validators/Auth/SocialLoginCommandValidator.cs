using FluentValidation;
using UserService.Application.Commands;

namespace UserService.Application.Validators.Auth;

public class SocialLoginCommandValidator : AbstractValidator<SocialLoginCommand>
{
    public SocialLoginCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty()
            .WithMessage("Access token is required");

        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("Provider is required");
    }
}