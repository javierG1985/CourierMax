using Ceiba.CourierMax.Application.DTOs.Requests;
using FluentValidation;

namespace Ceiba.CourierMax.Application.Validators;

public sealed class CancelShipmentRequestValidator : AbstractValidator<CancelShipmentRequest>
{
    public CancelShipmentRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("El motivo de cancelación es obligatorio.")
            .MinimumLength(5).WithMessage("El motivo de cancelación debe tener al menos 5 caracteres.");
        RuleFor(x => x.CancelledBy).NotEmpty().WithMessage("El campo 'cancelado por' es obligatorio.");
    }
}
