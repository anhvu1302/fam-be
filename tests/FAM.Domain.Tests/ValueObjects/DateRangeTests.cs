using FAM.Domain.Common;
using FAM.Domain.ValueObjects;
using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class DateRangeTests
{
    [Fact]
    public void Create_WithValidDates_ShouldCreateDateRange()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);

        // Act
        var dateRange = DateRange.Create(startDate, endDate);

        // Assert
        dateRange.Should().NotBeNull();
        dateRange.StartDate.Should().Be(startDate);
        dateRange.EndDate.Should().Be(endDate);
    }

    [Fact]
    public void Create_WithSameStartAndEndDate_ShouldCreateDateRange()
    {
        // Arrange
        var date = new DateTime(2023, 6, 15);

        // Act
        var dateRange = DateRange.Create(date, date);

        // Assert
        dateRange.StartDate.Should().Be(date);
        dateRange.EndDate.Should().Be(date);
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ShouldThrowDomainException()
    {
        // Arrange
        var startDate = new DateTime(2023, 12, 31);
        var endDate = new DateTime(2023, 1, 1);

        // Act & Assert
        Assert.Throws<DomainException>(() => DateRange.Create(startDate, endDate));
    }

    [Fact]
    public void CreateFromDuration_WithValidData_ShouldCreateDateRange()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var durationInDays = 30;

        // Act
        var dateRange = DateRange.CreateFromDuration(startDate, durationInDays);

        // Assert
        dateRange.StartDate.Should().Be(startDate);
        dateRange.EndDate.Should().Be(startDate.AddDays(durationInDays));
    }

    [Fact]
    public void CreateFromDuration_WithZeroDuration_ShouldCreateDateRange()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var durationInDays = 0;

        // Act
        var dateRange = DateRange.CreateFromDuration(startDate, durationInDays);

        // Assert
        dateRange.StartDate.Should().Be(startDate);
        dateRange.EndDate.Should().Be(startDate);
    }

    [Fact]
    public void GetDurationInDays_WithValidRange_ShouldReturnCorrectDuration()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 1, 31);
        var dateRange = DateRange.Create(startDate, endDate);

        // Act
        var duration = dateRange.GetDurationInDays();

        // Assert
        duration.Should().Be(30);
    }

    [Fact]
    public void GetDurationInDays_WithSameDates_ShouldReturnZero()
    {
        // Arrange
        var date = new DateTime(2023, 1, 1);
        var dateRange = DateRange.Create(date, date);

        // Act
        var duration = dateRange.GetDurationInDays();

        // Assert
        duration.Should().Be(0);
    }

    [Fact]
    public void Contains_WithDateWithinRange_ShouldReturnTrue()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);
        var dateRange = DateRange.Create(startDate, endDate);
        var testDate = new DateTime(2023, 6, 15);

        // Act
        var result = dateRange.Contains(testDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithDateEqualToStartDate_ShouldReturnTrue()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);
        var dateRange = DateRange.Create(startDate, endDate);

        // Act
        var result = dateRange.Contains(startDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithDateEqualToEndDate_ShouldReturnTrue()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);
        var dateRange = DateRange.Create(startDate, endDate);

        // Act
        var result = dateRange.Contains(endDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithDateBeforeStartDate_ShouldReturnFalse()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);
        var dateRange = DateRange.Create(startDate, endDate);
        var testDate = new DateTime(2022, 12, 31);

        // Act
        var result = dateRange.Contains(testDate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithDateAfterEndDate_ShouldReturnFalse()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);
        var dateRange = DateRange.Create(startDate, endDate);
        var testDate = new DateTime(2024, 1, 1);

        // Act
        var result = dateRange.Contains(testDate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WithOverlappingRange_ShouldReturnTrue()
    {
        // Arrange
        var range1 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 6, 30));
        var range2 = DateRange.Create(new DateTime(2023, 6, 1), new DateTime(2023, 12, 31));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_WithAdjacentRange_ShouldReturnFalse()
    {
        // Arrange
        var range1 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 6, 30));
        var range2 = DateRange.Create(new DateTime(2023, 7, 1), new DateTime(2023, 12, 31));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WithNonOverlappingRange_ShouldReturnFalse()
    {
        // Arrange
        var range1 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 3, 31));
        var range2 = DateRange.Create(new DateTime(2023, 7, 1), new DateTime(2023, 12, 31));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WithSameRange_ShouldReturnTrue()
    {
        // Arrange
        var range1 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 12, 31));
        var range2 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 12, 31));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_WithContainedRange_ShouldReturnTrue()
    {
        // Arrange
        var range1 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 12, 31));
        var range2 = DateRange.Create(new DateTime(2023, 3, 1), new DateTime(2023, 10, 31));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);
        var dateRange = DateRange.Create(startDate, endDate);

        // Act
        var result = dateRange.ToString();

        // Assert
        result.Should().Be("2023-01-01 to 2023-12-31");
    }

    [Fact]
    public void Equality_WithSameDates_ShouldBeEqual()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);
        var dateRange1 = DateRange.Create(startDate, endDate);
        var dateRange2 = DateRange.Create(startDate, endDate);

        // Act & Assert
        dateRange1.Should().Be(dateRange2);
        (dateRange1 == dateRange2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentDates_ShouldNotBeEqual()
    {
        // Arrange
        var dateRange1 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 12, 31));
        var dateRange2 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 12, 30));

        // Act & Assert
        dateRange1.Should().NotBe(dateRange2);
        (dateRange1 != dateRange2).Should().BeTrue();
    }
}