using BachataEvents.Application.Auth;
using BachataEvents.Domain.Constants;
using FluentValidation;

namespace BachataEvents.Application.Validation;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => r == AppRoles.User || r == AppRoles.Organizer)
            .WithMessage($"Role must be '{AppRoles.User}' or '{AppRoles.Organizer}'.");

        When(x => x.Role == AppRoles.Organizer, () =>
        {
            RuleFor(x => x.Organizer).NotNull();
            RuleFor(x => x.Organizer!.DisplayName).NotEmpty().MaximumLength(120);
            RuleFor(x => x.Organizer!.Website).MaximumLength(500);
            RuleFor(x => x.Organizer!.Instagram).MaximumLength(200);
        });

        When(x => x.Role == AppRoles.User, () =>
        {
            RuleFor(x => x.Organizer).Null();
        });
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(128);
    }
}
