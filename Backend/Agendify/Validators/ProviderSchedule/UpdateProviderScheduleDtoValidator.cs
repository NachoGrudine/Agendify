﻿using Agendify.DTOs.ProviderSchedule;
using FluentValidation;

namespace Agendify.Validators.ProviderSchedule;

public class UpdateProviderScheduleDtoValidator : AbstractValidator<UpdateProviderScheduleDto>
{
    public UpdateProviderScheduleDtoValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .IsInEnum().WithMessage("El día de la semana no es válido");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("La hora de inicio es requerida")
            .Must(time => time >= TimeSpan.Zero && time < TimeSpan.FromHours(24))
            .WithMessage("La hora de inicio debe estar entre 00:00 y 23:59");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("La hora de fin es requerida")
            .Must(time => time >= TimeSpan.Zero && time <= TimeSpan.FromHours(24))
            .WithMessage("La hora de fin debe estar entre 00:00 y 24:00")
            .GreaterThan(x => x.StartTime)
            .WithMessage("La hora de fin debe ser posterior a la hora de inicio");
    }
}

