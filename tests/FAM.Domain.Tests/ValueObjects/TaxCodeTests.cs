using FAM.Domain.Common;
using FAM.Domain.ValueObjects;
using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class TaxCodeTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateTaxCode()
    {
        // Arrange
        var value = "1234567890";

        // Act
        var taxCode = TaxCode.Create(value);

        // Assert
        taxCode.Should().NotBeNull();
        taxCode.Value.Should().Be(value.ToUpperInvariant());
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        var value = "";

        // Act
        Action act = () => TaxCode.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Tax code cannot be empty");
    }

    [Fact]
    public void Create_WithTooShortValue_ShouldThrowDomainException()
    {
        // Arrange
        var value = "1234567";

        // Act
        Action act = () => TaxCode.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Tax code must be between 8 and 15 characters");
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowDomainException()
    {
        // Arrange
        var value = new string('1', 16);

        // Act
        Action act = () => TaxCode.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Tax code must be between 8 and 15 characters");
    }

    [Fact]
    public void Create_WithInvalidCharacters_ShouldThrowDomainException()
    {
        // Arrange
        var value = "123456789@";

        // Act
        Action act = () => TaxCode.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Tax code can only contain numbers and hyphens");
    }

    [Fact]
    public void Create_WithLowercase_ShouldConvertToUppercase()
    {
        // Arrange
        var value = "1234567890";

        // Act
        var taxCode = TaxCode.Create(value);

        // Assert
        taxCode.Value.Should().Be("1234567890");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var taxCode = TaxCode.Create("1234567890");

        // Act
        string value = taxCode;

        // Assert
        value.Should().Be("1234567890");
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromString()
    {
        // Arrange
        var value = "1234567890";

        // Act
        var taxCode = (TaxCode)value;

        // Assert
        taxCode.Value.Should().Be("1234567890");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var taxCode = TaxCode.Create("1234567890");

        // Act
        var result = taxCode.ToString();

        // Assert
        result.Should().Be("1234567890");
    }
}