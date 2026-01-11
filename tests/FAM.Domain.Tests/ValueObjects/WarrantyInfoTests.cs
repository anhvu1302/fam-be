using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class WarrantyInfoTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateWarrantyInfo()
    {
        // Arrange
        DateTime startDate = DateTime.UtcNow;
        int durationMonths = 12;

        // Act
        WarrantyInfo warrantyInfo = WarrantyInfo.Create(startDate, durationMonths);

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
        DateTime startDate = DateTime.UtcNow;
        int durationMonths = 24;
        string terms = "Full coverage";
        string provider = "Manufacturer";

        // Act
        WarrantyInfo warrantyInfo = WarrantyInfo.Create(startDate, durationMonths, terms, provider);

        // Assert
        warrantyInfo.Terms.Should().Be(terms);
        warrantyInfo.Provider.Should().Be(provider);
    }

    [Fact]
    public void Create_WithZeroDuration_ShouldThrowDomainException()
    {
        // Arrange
        DateTime startDate = DateTime.UtcNow;
        int durationMonths = 0;

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
        DateTime startDate = DateTime.UtcNow;
        int durationMonths = -6;

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
        DateTime startDate = new(2023, 1, 1);
        DateTime endDate = new(2024, 1, 1);

        // Act
        WarrantyInfo warrantyInfo = WarrantyInfo.CreateWithEndDate(startDate, endDate);

        // Assert
        warrantyInfo.StartDate.Should().Be(startDate);
        warrantyInfo.EndDate.Should().Be(endDate);
        warrantyInfo.DurationMonths.Should().Be(12);
    }

    [Fact]
    public void CreateWithEndDate_WithEndDateBeforeStartDate_ShouldThrowDomainException()
    {
        // Arrange
        DateTime startDate = new(2024, 1, 1);
        DateTime endDate = new(2023, 1, 1);

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
        DateTime startDate = DateTime.UtcNow.AddMonths(-6);
        WarrantyInfo warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        bool result = warrantyInfo.IsActive();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WithPastEndDate_ShouldReturnFalse()
    {
        // Arrange
        DateTime startDate = DateTime.UtcNow.AddMonths(-18);
        WarrantyInfo warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        bool result = warrantyInfo.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithPastEndDate_ShouldReturnTrue()
    {
        // Arrange
        DateTime startDate = DateTime.UtcNow.AddMonths(-18);
        WarrantyInfo warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        bool result = warrantyInfo.IsExpired();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WithFutureEndDate_ShouldReturnFalse()
    {
        // Arrange
        DateTime startDate = DateTime.UtcNow.AddMonths(-6);
        WarrantyInfo warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        bool result = warrantyInfo.IsExpired();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DaysRemaining_WithActiveWarranty_ShouldReturnPositiveDays()
    {
        // Arrange
        DateTime startDate = DateTime.UtcNow.AddMonths(-6);
        WarrantyInfo warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        int? result = warrantyInfo.DaysRemaining();

        // Assert
        result.Should().BePositive();
    }

    [Fact]
    public void DaysRemaining_WithExpiredWarranty_ShouldReturnZero()
    {
        // Arrange
        DateTime startDate = DateTime.UtcNow.AddMonths(-18);
        WarrantyInfo warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        int? result = warrantyInfo.DaysRemaining();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void ToString_WithEndDate_ShouldReturnFormattedString()
    {
        // Arrange
        DateTime startDate = new(2023, 1, 1);
        WarrantyInfo warrantyInfo = WarrantyInfo.Create(startDate, 12);

        // Act
        string result = warrantyInfo.ToString();

        // Assert
        result.Should().Be("Warranty until 2024-01-01");
    }
}
