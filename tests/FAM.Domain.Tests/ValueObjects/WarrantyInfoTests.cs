using FluentAssertions;
using FAM.Domain.ValueObjects;
using FAM.Domain.Common;

namespace FAM.Domain.Tests.ValueObjects;

public class WarrantyInfoTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateWarrantyInfo()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var durationMonths = 12;

        // Act
        var warrantyInfo = WarrantyInfo.Create(startDate, durationMonths);

        // Assert
        warrantyInfo.Should().NotBeNull();
        warrantyInfo.StartDate.Should().Be(startDate);
        warrantyInfo.EndDate.Should().Be(startDate.AddMonths(12));
        warrantyInfo.DurationMonths.Should().Be(durationMonths);
        warrantyInfo.Terms.Should().BeNull();
        warrantyInfo.Provider.Should().BeNull();
    }

    [Fact]
    public void Create_WithTermsAndProvider_ShouldCreateWarrantyInfo()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var durationMonths = 24;
        var terms = "Full coverage";
        var provider = "Manufacturer";

        // Act
        var warrantyInfo = WarrantyInfo.Create(startDate, durationMonths, terms, provider);

        // Assert
        warrantyInfo.Terms.Should().Be(terms);
        warrantyInfo.Provider.Should().Be(provider);
    }

    [Fact]
    public void Create_WithZeroDuration_ShouldThrowDomainException()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var durationMonths = 0;

        // Act
        Action act = () => WarrantyInfo.Create(startDate, durationMonths);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Warranty duration must be positive");
    }

    [Fact]
    public void Create_WithNegativeDuration_ShouldThrowDomainException()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var durationMonths = -6;

        // Act
        Action act = () => WarrantyInfo.Create(startDate, durationMonths);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Warranty duration must be positive");
    }

    [Fact]
    public void CreateWithEndDate_WithValidData_ShouldCreateWarrantyInfo()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2024, 1, 1);

        // Act
        var warrantyInfo = WarrantyInfo.CreateWithEndDate(startDate, endDate);

        // Assert
        warrantyInfo.StartDate.Should().Be(startDate);
        warrantyInfo.EndDate.Should().Be(endDate);
        warrantyInfo.DurationMonths.Should().Be(12);
    }

    [Fact]
    public void CreateWithEndDate_WithEndDateBeforeStartDate_ShouldThrowDomainException()
    {
        // Arrange
        var startDate = new DateTime(2024, 1, 1);
        var endDate = new DateTime(2023, 1, 1);

        // Act
        Action act = () => WarrantyInfo.CreateWithEndDate(startDate, endDate);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("End date must be after start date");
    }

    [Fact]
    public void IsActive_WithFutureEndDate_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMonths(-6);
        var warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        var result = warrantyInfo.IsActive();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WithPastEndDate_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMonths(-18);
        var warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        var result = warrantyInfo.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithPastEndDate_ShouldReturnTrue()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMonths(-18);
        var warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        var result = warrantyInfo.IsExpired();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WithFutureEndDate_ShouldReturnFalse()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMonths(-6);
        var warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        var result = warrantyInfo.IsExpired();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DaysRemaining_WithActiveWarranty_ShouldReturnPositiveDays()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMonths(-6);
        var warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        var result = warrantyInfo.DaysRemaining();

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void DaysRemaining_WithExpiredWarranty_ShouldReturnZero()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddMonths(-18);
        var warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        var result = warrantyInfo.DaysRemaining();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ToString_WithEndDate_ShouldReturnFormattedString()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        var result = warrantyInfo.ToString();

        // Assert
        result.Should().Be("Warranty until 2024-01-01");
    }
}