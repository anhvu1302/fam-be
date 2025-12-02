using FAM.Domain.Statuses;
using FluentAssertions;

namespace FAM.Domain.Tests.Statuses;

public class UsageStatusTests
{
    [Fact]
    public void Create_WithValidCodeAndName_ShouldCreateUsageStatus()
    {
        // Arrange
        var code = "AVAILABLE";
        var name = "Available";

        // Act
        var status = UsageStatus.Create(code, name);

        // Assert
        status.Should().NotBeNull();
        status.Code.Should().Be(code);
        status.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithAllParameters_ShouldCreateUsageStatusWithAllFields()
    {
        // Arrange
        var code = "IN_USE";
        var name = "In Use";
        var description = "Asset is currently assigned and in use";
        var color = "#007bff";
        var orderNo = 2;

        // Act
        var status = UsageStatus.Create(code, name, description, color, orderNo);

        // Assert
        status.Code.Should().Be(code);
        status.Name.Should().Be(name);
        status.Description.Should().Be(description);
        status.Color.Should().Be(color);
        status.OrderNo.Should().Be(orderNo);
    }

    [Fact]
    public void Create_WithNullOptionalParameters_ShouldCreateUsageStatusWithNullValues()
    {
        // Arrange
        var code = "MAINTENANCE";
        var name = "Under Maintenance";

        // Act
        var status = UsageStatus.Create(code, name, null, null, null);

        // Assert
        status.Code.Should().Be(code);
        status.Name.Should().Be(name);
        status.Description.Should().BeNull();
        status.Color.Should().BeNull();
        status.OrderNo.Should().BeNull();
    }

    [Fact]
    public void Create_WithDifferentStatuses_ShouldCreateUsageStatus()
    {
        // Arrange
        var testCases = new[]
        {
            ("AVAILABLE", "Available"),
            ("IN_USE", "In Use"),
            ("ASSIGNED", "Assigned"),
            ("MAINTENANCE", "Under Maintenance"),
            ("REPAIR", "Under Repair"),
            ("LOST", "Lost"),
            ("STOLEN", "Stolen")
        };

        // Act & Assert
        foreach (var (code, name) in testCases)
        {
            var status = UsageStatus.Create(code, name);
            status.Code.Should().Be(code);
            status.Name.Should().Be(name);
        }
    }
}