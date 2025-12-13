using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class PercentageTests
{
    [Fact]
    public void Create_WithValidPercentage_ShouldCreatePercentage()
    {
        // Arrange
        var value = 25.5m;

        // Act
        var percentage = Percentage.Create(value);

        // Assert
        percentage.Value.Should().Be(25.5m);
    }

    [Fact]
    public void Create_WithZeroPercentage_ShouldCreatePercentage()
    {
        // Arrange
        var value = 0m;

        // Act
        var percentage = Percentage.Create(value);

        // Assert
        percentage.Value.Should().Be(0m);
        percentage.IsZero().Should().BeTrue();
    }

    [Fact]
    public void Create_WithFullPercentage_ShouldCreatePercentage()
    {
        // Arrange
        var value = 100m;

        // Act
        var percentage = Percentage.Create(value);

        // Assert
        percentage.Value.Should().Be(100m);
        percentage.IsFull().Should().BeTrue();
    }

    [Fact]
    public void Create_WithDecimalPlaces_ShouldRoundToTwoDecimalPlaces()
    {
        // Arrange
        var value = 25.123m;

        // Act
        var percentage = Percentage.Create(value);

        // Assert
        percentage.Value.Should().Be(25.12m);
    }

    [Fact]
    public void Create_WithNegativeValue_ShouldThrowDomainException()
    {
        // Arrange
        var negativeValue = -5m;

        // Act
        Action act = () => Percentage.Create(negativeValue);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Percentage cannot be negative");
    }

    [Fact]
    public void Create_WithValueOver100_ShouldThrowDomainException()
    {
        // Arrange
        var over100Value = 150m;

        // Act
        Action act = () => Percentage.Create(over100Value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Percentage cannot exceed 100%");
    }

    [Fact]
    public void FromDecimal_WithValidDecimal_ShouldCreatePercentage()
    {
        // Arrange
        var decimalValue = 0.25m; // 25%

        // Act
        var percentage = Percentage.FromDecimal(decimalValue);

        // Assert
        percentage.Value.Should().Be(25m);
    }

    [Fact]
    public void FromDecimal_WithZero_ShouldCreateZeroPercentage()
    {
        // Arrange
        var decimalValue = 0m;

        // Act
        var percentage = Percentage.FromDecimal(decimalValue);

        // Assert
        percentage.Value.Should().Be(0m);
    }

    [Fact]
    public void FromDecimal_WithOne_ShouldCreateFullPercentage()
    {
        // Arrange
        var decimalValue = 1m;

        // Act
        var percentage = Percentage.FromDecimal(decimalValue);

        // Assert
        percentage.Value.Should().Be(100m);
    }

    [Fact]
    public void ToDecimal_ShouldReturnCorrectDecimalValue()
    {
        // Arrange
        var percentage = Percentage.Create(25m);

        // Act
        var decimalValue = percentage.ToDecimal();

        // Assert
        decimalValue.Should().Be(0.25m);
    }

    [Fact]
    public void ToFraction_ShouldReturnSameAsToDecimal()
    {
        // Arrange
        var percentage = Percentage.Create(75m);

        // Act
        var fraction = percentage.ToFraction();
        var decimalValue = percentage.ToDecimal();

        // Assert
        fraction.Should().Be(decimalValue);
        fraction.Should().Be(0.75m);
    }

    [Fact]
    public void Add_WithValidPercentages_ShouldReturnCorrectSum()
    {
        // Arrange
        var percentage1 = Percentage.Create(25m);
        var percentage2 = Percentage.Create(30m);

        // Act
        Percentage result = percentage1.Add(percentage2);

        // Assert
        result.Value.Should().Be(55m);
    }

    [Fact]
    public void Add_WithResultOver100_ShouldThrowDomainException()
    {
        // Arrange
        var percentage1 = Percentage.Create(75m);
        var percentage2 = Percentage.Create(30m);

        // Act
        Action act = () => percentage1.Add(percentage2);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Percentage cannot exceed 100%");
    }

    [Fact]
    public void Subtract_WithValidPercentages_ShouldReturnCorrectDifference()
    {
        // Arrange
        var percentage1 = Percentage.Create(75m);
        var percentage2 = Percentage.Create(25m);

        // Act
        Percentage result = percentage1.Subtract(percentage2);

        // Assert
        result.Value.Should().Be(50m);
    }

    [Fact]
    public void Subtract_WithNegativeResult_ShouldThrowDomainException()
    {
        // Arrange
        var percentage1 = Percentage.Create(25m);
        var percentage2 = Percentage.Create(75m);

        // Act
        Action act = () => percentage1.Subtract(percentage2);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Percentage cannot be negative");
    }

    [Fact]
    public void Multiply_WithPositiveFactor_ShouldReturnCorrectProduct()
    {
        // Arrange
        var percentage = Percentage.Create(50m);
        var factor = 0.5m;

        // Act
        Percentage result = percentage.Multiply(factor);

        // Assert
        result.Value.Should().Be(25m);
    }

    [Fact]
    public void Multiply_WithFactorMakingResultOver100_ShouldThrowDomainException()
    {
        // Arrange
        var percentage = Percentage.Create(50m);
        var factor = 3m;

        // Act
        Action act = () => percentage.Multiply(factor);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Percentage cannot exceed 100%");
    }

    [Fact]
    public void IsZero_WithZeroPercentage_ShouldReturnTrue()
    {
        // Arrange
        var percentage = Percentage.Create(0m);

        // Act
        var isZero = percentage.IsZero();

        // Assert
        isZero.Should().BeTrue();
    }

    [Fact]
    public void IsZero_WithNonZeroPercentage_ShouldReturnFalse()
    {
        // Arrange
        var percentage = Percentage.Create(10m);

        // Act
        var isZero = percentage.IsZero();

        // Assert
        isZero.Should().BeFalse();
    }

    [Fact]
    public void IsFull_With100Percentage_ShouldReturnTrue()
    {
        // Arrange
        var percentage = Percentage.Create(100m);

        // Act
        var isFull = percentage.IsFull();

        // Assert
        isFull.Should().BeTrue();
    }

    [Fact]
    public void IsFull_WithNon100Percentage_ShouldReturnFalse()
    {
        // Arrange
        var percentage = Percentage.Create(90m);

        // Act
        var isFull = percentage.IsFull();

        // Assert
        isFull.Should().BeFalse();
    }

    [Fact]
    public void ImplicitOperator_ToDecimal_ShouldReturnValue()
    {
        // Arrange
        var percentage = Percentage.Create(25m);

        // Act
        decimal decimalValue = percentage;

        // Assert
        decimalValue.Should().Be(25m);
    }

    [Fact]
    public void ExplicitOperator_FromDecimal_ShouldCreatePercentage()
    {
        // Arrange
        var decimalValue = 75m;

        // Act
        var percentage = (Percentage)decimalValue;

        // Assert
        percentage.Value.Should().Be(75m);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var percentage = Percentage.Create(25.5m);

        // Act
        var result = percentage.ToString();

        // Assert
        result.Should().Be("25.5%");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var percentage1 = Percentage.Create(25m);
        var percentage2 = Percentage.Create(25m);

        // Act & Assert
        percentage1.Should().Be(percentage2);
        percentage1.GetHashCode().Should().Be(percentage2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var percentage1 = Percentage.Create(25m);
        var percentage2 = Percentage.Create(30m);

        // Act & Assert
        percentage1.Should().NotBe(percentage2);
    }

    [Fact]
    public void Equality_WithRoundedValues_ShouldBeEqual()
    {
        // Arrange
        var percentage1 = Percentage.Create(25.123m); // rounds to 25.12
        var percentage2 = Percentage.Create(25.12m);

        // Act & Assert
        percentage1.Should().Be(percentage2);
    }
}
