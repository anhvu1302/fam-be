using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmountAndCurrency_ShouldCreateMoney()
    {
        // Arrange
        decimal amount = 100.50m;
        string currency = "USD";

        // Act
        Money money = Money.Create(amount, currency);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldCreateMoney()
    {
        // Arrange
        decimal amount = 0m;
        string currency = "EUR";

        // Act
        Money money = Money.Create(amount, currency);

        // Assert
        money.Amount.Should().Be(amount);
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Create_WithCurrencyInLowerCase_ShouldConvertToUpperCase()
    {
        // Arrange
        decimal amount = 50.25m;
        string currency = "usd";

        // Act
        Money money = Money.Create(amount, currency);

        // Assert
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowDomainException()
    {
        // Arrange
        decimal negativeAmount = -10.00m;
        string currency = "USD";

        // Act
        Action act = () => Money.Create(negativeAmount, currency);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Amount cannot be negative");
    }

    [Fact]
    public void Create_WithNullCurrency_ShouldThrowDomainException()
    {
        // Arrange
        decimal amount = 100.00m;
        string? nullCurrency = null;

        // Act
        Action act = () => Money.Create(amount, nullCurrency!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Currency is required");
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldThrowDomainException()
    {
        // Arrange
        decimal amount = 100.00m;
        string emptyCurrency = string.Empty;

        // Act
        Action act = () => Money.Create(amount, emptyCurrency);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Currency is required");
    }

    [Fact]
    public void Create_WithWhitespaceCurrency_ShouldThrowDomainException()
    {
        // Arrange
        decimal amount = 100.00m;
        string whitespaceCurrency = "   ";

        // Act
        Action act = () => Money.Create(amount, whitespaceCurrency);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Currency is required");
    }

    [Fact]
    public void Zero_WithDefaultCurrency_ShouldCreateZeroMoney()
    {
        // Act
        Money money = Money.Zero();

        // Assert
        money.Amount.Should().Be(0);
        money.Currency.Should().Be("VND");
    }

    [Fact]
    public void Zero_WithSpecifiedCurrency_ShouldCreateZeroMoney()
    {
        // Arrange
        string currency = "EUR";

        // Act
        Money money = Money.Zero(currency);

        // Assert
        money.Amount.Should().Be(0);
        money.Currency.Should().Be("EUR");
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnCorrectSum()
    {
        // Arrange
        Money money1 = Money.Create(100.00m, "USD");
        Money money2 = Money.Create(50.50m, "USD");

        // Act
        Money result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150.50m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Money money1 = Money.Create(100.00m, "USD");
        Money money2 = Money.Create(50.50m, "EUR");

        // Act
        Action act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add EUR to USD");
    }

    [Fact]
    public void Subtract_WithSameCurrency_ShouldReturnCorrectDifference()
    {
        // Arrange
        Money money1 = Money.Create(100.00m, "USD");
        Money money2 = Money.Create(30.50m, "USD");

        // Act
        Money result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(69.50m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Subtract_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Money money1 = Money.Create(100.00m, "USD");
        Money money2 = Money.Create(30.50m, "EUR");

        // Act
        Action act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot subtract EUR from USD");
    }

    [Fact]
    public void Multiply_WithPositiveMultiplier_ShouldReturnCorrectProduct()
    {
        // Arrange
        Money money = Money.Create(100.00m, "USD");
        decimal multiplier = 1.5m;

        // Act
        Money result = money.Multiply(multiplier);

        // Assert
        result.Amount.Should().Be(150.00m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Multiply_WithZeroMultiplier_ShouldReturnZero()
    {
        // Arrange
        Money money = Money.Create(100.00m, "USD");
        decimal multiplier = 0m;

        // Act
        Money result = money.Multiply(multiplier);

        // Assert
        result.Amount.Should().Be(0);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Divide_WithValidDivisor_ShouldReturnCorrectQuotient()
    {
        // Arrange
        Money money = Money.Create(100.00m, "USD");
        decimal divisor = 4m;

        // Act
        Money result = money.Divide(divisor);

        // Assert
        result.Amount.Should().Be(25.00m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public void Divide_WithZeroDivisor_ShouldThrowDivideByZeroException()
    {
        // Arrange
        Money money = Money.Create(100.00m, "USD");
        decimal divisor = 0m;

        // Act
        Action act = () => money.Divide(divisor);

        // Assert
        act.Should().Throw<DivideByZeroException>();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        Money money = Money.Create(1234.56m, "USD");

        // Act
        string result = money.ToString();

        // Assert
        result.Should().Be("1,234.56 USD");
    }

    [Fact]
    public void Equality_WithSameAmountAndCurrency_ShouldBeEqual()
    {
        // Arrange
        Money money1 = Money.Create(100.00m, "USD");
        Money money2 = Money.Create(100.00m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
        money1.GetHashCode().Should().Be(money2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentAmount_ShouldNotBeEqual()
    {
        // Arrange
        Money money1 = Money.Create(100.00m, "USD");
        Money money2 = Money.Create(200.00m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void Equality_WithDifferentCurrency_ShouldNotBeEqual()
    {
        // Arrange
        Money money1 = Money.Create(100.00m, "USD");
        Money money2 = Money.Create(100.00m, "EUR");

        // Act & Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void Equality_WithDifferentCurrencyCase_ShouldBeEqual()
    {
        // Arrange
        Money money1 = Money.Create(100.00m, "USD");
        Money money2 = Money.Create(100.00m, "usd");

        // Act & Assert
        money1.Should().Be(money2);
    }
}
