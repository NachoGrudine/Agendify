using Agendify.DTOs.Customer;
using Agendify.Validators.Customer;
using FluentValidation.TestHelper;

namespace TestAgendify.Validators.Customer;

public class UpdateCustomerDtoValidatorTests
{
    private readonly UpdateCustomerDtoValidator _validator;

    public UpdateCustomerDtoValidatorTests()
    {
        _validator = new UpdateCustomerDtoValidator();
    }

    #region Name Tests

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var dto = new UpdateCustomerDto { Name = string.Empty };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("El nombre es requerido");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_MaxLength()
    {
        var dto = new UpdateCustomerDto { Name = new string('A', 201) };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("El nombre no puede exceder 200 caracteres");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Name_Is_Valid()
    {
        var dto = new UpdateCustomerDto { Name = "John Doe" };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    #endregion

    #region Phone Tests

    [Fact]
    public void Should_Not_Have_Error_When_Phone_Is_Null()
    {
        var dto = new UpdateCustomerDto 
        { 
            Name = "John Doe",
            Phone = null 
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    [Fact]
    public void Should_Have_Error_When_Phone_Has_Invalid_Format()
    {
        var dto = new UpdateCustomerDto 
        { 
            Name = "John Doe",
            Phone = "abc123"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Theory]
    [InlineData("+5491112345678")]
    [InlineData("+1234567890")]
    [InlineData("1234567890")]
    public void Should_Not_Have_Error_When_Phone_Has_Valid_Format(string phone)
    {
        var dto = new UpdateCustomerDto 
        { 
            Name = "John Doe",
            Phone = phone 
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Phone);
    }

    #endregion

    #region Email Tests

    [Fact]
    public void Should_Not_Have_Error_When_Email_Is_Null()
    {
        var dto = new UpdateCustomerDto 
        { 
            Name = "John Doe",
            Email = null 
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@example.co.uk")]
    public void Should_Not_Have_Error_When_Email_Is_Valid(string email)
    {
        var dto = new UpdateCustomerDto 
        { 
            Name = "John Doe",
            Email = email 
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    public void Should_Have_Error_When_Email_Has_Invalid_Format(string email)
    {
        var dto = new UpdateCustomerDto 
        { 
            Name = "John Doe",
            Email = email 
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Should_Not_Have_Error_When_All_Fields_Are_Valid()
    {
        var dto = new UpdateCustomerDto
        {
            Name = "John Doe",
            Phone = "+5491112345678",
            Email = "john.doe@example.com"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion
}

