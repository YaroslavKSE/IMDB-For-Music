using FluentValidation;
using UserService.Application.Queries;
using UserService.Application.Queries.Users;

namespace UserService.Application.Validators.Users;

public class GetUserByEmailQueryValidator : AbstractValidator<GetUserByEmailQuery>
{
    public GetUserByEmailQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Please provide a valid email address");
    }
}