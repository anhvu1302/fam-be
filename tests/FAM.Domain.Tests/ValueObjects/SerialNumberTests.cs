using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class SerialNumberTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateSerialNumber()
    {
        // Arrange
        string value = "SN123456789";

        // Act
        SerialNumber serialNumber = SerialNumber.Create(value);

        // Assert
        serialNumber.Should().NotBeNull();
        serialNumber.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        string value = "";

        // Act
        Action act = () => SerialNumber.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Serial number cannot be empty");
    }

    [Fact]
    public void Create_WithWhitespace_ShouldThrowDomainException()
    {
        // Arrange
        string value = "   ";

        // Act
        Action act = () => SerialNumber.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Serial number cannot be empty");
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowDomainException()
    {
        // Arrange
        string value = new('A', 101);

        // Act
        Action act = () => SerialNumber.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Serial number cannot exceed 100 characters");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        SerialNumber serialNumber = SerialNumber.Create("SN123");

        // Act
        string value = serialNumber;

        // Assert
        value.Should().Be("SN123");
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromString()
    {
        // Arrange
        string value = "SN123";

        // Act
        SerialNumber serialNumber = (SerialNumber)value;

        // Assert
        serialNumber.Value.Should().Be(value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        SerialNumber serialNumber = SerialNumber.Create("SN123");

        // Act
        string result = serialNumber.ToString();

        // Assert
        result.Should().Be("SN123");
    }
}
