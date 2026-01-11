using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class PostalCodeTests
{
    [Fact]
    public void Create_WithValidUSPostalCode_ShouldCreatePostalCode()
    {
        // Arrange
        string value = "12345";

        // Act
        PostalCode postalCode = PostalCode.Create(value);

        // Assert
        postalCode.Should().NotBeNull();
        postalCode.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithValidVietnamesePostalCode_ShouldCreatePostalCode()
    {
        // Arrange
        string value = "1234";

        // Act
        PostalCode postalCode = PostalCode.Create(value);

        // Assert
        postalCode.Should().NotBeNull();
        postalCode.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithValidCanadianPostalCode_ShouldCreatePostalCode()
    {
        // Arrange
        string value = "K1A 1A1";

        // Act
        PostalCode postalCode = PostalCode.Create(value);

        // Assert
        postalCode.Should().NotBeNull();
        postalCode.Value.Should().Be("K1A 1A1");
    }

    [Fact]
    public void Create_WithValidUKPostalCode_ShouldCreatePostalCode()
    {
        // Arrange
        string value = "SW1A 1AA";

        // Act
        PostalCode postalCode = PostalCode.Create(value);

        // Assert
        postalCode.Should().NotBeNull();
        postalCode.Value.Should().Be("SW1A 1AA");
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        string value = "";

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
        string value = "   ";

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
        string value = "INVALID";

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
        string value = "k1a 1a1";

        // Act
        PostalCode postalCode = PostalCode.Create(value);

        // Assert
        postalCode.Value.Should().Be("K1A 1A1");
    }

    [Fact]
    public void IsVietnamesePostalCode_WithVietnameseCode_ShouldReturnTrue()
    {
        // Arrange
        PostalCode postalCode = PostalCode.Create("1234");

        // Act
        bool result = postalCode.IsVietnamesePostalCode();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsVietnamesePostalCode_WithNonVietnameseCode_ShouldReturnFalse()
    {
        // Arrange
        PostalCode postalCode = PostalCode.Create("SW1A 1AA");

        // Act
        bool result = postalCode.IsVietnamesePostalCode();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsUSPostalCode_WithUSCode_ShouldReturnTrue()
    {
        // Arrange
        PostalCode postalCode = PostalCode.Create("12345");

        // Act
        bool result = postalCode.IsUSPostalCode();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsUSPostalCode_WithNonUSCode_ShouldReturnFalse()
    {
        // Arrange
        PostalCode postalCode = PostalCode.Create("1234");

        // Act
        bool result = postalCode.IsUSPostalCode();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        PostalCode postalCode = PostalCode.Create("12345");

        // Act
        string value = postalCode;

        // Assert
        value.Should().Be("12345");
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromString()
    {
        // Arrange
        string value = "12345";

        // Act
        PostalCode postalCode = (PostalCode)value;

        // Assert
        postalCode.Value.Should().Be("12345");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        PostalCode postalCode = PostalCode.Create("12345");

        // Act
        string result = postalCode.ToString();

        // Assert
        result.Should().Be("12345");
    }
}
