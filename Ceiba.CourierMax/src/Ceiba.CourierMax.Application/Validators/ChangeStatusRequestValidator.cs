using Ceiba.CourierMax.Application.DTOs.Requests;
using FluentValidation;

namespace Ceiba.CourierMax.Application.Validators;

public sealed class ChangeStatusRequestValidator : AbstractValidator<ChangeStatusRequest>
{
    public ChangeStatusRequestValidator()
    {
        RuleFor(x => x.ChangedBy).NotEmpty().WithMessage("El campo 'modificado por' es obligatorio.");
    }
}
