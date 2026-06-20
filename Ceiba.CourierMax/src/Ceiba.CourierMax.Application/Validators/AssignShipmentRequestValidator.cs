using Ceiba.CourierMax.Application.DTOs.Requests;
using FluentValidation;

namespace Ceiba.CourierMax.Application.Validators;

public sealed class AssignShipmentRequestValidator : AbstractValidator<AssignShipmentRequest>
{
    public AssignShipmentRequestValidator()
    {
        RuleFor(x => x.DriverId).NotEmpty().WithMessage("El identificador del conductor es obligatorio.");
        RuleFor(x => x.AssignedBy).NotEmpty().WithMessage("El campo 'asignado por' es obligatorio.");
    }
}
