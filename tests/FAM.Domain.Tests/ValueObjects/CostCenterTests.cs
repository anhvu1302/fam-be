using FluentAssertions;
using FAM.Domain.ValueObjects;
using FAM.Domain.Common;

namespace FAM.Domain.Tests.ValueObjects;

public class CostCenterTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCostCenter()
    {
        // Arrange
        var value = "CC-001";

        // Act
        var costCenter = CostCenter.Create(value);

        // Assert
        costCenter.Should().NotBeNull();
        costCenter.Value.Should().Be(value.ToUpperInvariant());
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        var value = "";

        // Act
        Action act = () => CostCenter.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Cost center cannot be empty");
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowDomainException()
    {
        // Arrange
        var value = new string('A', 31);

        // Act
        Action act = () => CostCenter.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Cost center cannot exceed 30 characters");
    }

    [Fact]
    public void Create_WithLowercase_ShouldNotConvertToUppercase()
    {
        // Arrange
        var value = "cc-001";

        // Act
        var costCenter = CostCenter.Create(value);

        // Assert
        costCenter.Value.Should().Be("cc-001");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var costCenter = CostCenter.Create("CC-001");

        // Act
        string value = costCenter;

        // Assert
        value.Should().Be("CC-001");
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromString()
    {
        // Arrange
        var value = "CC-001";

        // Act
        CostCenter costCenter = (CostCenter)value;

        // Assert
        costCenter.Value.Should().Be("CC-001");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var costCenter = CostCenter.Create("CC-001");

        // Act
        var result = costCenter.ToString();

        // Assert
        result.Should().Be("CC-001");
    }
}