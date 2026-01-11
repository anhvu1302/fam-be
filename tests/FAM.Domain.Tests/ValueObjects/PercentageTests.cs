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
        decimal value = 25.5m;

        // Act
        Percentage percentage = Percentage.Create(value);

        // Assert
        percentage.Value.Should().Be(25.5m);
    }

    [Fact]
    public void Create_WithZeroPercentage_ShouldCreatePercentage()
    {
        // Arrange
        decimal value = 0m;

        // Act
        Percentage percentage = Percentage.Create(value);

        // Assert
        percentage.Value.Should().Be(0m);
        percentage.IsZero().Should().BeTrue();
    }

    [Fact]
    public void Create_WithFullPercentage_ShouldCreatePercentage()
    {
        // Arrange
        decimal value = 100m;

        // Act
        Percentage percentage = Percentage.Create(value);

        // Assert
        percentage.Value.Should().Be(100m);
        percentage.IsFull().Should().BeTrue();
    }

    [Fact]
    public void Create_WithDecimalPlaces_ShouldRoundToTwoDecimalPlaces()
    {
        // Arrange
        decimal value = 25.123m;

        // Act
        Percentage percentage = Percentage.Create(value);

        // Assert
        percentage.Value.Should().Be(25.12m);
    }

    [Fact]
    public void Create_WithNegativeValue_ShouldThrowDomainException()
    {
        // Arrange
        decimal negativeValue = -5m;

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
        decimal over100Value = 150m;

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
        decimal decimalValue = 0.25m; // 25%

        // Act
        Percentage percentage = Percentage.FromDecimal(decimalValue);

        // Assert
        percentage.Value.Should().Be(25m);
    }

    [Fact]
    public void FromDecimal_WithZero_ShouldCreateZeroPercentage()
    {
        // Arrange
        decimal decimalValue = 0m;

        // Act
        Percentage percentage = Percentage.FromDecimal(decimalValue);

        // Assert
        percentage.Value.Should().Be(0m);
    }

    [Fact]
    public void FromDecimal_WithOne_ShouldCreateFullPercentage()
    {
        // Arrange
        decimal decimalValue = 1m;

        // Act
        Percentage percentage = Percentage.FromDecimal(decimalValue);

        // Assert
        percentage.Value.Should().Be(100m);
    }

    [Fact]
    public void ToDecimal_ShouldReturnCorrectDecimalValue()
    {
        // Arrange
        Percentage percentage = Percentage.Create(25m);

        // Act
        decimal decimalValue = percentage.ToDecimal();

        // Assert
        decimalValue.Should().Be(0.25m);
    }

    [Fact]
    public void ToFraction_ShouldReturnSameAsToDecimal()
    {
        // Arrange
        Percentage percentage = Percentage.Create(75m);

        // Act
        decimal fraction = percentage.ToFraction();
        decimal decimalValue = percentage.ToDecimal();

        // Assert
        fraction.Should().Be(decimalValue);
        fraction.Should().Be(0.75m);
    }

    [Fact]
    public void Add_WithValidPercentages_ShouldReturnCorrectSum()
    {
        // Arrange
        Percentage percentage1 = Percentage.Create(25m);
        Percentage percentage2 = Percentage.Create(30m);

        // Act
        Percentage result = percentage1.Add(percentage2);

        // Assert
        result.Value.Should().Be(55m);
    }

    [Fact]
    public void Add_WithResultOver100_ShouldThrowDomainException()
    {
        // Arrange
        Percentage percentage1 = Percentage.Create(75m);
        Percentage percentage2 = Percentage.Create(30m);

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
        Percentage percentage1 = Percentage.Create(75m);
        Percentage percentage2 = Percentage.Create(25m);

        // Act
        Percentage result = percentage1.Subtract(percentage2);

        // Assert
        result.Value.Should().Be(50m);
    }

    [Fact]
    public void Subtract_WithNegativeResult_ShouldThrowDomainException()
    {
        // Arrange
        Percentage percentage1 = Percentage.Create(25m);
        Percentage percentage2 = Percentage.Create(75m);

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
        Percentage percentage = Percentage.Create(50m);
        decimal factor = 0.5m;

        // Act
        Percentage result = percentage.Multiply(factor);

        // Assert
        result.Value.Should().Be(25m);
    }

    [Fact]
    public void Multiply_WithFactorMakingResultOver100_ShouldThrowDomainException()
    {
        // Arrange
        Percentage percentage = Percentage.Create(50m);
        decimal factor = 3m;

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
        Percentage percentage = Percentage.Create(0m);

        // Act
        bool isZero = percentage.IsZero();

        // Assert
        isZero.Should().BeTrue();
    }

    [Fact]
    public void IsZero_WithNonZeroPercentage_ShouldReturnFalse()
    {
        // Arrange
        Percentage percentage = Percentage.Create(10m);

        // Act
        bool isZero = percentage.IsZero();

        // Assert
        isZero.Should().BeFalse();
    }

    [Fact]
    public void IsFull_With100Percentage_ShouldReturnTrue()
    {
        // Arrange
        Percentage percentage = Percentage.Create(100m);

        // Act
        bool isFull = percentage.IsFull();

        // Assert
        isFull.Should().BeTrue();
    }

    [Fact]
    public void IsFull_WithNon100Percentage_ShouldReturnFalse()
    {
        // Arrange
        Percentage percentage = Percentage.Create(90m);

        // Act
        bool isFull = percentage.IsFull();

        // Assert
        isFull.Should().BeFalse();
    }

    [Fact]
    public void ImplicitOperator_ToDecimal_ShouldReturnValue()
    {
        // Arrange
        Percentage percentage = Percentage.Create(25m);

        // Act
        decimal decimalValue = percentage;

        // Assert
        decimalValue.Should().Be(25m);
    }

    [Fact]
    public void ExplicitOperator_FromDecimal_ShouldCreatePercentage()
    {
        // Arrange
        decimal decimalValue = 75m;

        // Act
        Percentage percentage = (Percentage)decimalValue;

        // Assert
        percentage.Value.Should().Be(75m);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        Percentage percentage = Percentage.Create(25.5m);

        // Act
        string result = percentage.ToString();

        // Assert
        result.Should().Be("25.5%");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        Percentage percentage1 = Percentage.Create(25m);
        Percentage percentage2 = Percentage.Create(25m);

        // Act & Assert
        percentage1.Should().Be(percentage2);
        percentage1.GetHashCode().Should().Be(percentage2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        Percentage percentage1 = Percentage.Create(25m);
        Percentage percentage2 = Percentage.Create(30m);

        // Act & Assert
        percentage1.Should().NotBe(percentage2);
    }

    [Fact]
    public void Equality_WithRoundedValues_ShouldBeEqual()
    {
        // Arrange
        Percentage percentage1 = Percentage.Create(25.123m); // rounds to 25.12
        Percentage percentage2 = Percentage.Create(25.12m);

        // Act & Assert
        percentage1.Should().Be(percentage2);
    }
}
