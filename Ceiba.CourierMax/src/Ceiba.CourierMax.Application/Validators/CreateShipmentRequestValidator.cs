using Ceiba.CourierMax.Application.DTOs.Requests;
using FluentValidation;

namespace Ceiba.CourierMax.Application.Validators;

public sealed class CreateShipmentRequestValidator : AbstractValidator<CreateShipmentRequest>
{
    public CreateShipmentRequestValidator()
    {
        RuleFor(x => x.SenderName).NotEmpty().WithMessage("El nombre del remitente es obligatorio.");
        RuleFor(x => x.SenderPhone).NotEmpty().WithMessage("El teléfono del remitente es obligatorio.");
        RuleFor(x => x.SenderAddress).NotEmpty().WithMessage("La dirección del remitente es obligatoria.");
        RuleFor(x => x.RecipientName).NotEmpty().WithMessage("El nombre del destinatario es obligatorio.");
        RuleFor(x => x.RecipientPhone).NotEmpty().WithMessage("El teléfono del destinatario es obligatorio.");
        RuleFor(x => x.RecipientAddress).NotEmpty().WithMessage("La dirección del destinatario es obligatoria.");
        RuleFor(x => x.WeightKg).GreaterThan(0).WithMessage("El peso debe ser mayor a 0.");
        RuleFor(x => x.LengthCm).GreaterThan(0).WithMessage("El largo debe ser mayor a 0.");
        RuleFor(x => x.WidthCm).GreaterThan(0).WithMessage("El ancho debe ser mayor a 0.");
        RuleFor(x => x.HeightCm).GreaterThan(0).WithMessage("El alto debe ser mayor a 0.");
    }
}
