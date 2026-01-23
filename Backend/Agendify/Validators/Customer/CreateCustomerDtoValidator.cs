﻿using Agendify.DTOs.Customer;
using FluentValidation;

namespace Agendify.Validators.Customer;

public class CreateCustomerDtoValidator : AbstractValidator<CreateCustomerDto>
{
    public CreateCustomerDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre es requerido")
            .MaximumLength(200).WithMessage("El nombre no puede exceder 200 caracteres");

        RuleFor(x => x.Phone)
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .When(x => x.Phone != null)
            .WithMessage("El teléfono debe tener un formato válido (ej: +5491112345678)")
            .MaximumLength(20).WithMessage("El teléfono no puede exceder 20 caracteres");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => x.Email != null)
            .WithMessage("El email debe tener un formato válido")
            .MaximumLength(200).WithMessage("El email no puede exceder 200 caracteres");
    }
}
