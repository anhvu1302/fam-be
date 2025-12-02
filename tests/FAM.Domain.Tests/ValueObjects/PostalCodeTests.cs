using FAM.Domain.Common;
using FAM.Domain.ValueObjects;
using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class PostalCodeTests
{
    [Fact]
    public void Create_WithValidUSPostalCode_ShouldCreatePostalCode()
    {
        // Arrange
        var value = "12345";

        // Act
        var postalCode = PostalCode.Create(value);

        // Assert
        postalCode.Should().NotBeNull();
        postalCode.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithValidVietnamesePostalCode_ShouldCreatePostalCode()
    {
        // Arrange
        var value = "1234";

        // Act
        var postalCode = PostalCode.Create(value);

        // Assert
        postalCode.Should().NotBeNull();
        postalCode.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithValidCanadianPostalCode_ShouldCreatePostalCode()
    {
        // Arrange
        var value = "K1A 1A1";

        // Act
        var postalCode = PostalCode.Create(value);

        // Assert
        postalCode.Should().NotBeNull();
        postalCode.Value.Should().Be("K1A 1A1");
    }

    [Fact]
    public void Create_WithValidUKPostalCode_ShouldCreatePostalCode()
    {
        // Arrange
        var value = "SW1A 1AA";

        // Act
        var postalCode = PostalCode.Create(value);

        // Assert
        postalCode.Should().NotBeNull();
        postalCode.Value.Should().Be("SW1A 1AA");
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        var value = "";

        // Act
        Action act = () => PostalCode.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Postal code cannot be empty");
    }

    [Fact]
    public void Create_WithWhitespace_ShouldThrowDomainException()
    {
        // Arrange
        var value = "   ";

        // Act
        Action act = () => PostalCode.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Postal code cannot be empty");
    }

    [Fact]
    public void Create_WithInvalidFormat_ShouldThrowDomainException()
    {
        // Arrange
        var value = "INVALID";

        // Act
        Action act = () => PostalCode.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid postal code format");
    }

    [Fact]
    public void Create_WithLowercase_ShouldConvertToUppercase()
    {
        // Arrange
        var value = "k1a 1a1";

        // Act
        var postalCode = PostalCode.Create(value);

        // Assert
        postalCode.Value.Should().Be("K1A 1A1");
    }

    [Fact]
    public void IsVietnamesePostalCode_WithVietnameseCode_ShouldReturnTrue()
    {
        // Arrange
        var postalCode = PostalCode.Create("1234");

        // Act
        var result = postalCode.IsVietnamesePostalCode();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsVietnamesePostalCode_WithNonVietnameseCode_ShouldReturnFalse()
    {
        // Arrange
        var postalCode = PostalCode.Create("SW1A 1AA");

        // Act
        var result = postalCode.IsVietnamesePostalCode();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsUSPostalCode_WithUSCode_ShouldReturnTrue()
    {
        // Arrange
        var postalCode = PostalCode.Create("12345");

        // Act
        var result = postalCode.IsUSPostalCode();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsUSPostalCode_WithNonUSCode_ShouldReturnFalse()
    {
        // Arrange
        var postalCode = PostalCode.Create("1234");

        // Act
        var result = postalCode.IsUSPostalCode();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var postalCode = PostalCode.Create("12345");

        // Act
        string value = postalCode;

        // Assert
        value.Should().Be("12345");
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromString()
    {
        // Arrange
        var value = "12345";

        // Act
        var postalCode = (PostalCode)value;

        // Assert
        postalCode.Value.Should().Be("12345");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var postalCode = PostalCode.Create("12345");

        // Act
        var result = postalCode.ToString();

        // Assert
        result.Should().Be("12345");
    }
}