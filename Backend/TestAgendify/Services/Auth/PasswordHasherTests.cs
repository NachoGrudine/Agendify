using Agendify.Services.Auth.Password;
using FluentAssertions;

namespace TestAgendify.Services.Auth;

public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    [Fact]
    public void HashPassword_Should_Return_Non_Empty_Hash()
    {
        // Arrange
        var password = "MySecurePassword123";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HashPassword_Should_Return_Different_Hash_For_Same_Password()
    {
        // Arrange
        var password = "MySecurePassword123";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2, "because each hash should use a unique salt");
    }

    [Fact]
    public void HashPassword_Should_Return_Base64_String()
    {
        // Arrange
        var password = "TestPassword";

        // Act
        var hash = _passwordHasher.HashPassword(password);

        // Assert
        // Try to decode from Base64 - if it fails, it's not valid Base64
        var action = () => Convert.FromBase64String(hash);
        action.Should().NotThrow();
    }

    [Theory]
    [InlineData("password")]
    [InlineData("12345678")]
    [InlineData("MyP@ssw0rd!")]
    [InlineData("")]
    [InlineData("a")]
    public void VerifyPassword_Should_Return_True_For_Correct_Password(string password)
    {
        // Arrange
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_For_Incorrect_Password()
    {
        // Arrange
        var correctPassword = "CorrectPassword123";
        var incorrectPassword = "WrongPassword456";
        var hash = _passwordHasher.HashPassword(correctPassword);

        // Act
        var result = _passwordHasher.VerifyPassword(incorrectPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_For_Similar_But_Different_Password()
    {
        // Arrange
        var password1 = "Password123";
        var password2 = "Password124"; // Very similar but different
        var hash = _passwordHasher.HashPassword(password1);

        // Act
        var result = _passwordHasher.VerifyPassword(password2, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_For_Case_Sensitive_Difference()
    {
        // Arrange
        var password = "Password";
        var wrongCasePassword = "password";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(wrongCasePassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_Should_Work_With_Special_Characters()
    {
        // Arrange
        var password = "P@ssw0rd!#$%^&*()";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_Work_With_Unicode_Characters()
    {
        // Arrange
        var password = "Contraseña123áéíóú";
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_Work_With_Very_Long_Password()
    {
        // Arrange
        var password = new string('a', 1000);
        var hash = _passwordHasher.HashPassword(password);

        // Act
        var result = _passwordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HashPassword_Should_Produce_Consistent_Length_Hashes()
    {
        // Arrange
        var password1 = "short";
        var password2 = "this is a much longer password with many characters";

        // Act
        var hash1 = _passwordHasher.HashPassword(password1);
        var hash2 = _passwordHasher.HashPassword(password2);

        // Assert
        hash1.Length.Should().Be(hash2.Length, "because hash length should be constant regardless of input");
    }
    [Fact]
    public void VerifyPassword_Should_Return_False_For_Invalid_Hash_Format()
    {
        // Arrange
        var password = "password123";
        var invalidHash = "not-a-valid-hash";

        // Act & Assert
        var action = () => _passwordHasher.VerifyPassword(password, invalidHash);
        action.Should().Throw<FormatException>();
    }

    [Fact]
    public void Multiple_Hash_And_Verify_Cycles_Should_Work()
    {
        // Arrange
        var password = "MyPassword123";

        // Act & Assert - Multiple cycles
        for (int i = 0; i < 5; i++)
        {
            var hash = _passwordHasher.HashPassword(password);
            var result = _passwordHasher.VerifyPassword(password, hash);
            result.Should().BeTrue($"verification should succeed on iteration {i}");
        }
    }
}

