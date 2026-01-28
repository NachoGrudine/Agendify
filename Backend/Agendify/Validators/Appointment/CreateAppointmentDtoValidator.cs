using Agendify.DTOs.Appointment;
using FluentValidation;

namespace Agendify.Validators.Appointment;

public class CreateAppointmentDtoValidator : AbstractValidator<CreateAppointmentDto>
{
    public CreateAppointmentDtoValidator()
    {
        RuleFor(x => x.ProviderId)
            .GreaterThan(0).WithMessage("El ID del proveedor es requerido");
        
        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("La fecha de inicio es requerida")
            .LessThan(x => x.EndTime).WithMessage("La fecha de inicio debe ser anterior a la fecha de fin");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("La fecha de fin es requerida")
            .GreaterThan(x => x.StartTime).WithMessage("La fecha de fin debe ser posterior a la fecha de inicio");

        // Validación: Si se proporciona CustomerId, debe ser mayor a 0
        RuleFor(x => x.CustomerId)
            .GreaterThan(0)
            .When(x => x.CustomerId.HasValue)
            .WithMessage("El ID del cliente debe ser mayor a 0 si se proporciona");

        // Validación: Si se proporciona CustomerName, no debe estar vacío
        RuleFor(x => x.CustomerName)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.CustomerName))
            .WithMessage("El nombre del cliente no puede estar vacío si se proporciona");

        // Validación: Si se proporciona ServiceId, debe ser mayor a 0
        RuleFor(x => x.ServiceId)
            .GreaterThan(0)
            .When(x => x.ServiceId.HasValue)
            .WithMessage("El ID del servicio debe ser mayor a 0 si se proporciona");

        // Validación: Si se proporciona ServiceName, no debe estar vacío
        RuleFor(x => x.ServiceName)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.ServiceName))
            .WithMessage("El nombre del servicio no puede estar vacío si se proporciona");
    }
}
