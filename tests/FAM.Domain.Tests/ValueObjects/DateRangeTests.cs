using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class DateRangeTests
{
    [Fact]
    public void Create_WithValidDates_ShouldCreateDateRange()
    {
        // Arrange
        DateTime startDate = new(2023, 1, 1);
        DateTime endDate = new(2023, 12, 31);

        // Act
        DateRange dateRange = DateRange.Create(startDate, endDate);

        // Assert
        dateRange.Should().NotBeNull();
        dateRange.StartDate.Should().Be(startDate);
        dateRange.EndDate.Should().Be(endDate);
    }

    [Fact]
    public void Create_WithSameStartAndEndDate_ShouldCreateDateRange()
    {
        // Arrange
        DateTime date = new(2023, 6, 15);

        // Act
        DateRange dateRange = DateRange.Create(date, date);

        // Assert
        dateRange.StartDate.Should().Be(date);
        dateRange.EndDate.Should().Be(date);
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ShouldThrowDomainException()
    {
        // Arrange
        DateTime startDate = new(2023, 12, 31);
        DateTime endDate = new(2023, 1, 1);

        // Act & Assert
        Assert.Throws<DomainException>(() => DateRange.Create(startDate, endDate));
    }

    [Fact]
    public void CreateFromDuration_WithValidData_ShouldCreateDateRange()
    {
        // Arrange
        DateTime startDate = new(2023, 1, 1);
        int durationInDays = 30;

        // Act
        DateRange dateRange = DateRange.CreateFromDuration(startDate, durationInDays);

        // Assert
        dateRange.StartDate.Should().Be(startDate);
        dateRange.EndDate.Should().Be(startDate.AddDays(durationInDays));
    }

    [Fact]
    public void CreateFromDuration_WithZeroDuration_ShouldCreateDateRange()
    {
        // Arrange
        DateTime startDate = new(2023, 1, 1);
        int durationInDays = 0;

        // Act
        DateRange dateRange = DateRange.CreateFromDuration(startDate, durationInDays);

        // Assert
        dateRange.StartDate.Should().Be(startDate);
        dateRange.EndDate.Should().Be(startDate);
    }

    [Fact]
    public void GetDurationInDays_WithValidRange_ShouldReturnCorrectDuration()
    {
        // Arrange
        DateTime startDate = new(2023, 1, 1);
        DateTime endDate = new(2023, 1, 31);
        DateRange dateRange = DateRange.Create(startDate, endDate);

        // Act
        int duration = dateRange.GetDurationInDays();

        // Assert
        duration.Should().Be(30);
    }

    [Fact]
    public void GetDurationInDays_WithSameDates_ShouldReturnZero()
    {
        // Arrange
        DateTime date = new(2023, 1, 1);
        DateRange dateRange = DateRange.Create(date, date);

        // Act
        int duration = dateRange.GetDurationInDays();

        // Assert
        duration.Should().Be(0);
    }

    [Fact]
    public void Contains_WithDateWithinRange_ShouldReturnTrue()
    {
        // Arrange
        DateTime startDate = new(2023, 1, 1);
        DateTime endDate = new(2023, 12, 31);
        DateRange dateRange = DateRange.Create(startDate, endDate);
        DateTime testDate = new(2023, 6, 15);

        // Act
        bool result = dateRange.Contains(testDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithDateEqualToStartDate_ShouldReturnTrue()
    {
        // Arrange
        DateTime startDate = new(2023, 1, 1);
        DateTime endDate = new(2023, 12, 31);
        DateRange dateRange = DateRange.Create(startDate, endDate);

        // Act
        bool result = dateRange.Contains(startDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithDateEqualToEndDate_ShouldReturnTrue()
    {
        // Arrange
        DateTime startDate = new(2023, 1, 1);
        DateTime endDate = new(2023, 12, 31);
        DateRange dateRange = DateRange.Create(startDate, endDate);

        // Act
        bool result = dateRange.Contains(endDate);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Contains_WithDateBeforeStartDate_ShouldReturnFalse()
    {
        // Arrange
        DateTime startDate = new(2023, 1, 1);
        DateTime endDate = new(2023, 12, 31);
        DateRange dateRange = DateRange.Create(startDate, endDate);
        DateTime testDate = new(2022, 12, 31);

        // Act
        bool result = dateRange.Contains(testDate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Contains_WithDateAfterEndDate_ShouldReturnFalse()
    {
        // Arrange
        DateTime startDate = new(2023, 1, 1);
        DateTime endDate = new(2023, 12, 31);
        DateRange dateRange = DateRange.Create(startDate, endDate);
        DateTime testDate = new(2024, 1, 1);

        // Act
        bool result = dateRange.Contains(testDate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WithOverlappingRange_ShouldReturnTrue()
    {
        // Arrange
        DateRange range1 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 6, 30));
        DateRange range2 = DateRange.Create(new DateTime(2023, 6, 1), new DateTime(2023, 12, 31));

        // Act
        bool result = range1.Overlaps(range2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_WithAdjacentRange_ShouldReturnFalse()
    {
        // Arrange
        DateRange range1 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 6, 30));
        DateRange range2 = DateRange.Create(new DateTime(2023, 7, 1), new DateTime(2023, 12, 31));

        // Act
        bool result = range1.Overlaps(range2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WithNonOverlappingRange_ShouldReturnFalse()
    {
        // Arrange
        DateRange range1 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 3, 31));
        DateRange range2 = DateRange.Create(new DateTime(2023, 7, 1), new DateTime(2023, 12, 31));

        // Act
        bool result = range1.Overlaps(range2);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WithSameRange_ShouldReturnTrue()
    {
        // Arrange
        DateRange range1 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 12, 31));
        DateRange range2 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 12, 31));

        // Act
        bool result = range1.Overlaps(range2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_WithContainedRange_ShouldReturnTrue()
    {
        // Arrange
        DateRange range1 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 12, 31));
        DateRange range2 = DateRange.Create(new DateTime(2023, 3, 1), new DateTime(2023, 10, 31));

        // Act
        bool result = range1.Overlaps(range2);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        DateTime startDate = new(2023, 1, 1);
        DateTime endDate = new(2023, 12, 31);
        DateRange dateRange = DateRange.Create(startDate, endDate);

        // Act
        string result = dateRange.ToString();

        // Assert
        result.Should().Be("2023-01-01 to 2023-12-31");
    }

    [Fact]
    public void Equality_WithSameDates_ShouldBeEqual()
    {
        // Arrange
        DateTime startDate = new(2023, 1, 1);
        DateTime endDate = new(2023, 12, 31);
        DateRange dateRange1 = DateRange.Create(startDate, endDate);
        DateRange dateRange2 = DateRange.Create(startDate, endDate);

        // Act & Assert
        dateRange1.Should().Be(dateRange2);
        (dateRange1 == dateRange2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentDates_ShouldNotBeEqual()
    {
        // Arrange
        DateRange dateRange1 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 12, 31));
        DateRange dateRange2 = DateRange.Create(new DateTime(2023, 1, 1), new DateTime(2023, 12, 30));

        // Act & Assert
        dateRange1.Should().NotBe(dateRange2);
        (dateRange1 != dateRange2).Should().BeTrue();
    }
}
