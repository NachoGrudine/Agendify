using Agendify.DTOs.Auth;
using FluentValidation;

namespace Agendify.Validators.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido")
            .EmailAddress().WithMessage("El email debe tener un formato válido")
            .MaximumLength(200).WithMessage("El email no puede exceder 200 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida")
            .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres")
            .MaximumLength(100).WithMessage("La contraseña no puede exceder 100 caracteres");

        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("El nombre del negocio es requerido")
            .MaximumLength(200).WithMessage("El nombre del negocio no puede exceder 200 caracteres");

        RuleFor(x => x.Industry)
            .NotEmpty().WithMessage("La industria es requerida")
            .MaximumLength(100).WithMessage("La industria no puede exceder 100 caracteres");

        RuleFor(x => x.ProviderName)
            .NotEmpty().WithMessage("El nombre del proveedor es requerido")
            .MaximumLength(200).WithMessage("El nombre del proveedor no puede exceder 200 caracteres");

        // ProviderSpecialty es opcional
        RuleFor(x => x.ProviderSpecialty)
            .MaximumLength(200).WithMessage("La especialidad no puede exceder 200 caracteres")
            .When(x => !string.IsNullOrEmpty(x.ProviderSpecialty));
    }
}

