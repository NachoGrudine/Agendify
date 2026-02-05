using Agendify.DTOs.ProviderSchedule;
using FluentValidation;

namespace Agendify.Validators.ProviderSchedule;

public class CreateProviderScheduleDtoValidator : AbstractValidator<CreateProviderScheduleDto>
{
    public CreateProviderScheduleDtoValidator()
    {
        RuleFor(x => x.ProviderId)
            .GreaterThan(0).WithMessage("El ID del proveedor es requerido");

        RuleFor(x => x.DayOfWeek)
            .IsInEnum().WithMessage("El día de la semana no es válido");

        RuleFor(x => x.StartTime)
            .Must(time => time >= TimeSpan.Zero && time < TimeSpan.FromHours(24))
            .WithMessage("La hora de inicio debe estar entre 00:00 y 23:59");

        RuleFor(x => x.EndTime)
            .Must(time => time > TimeSpan.Zero && time <= TimeSpan.FromHours(24))
            .WithMessage("La hora de fin debe estar entre 00:01 y 24:00")
            .GreaterThan(x => x.StartTime)
            .WithMessage("La hora de fin debe ser posterior a la hora de inicio");
    }
}
