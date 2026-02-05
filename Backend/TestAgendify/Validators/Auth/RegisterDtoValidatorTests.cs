using Agendify.DTOs.Auth;
using Agendify.Validators.Auth;
using FluentValidation.TestHelper;

namespace TestAgendify.Validators.Auth;

public class RegisterDtoValidatorTests
{
    private readonly RegisterDtoValidator _validator;

    public RegisterDtoValidatorTests()
    {
        _validator = new RegisterDtoValidator();
    }

    #region Email Tests

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        var dto = new RegisterDto
        {
            Email = string.Empty,
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("El email es requerido");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Has_Invalid_Format()
    {
        var dto = new RegisterDto
        {
            Email = "invalid-email",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("El email debe tener un formato válido");
    }

    [Fact]
    public void Should_Have_Error_When_Email_Exceeds_MaxLength()
    {
        var longEmail = new string('a', 190) + "@example.com";
        var dto = new RegisterDto
        {
            Email = longEmail,
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("El email no puede exceder 200 caracteres");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Email_Is_Valid()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Password Tests

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = string.Empty,
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("La contraseña es requerida");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Short()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "12345",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("La contraseña debe tener al menos 6 caracteres");
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Too_Long()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = new string('a', 101),
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("La contraseña no puede exceder 100 caracteres");
    }

    [Theory]
    [InlineData("password")]
    [InlineData("123456")]
    [InlineData("MySecureP@ssw0rd")]
    public void Should_Not_Have_Error_When_Password_Is_Valid(string password)
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = password,
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region BusinessName Tests

    [Fact]
    public void Should_Have_Error_When_BusinessName_Is_Empty()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = string.Empty,
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BusinessName)
            .WithErrorMessage("El nombre del negocio es requerido");
    }

    [Fact]
    public void Should_Have_Error_When_BusinessName_Exceeds_MaxLength()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = new string('A', 201),
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.BusinessName)
            .WithErrorMessage("El nombre del negocio no puede exceder 200 caracteres");
    }

    [Fact]
    public void Should_Not_Have_Error_When_BusinessName_Is_Valid()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.BusinessName);
    }

    #endregion

    #region Industry Tests

    [Fact]
    public void Should_Have_Error_When_Industry_Is_Empty()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = string.Empty,
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Industry)
            .WithErrorMessage("La industria es requerida");
    }

    [Fact]
    public void Should_Have_Error_When_Industry_Exceeds_MaxLength()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = new string('A', 101),
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Industry)
            .WithErrorMessage("La industria no puede exceder 100 caracteres");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Industry_Is_Valid()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Industry);
    }

    #endregion

    #region ProviderName Tests

    [Fact]
    public void Should_Have_Error_When_ProviderName_Is_Empty()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = string.Empty
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ProviderName)
            .WithErrorMessage("El nombre del proveedor es requerido");
    }

    [Fact]
    public void Should_Have_Error_When_ProviderName_Exceeds_MaxLength()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = new string('A', 201)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ProviderName)
            .WithErrorMessage("El nombre del proveedor no puede exceder 200 caracteres");
    }

    [Fact]
    public void Should_Not_Have_Error_When_ProviderName_Is_Valid()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ProviderName);
    }

    #endregion

    #region ProviderSpecialty Tests

    [Fact]
    public void Should_Not_Have_Error_When_ProviderSpecialty_Is_Null()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe",
            ProviderSpecialty = null
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ProviderSpecialty);
    }

    [Fact]
    public void Should_Not_Have_Error_When_ProviderSpecialty_Is_Empty()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe",
            ProviderSpecialty = string.Empty
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ProviderSpecialty);
    }

    [Fact]
    public void Should_Have_Error_When_ProviderSpecialty_Exceeds_MaxLength()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe",
            ProviderSpecialty = new string('A', 201)
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ProviderSpecialty)
            .WithErrorMessage("La especialidad no puede exceder 200 caracteres");
    }

    [Fact]
    public void Should_Not_Have_Error_When_ProviderSpecialty_Is_Valid()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe",
            ProviderSpecialty = "Full Stack Developer"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.ProviderSpecialty);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Should_Not_Have_Error_When_All_Required_Fields_Are_Valid()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var dto = new RegisterDto
        {
            Email = "test@example.com",
            Password = "password123",
            BusinessName = "My Business",
            Industry = "Technology",
            ProviderName = "John Doe",
            ProviderSpecialty = "Full Stack Developer"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_All_Required_Fields_Are_Empty()
    {
        var dto = new RegisterDto
        {
            Email = string.Empty,
            Password = string.Empty,
            BusinessName = string.Empty,
            Industry = string.Empty,
            ProviderName = string.Empty
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
        result.ShouldHaveValidationErrorFor(x => x.BusinessName);
        result.ShouldHaveValidationErrorFor(x => x.Industry);
        result.ShouldHaveValidationErrorFor(x => x.ProviderName);
    }

    #endregion
}

