using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class CostCenterTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCostCenter()
    {
        // Arrange
        string value = "CC-001";

        // Act
        CostCenter costCenter = CostCenter.Create(value);

        // Assert
        costCenter.Should().NotBeNull();
        costCenter.Value.Should().Be(value.ToUpperInvariant());
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        string value = "";

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
        string value = new('A', 31);

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
        string value = "cc-001";

        // Act
        CostCenter costCenter = CostCenter.Create(value);

        // Assert
        costCenter.Value.Should().Be("cc-001");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        CostCenter costCenter = CostCenter.Create("CC-001");

        // Act
        string value = costCenter!;

        // Assert
        value.Should().Be("CC-001");
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromString()
    {
        // Arrange
        string value = "CC-001";

        // Act
        CostCenter costCenter = (CostCenter)value;

        // Assert
        costCenter.Value.Should().Be("CC-001");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        CostCenter costCenter = CostCenter.Create("CC-001");

        // Act
        string result = costCenter.ToString();

        // Assert
        result.Should().Be("CC-001");
    }
}
