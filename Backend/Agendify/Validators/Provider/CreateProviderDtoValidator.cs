﻿using Agendify.DTOs.Provider;
using FluentValidation;

namespace Agendify.Validators.Provider;

public class CreateProviderDtoValidator : AbstractValidator<CreateProviderDto>
{
    public CreateProviderDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Specialty)
            .NotEmpty().WithMessage("La especialidad es requerida")
            .MaximumLength(200).WithMessage("La especialidad no puede exceder 200 caracteres");
    }
}
