using FAM.Domain.Finance;

using FluentAssertions;

namespace FAM.Domain.Tests.Finance;

public class FinanceEntryTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateFinanceEntry()
    {
        // Arrange
        long assetId = 1L;
        DateTime period = new(2023, 10, 1);
        string entryType = "depreciation";
        decimal amount = 1000.50m;

        // Act
        FinanceEntry entry = FinanceEntry.Create(assetId, period, entryType, amount);

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
        long assetId = 2L;
        DateTime period = new(2023, 10, 1);
        string entryType = "adjustment";
        decimal amount = -500.00m;
        decimal bookValueAfter = 5000.00m;
        int createdBy = 1;

        // Act
        FinanceEntry entry = FinanceEntry.Create(assetId, period, entryType, amount, bookValueAfter, createdBy);

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
        long assetId = 3L;
        DateTime period = new(2023, 10, 1);
        string entryType = "writeoff";
        decimal amount = 0m;

        // Act
        FinanceEntry entry = FinanceEntry.Create(assetId, period, entryType, amount);

        // Assert
        entry.Amount.Should().Be(0m);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldCreateFinanceEntry()
    {
        // Arrange
        long assetId = 4L;
        DateTime period = new(2023, 10, 1);
        string entryType = "adjustment";
        decimal amount = -1000.00m;

        // Act
        FinanceEntry entry = FinanceEntry.Create(assetId, period, entryType, amount);

        // Assert
        entry.Amount.Should().Be(amount);
    }

    [Fact]
    public void Create_WithPositiveAmount_ShouldCreateFinanceEntry()
    {
        // Arrange
        long assetId = 5L;
        DateTime period = new(2023, 10, 1);
        string entryType = "depreciation";
        decimal amount = 2500.75m;

        // Act
        FinanceEntry entry = FinanceEntry.Create(assetId, period, entryType, amount);

        // Assert
        entry.Amount.Should().Be(amount);
    }

    [Fact]
    public void Create_WithDifferentEntryTypes_ShouldCreateFinanceEntry()
    {
        // Arrange
        long assetId = 6L;
        DateTime period = new(2023, 10, 1);
        decimal amount = 1000.00m;
        string[] entryTypes = new[] { "depreciation", "adjustment", "writeoff", "revaluation" };

        // Act & Assert
        foreach (string entryType in entryTypes)
        {
            FinanceEntry entry = FinanceEntry.Create(assetId, period, entryType, amount);
            entry.EntryType.Should().Be(entryType);
        }
    }

    [Fact]
    public void Create_WithNullBookValueAfter_ShouldCreateFinanceEntryWithNullBookValue()
    {
        // Arrange
        long assetId = 7L;
        DateTime period = new(2023, 10, 1);
        string entryType = "depreciation";
        decimal amount = 500.00m;

        // Act
        FinanceEntry entry = FinanceEntry.Create(assetId, period, entryType, amount, null);

        // Assert
        entry.BookValueAfter.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullCreatedBy_ShouldCreateFinanceEntryWithNullCreatedBy()
    {
        // Arrange
        long assetId = 8L;
        DateTime period = new(2023, 10, 1);
        string entryType = "adjustment";
        decimal amount = 200.00m;

        // Act
        FinanceEntry entry = FinanceEntry.Create(assetId, period, entryType, amount, null, null);

        // Assert
        entry.CreatedById.Should().BeNull();
    }
}
