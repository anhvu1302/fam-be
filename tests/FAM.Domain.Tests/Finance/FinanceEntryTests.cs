using FluentAssertions;
using FAM.Domain.Finance;
using Xunit;

namespace FAM.Domain.Tests.Finance;

public class FinanceEntryTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateFinanceEntry()
    {
        // Arrange
        var assetId = 1L;
        var period = new DateTime(2023, 10, 1);
        var entryType = "depreciation";
        var amount = 1000.50m;

        // Act
        var entry = FinanceEntry.Create(assetId, period, entryType, amount);

        // Assert
        entry.Should().NotBeNull();
        entry.AssetId.Should().Be(assetId);
        entry.Period.Should().Be(period);
        entry.EntryType.Should().Be(entryType);
        entry.Amount.Should().Be(amount);
        entry.BookValueAfter.Should().BeNull();
    }

    [Fact]
    public void Create_WithAllParameters_ShouldCreateFinanceEntryWithAllFields()
    {
        // Arrange
        var assetId = 2L;
        var period = new DateTime(2023, 10, 1);
        var entryType = "adjustment";
        var amount = -500.00m;
        var bookValueAfter = 5000.00m;
        var createdBy = 1;

        // Act
        var entry = FinanceEntry.Create(assetId, period, entryType, amount, bookValueAfter, createdBy);

        // Assert
        entry.AssetId.Should().Be(assetId);
        entry.Period.Should().Be(period);
        entry.EntryType.Should().Be(entryType);
        entry.Amount.Should().Be(amount);
        entry.BookValueAfter.Should().Be(bookValueAfter);
        entry.CreatedById.Should().Be(createdBy);
    }

    [Fact]
    public void Create_WithZeroAmount_ShouldCreateFinanceEntry()
    {
        // Arrange
        var assetId = 3L;
        var period = new DateTime(2023, 10, 1);
        var entryType = "writeoff";
        var amount = 0m;

        // Act
        var entry = FinanceEntry.Create(assetId, period, entryType, amount);

        // Assert
        entry.Amount.Should().Be(0m);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldCreateFinanceEntry()
    {
        // Arrange
        var assetId = 4L;
        var period = new DateTime(2023, 10, 1);
        var entryType = "adjustment";
        var amount = -1000.00m;

        // Act
        var entry = FinanceEntry.Create(assetId, period, entryType, amount);

        // Assert
        entry.Amount.Should().Be(amount);
    }

    [Fact]
    public void Create_WithPositiveAmount_ShouldCreateFinanceEntry()
    {
        // Arrange
        var assetId = 5L;
        var period = new DateTime(2023, 10, 1);
        var entryType = "depreciation";
        var amount = 2500.75m;

        // Act
        var entry = FinanceEntry.Create(assetId, period, entryType, amount);

        // Assert
        entry.Amount.Should().Be(amount);
    }

    [Fact]
    public void Create_WithDifferentEntryTypes_ShouldCreateFinanceEntry()
    {
        // Arrange
        var assetId = 6L;
        var period = new DateTime(2023, 10, 1);
        var amount = 1000.00m;
        var entryTypes = new[] { "depreciation", "adjustment", "writeoff", "revaluation" };

        // Act & Assert
        foreach (var entryType in entryTypes)
        {
            var entry = FinanceEntry.Create(assetId, period, entryType, amount);
            entry.EntryType.Should().Be(entryType);
        }
    }

    [Fact]
    public void Create_WithNullBookValueAfter_ShouldCreateFinanceEntryWithNullBookValue()
    {
        // Arrange
        var assetId = 7L;
        var period = new DateTime(2023, 10, 1);
        var entryType = "depreciation";
        var amount = 500.00m;

        // Act
        var entry = FinanceEntry.Create(assetId, period, entryType, amount, null);

        // Assert
        entry.BookValueAfter.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullCreatedBy_ShouldCreateFinanceEntryWithNullCreatedBy()
    {
        // Arrange
        var assetId = 8L;
        var period = new DateTime(2023, 10, 1);
        var entryType = "adjustment";
        var amount = 200.00m;

        // Act
        var entry = FinanceEntry.Create(assetId, period, entryType, amount, null, null);

        // Assert
        entry.CreatedById.Should().BeNull();
    }
}