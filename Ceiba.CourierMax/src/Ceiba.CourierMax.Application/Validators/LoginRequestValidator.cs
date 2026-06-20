using Ceiba.CourierMax.Application.DTOs.Requests;
using FluentValidation;

namespace Ceiba.CourierMax.Application.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("El nombre de usuario es obligatorio.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("La contraseña es obligatoria.");
    }
}
