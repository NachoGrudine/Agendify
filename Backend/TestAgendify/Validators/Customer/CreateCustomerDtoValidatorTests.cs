using Agendify.DTOs.Customer;
using Agendify.Validators.Customer;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace TestAgendify.Validators.Customer;

public class CreateCustomerDtoValidatorTests
{
    private readonly CreateCustomerDtoValidator _validator;

    public CreateCustomerDtoValidatorTests()
    {
        _validator = new CreateCustomerDtoValidator();
    }

    #region Name Tests

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        // Arrange
        var dto = new CreateCustomerDto { Name = string.Empty };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("El nombre es requerido");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Null()
    {
        // Arrange
        var dto = new CreateCustomerDto { Name = null! };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        // Arrange
        var dto = new CreateCustomerDto { Name = new string('A', 201) };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("El nombre no puede exceder 200 caracteres");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        // Arrange
        var dto = new CreateCustomerDto { Name = "John Doe" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Has_Exactly_200_Characters()
    {
        // Arrange
        var dto = new CreateCustomerDto { Name = new string('A', 200) };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region Phone Tests

    [Fact]
    public void Should_Not_Have_Error_When_Phone_Is_Null()
    {
        // Arrange
        var dto = new CreateCustomerDto 
        { 
            Name = "John Doe",
            Phone = null 
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Should_Have_Error_When_Phone_Has_Invalid_Format()
    {
        // Arrange
        var dto = new CreateCustomerDto 
        { 
            Name = "John Doe",
            Phone = "abc123" // Contiene letras, debería fallar
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Theory]
    [InlineData("+5491112345678")]
    [InlineData("+1234567890")]
    [InlineData("1234567890")]
    public void Should_Not_Have_Error_When_Phone_Has_Valid_Format(string phone)
    {
        // Arrange
        var dto = new CreateCustomerDto 
        { 
            Name = "John Doe",
            Phone = phone 
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Should_Have_Error_When_Phone_Exceeds_MaxLength()
    {
        // Arrange
        var dto = new CreateCustomerDto 
        { 
            Name = "John Doe",
            Phone = new string('1', 21) 
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone)
            .WithErrorMessage("El teléfono no puede exceder 20 caracteres");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12-34-56")]
    [InlineData("+0123456789")]
    [InlineData("123 456 7890")]
    public void Should_Have_Error_When_Phone_Has_Invalid_Characters(string phone)
    {
        // Arrange
        var dto = new CreateCustomerDto 
        { 
            Name = "John Doe",
            Phone = phone 
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    #endregion

    #region Email Tests

    [Fact]
    public void Should_Not_Have_Error_When_Email_Is_Null()
    {
        // Arrange
        var dto = new CreateCustomerDto 
        { 
            Name = "John Doe",
            Email = null 
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@example.co.uk")]
    [InlineData("test+tag@example.com")]
    public void Should_Not_Have_Error_When_Email_Is_Valid(string email)
    {
        // Arrange
        var dto = new CreateCustomerDto 
        { 
            Name = "John Doe",
            Email = email 
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Should_Have_Error_When_Email_Has_Invalid_Format(string email)
    {
        // Arrange
        var dto = new CreateCustomerDto 
        { 
            Name = "John Doe",
            Email = email 
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Exceeds_MaxLength()
    {
        // Arrange
        var longEmail = new string('a', 190) + "@example.com"; // Total > 200 chars
        var dto = new CreateCustomerDto 
        { 
            Name = "John Doe",
            Email = longEmail 
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("El email no puede exceder 200 caracteres");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        // Arrange
        var dto = new CreateCustomerDto
        {
            Name = "John Doe",
            Phone = "+5491112345678",
            Email = "john.doe@example.com"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Only_Required_Fields_Are_Provided()
    {
        // Arrange
        var dto = new CreateCustomerDto
        {
            Name = "John Doe"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_Multiple_Fields_Are_Invalid()
    {
        // Arrange
        var dto = new CreateCustomerDto
        {
            Name = string.Empty,
            Phone = "invalid",
            Email = "invalid-email"
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    #endregion
}

