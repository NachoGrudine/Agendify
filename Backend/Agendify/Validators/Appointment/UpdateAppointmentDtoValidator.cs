﻿using Agendify.DTOs.Appointment;
using FluentValidation;

namespace Agendify.Validators.Appointment;

public class UpdateAppointmentDtoValidator : AbstractValidator<UpdateAppointmentDto>
{
    public UpdateAppointmentDtoValidator()
    {
        RuleFor(x => x.ProviderId)
            .GreaterThan(0).WithMessage("El ID del proveedor es requerido");
        
        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("La fecha de inicio es requerida")
            .LessThan(x => x.EndTime).WithMessage("La fecha de inicio debe ser anterior a la fecha de fin");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("La fecha de fin es requerida")
            .GreaterThan(x => x.StartTime).WithMessage("La fecha de fin debe ser posterior a la fecha de inicio");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("El estado no es válido");
    }
}

