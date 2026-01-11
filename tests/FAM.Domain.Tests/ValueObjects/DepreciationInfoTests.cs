using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class DepreciationInfoTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateDepreciationInfo()
    {
        // Arrange
        string method = "StraightLine";
        int usefulLifeMonths = 60;
        decimal residualValue = 1000.00m;

        // Act
        DepreciationInfo depreciationInfo = DepreciationInfo.Create(method, usefulLifeMonths, residualValue);

        // Assert
        depreciationInfo.Should().NotBeNull();
        depreciationInfo.Method.Should().Be(method);
        depreciationInfo.UsefulLifeMonths.Should().Be(usefulLifeMonths);
        depreciationInfo.ResidualValue.Should().Be(residualValue);
        depreciationInfo.InServiceDate.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullOrEmptyMethod_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => DepreciationInfo.Create(null!, 60, 1000));
        Assert.Throws<DomainException>(() => DepreciationInfo.Create("", 60, 1000));
        Assert.Throws<DomainException>(() => DepreciationInfo.Create("   ", 60, 1000));
    }

    [Fact]
    public void Create_WithZeroUsefulLife_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => DepreciationInfo.Create("StraightLine", 0, 1000));
    }

    [Fact]
    public void Create_WithNegativeUsefulLife_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => DepreciationInfo.Create("StraightLine", -1, 1000));
    }

    [Fact]
    public void Create_WithNegativeResidualValue_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => DepreciationInfo.Create("StraightLine", 60, -100));
    }

    [Fact]
    public void Create_WithZeroResidualValue_ShouldCreateDepreciationInfo()
    {
        // Arrange
        string method = "StraightLine";
        int usefulLifeMonths = 60;
        decimal residualValue = 0.00m;

        // Act
        DepreciationInfo depreciationInfo = DepreciationInfo.Create(method, usefulLifeMonths, residualValue);

        // Assert
        depreciationInfo.ResidualValue.Should().Be(0.00m);
    }

    [Fact]
    public void CalculateMonthlyDepreciation_WithStraightLineMethod_ShouldCalculateCorrectly()
    {
        // Arrange
        DepreciationInfo depreciationInfo = DepreciationInfo.Create("StraightLine", 60, 1000.00m);
        decimal purchaseCost = 10000.00m;

        // Act
        decimal monthlyDepreciation = depreciationInfo.CalculateMonthlyDepreciation(purchaseCost);

        // Assert
        monthlyDepreciation.Should().Be(150.00m); // (10000 - 1000) / 60
    }

    [Fact]
    public void CalculateMonthlyDepreciation_WithZeroResidualValue_ShouldCalculateCorrectly()
    {
        // Arrange
        DepreciationInfo depreciationInfo = DepreciationInfo.Create("StraightLine", 60, 0.00m);
        decimal purchaseCost = 6000.00m;

        // Act
        decimal monthlyDepreciation = depreciationInfo.CalculateMonthlyDepreciation(purchaseCost);

        // Assert
        monthlyDepreciation.Should().Be(100.00m); // 6000 / 60
    }

    [Fact]
    public void CalculateMonthlyDepreciation_WithUnknownMethod_ShouldReturnZero()
    {
        // Arrange
        DepreciationInfo depreciationInfo = DepreciationInfo.Create("DecliningBalance", 60, 1000.00m);
        decimal purchaseCost = 10000.00m;

        // Act
        decimal monthlyDepreciation = depreciationInfo.CalculateMonthlyDepreciation(purchaseCost);

        // Assert
        monthlyDepreciation.Should().Be(0);
    }

    [Fact]
    public void CalculateCurrentBookValue_WithZeroElapsedMonths_ShouldReturnPurchaseCost()
    {
        // Arrange
        DepreciationInfo depreciationInfo = DepreciationInfo.Create("StraightLine", 60, 1000.00m);
        decimal purchaseCost = 10000.00m;

        // Act
        decimal bookValue = depreciationInfo.CalculateCurrentBookValue(purchaseCost, 0);

        // Assert
        bookValue.Should().Be(purchaseCost);
    }

    [Fact]
    public void CalculateCurrentBookValue_WithNegativeElapsedMonths_ShouldReturnPurchaseCost()
    {
        // Arrange
        DepreciationInfo depreciationInfo = DepreciationInfo.Create("StraightLine", 60, 1000.00m);
        decimal purchaseCost = 10000.00m;

        // Act
        decimal bookValue = depreciationInfo.CalculateCurrentBookValue(purchaseCost, -1);

        // Assert
        bookValue.Should().Be(purchaseCost);
    }

    [Fact]
    public void CalculateCurrentBookValue_WithPartialDepreciation_ShouldCalculateCorrectly()
    {
        // Arrange
        DepreciationInfo depreciationInfo = DepreciationInfo.Create("StraightLine", 60, 1000.00m);
        decimal purchaseCost = 10000.00m;
        int elapsedMonths = 12;

        // Act
        decimal bookValue = depreciationInfo.CalculateCurrentBookValue(purchaseCost, elapsedMonths);

        // Assert
        bookValue.Should().Be(8200.00m); // 10000 - (150 * 12) = 10000 - 1800 = 8200
    }

    [Fact]
    public void CalculateCurrentBookValue_WithFullDepreciation_ShouldReturnResidualValue()
    {
        // Arrange
        DepreciationInfo depreciationInfo = DepreciationInfo.Create("StraightLine", 60, 1000.00m);
        decimal purchaseCost = 10000.00m;
        int elapsedMonths = 60;

        // Act
        decimal bookValue = depreciationInfo.CalculateCurrentBookValue(purchaseCost, elapsedMonths);

        // Assert
        bookValue.Should().Be(1000.00m); // Residual value
    }

    [Fact]
    public void CalculateCurrentBookValue_WithOverDepreciation_ShouldReturnResidualValue()
    {
        // Arrange
        DepreciationInfo depreciationInfo = DepreciationInfo.Create("StraightLine", 60, 1000.00m);
        decimal purchaseCost = 10000.00m;
        int elapsedMonths = 120; // More than useful life

        // Act
        decimal bookValue = depreciationInfo.CalculateCurrentBookValue(purchaseCost, elapsedMonths);

        // Assert
        bookValue.Should().Be(1000.00m); // Residual value
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        DepreciationInfo depreciationInfo = DepreciationInfo.Create("StraightLine", 60, 1000.00m);

        // Act
        string result = depreciationInfo.ToString();

        // Assert
        result.Should().Be("StraightLine, 60 months, Residual: 1000.00");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        DepreciationInfo depreciationInfo1 = DepreciationInfo.Create("StraightLine", 60, 1000.00m);
        DepreciationInfo depreciationInfo2 = DepreciationInfo.Create("StraightLine", 60, 1000.00m);

        // Act & Assert
        depreciationInfo1.Should().Be(depreciationInfo2);
        (depreciationInfo1 == depreciationInfo2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        DepreciationInfo depreciationInfo1 = DepreciationInfo.Create("StraightLine", 60, 1000.00m);
        DepreciationInfo depreciationInfo2 = DepreciationInfo.Create("StraightLine", 48, 1000.00m);

        // Act & Assert
        depreciationInfo1.Should().NotBe(depreciationInfo2);
        (depreciationInfo1 != depreciationInfo2).Should().BeTrue();
    }
}
