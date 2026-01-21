using Agendify.API.DTOs.Service;
using FluentValidation;

namespace Agendify.API.Validators.Service;

public class UpdateServiceDtoValidator : AbstractValidator<UpdateServiceDto>
{
    public UpdateServiceDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.DefaultDuration)
            .GreaterThan(0).WithMessage("La duración debe ser mayor a 0 minutos")
            .LessThanOrEqualTo(1440).WithMessage("La duración no puede exceder 1440 minutos (24 horas)");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo")
            .LessThan(1000000).WithMessage("El precio no puede exceder 999,999");
    }
}

