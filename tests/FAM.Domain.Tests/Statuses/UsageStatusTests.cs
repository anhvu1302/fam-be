using FAM.Domain.Statuses;

using FluentAssertions;

namespace FAM.Domain.Tests.Statuses;

public class UsageStatusTests
{
    [Fact]
    public void Create_WithValidCodeAndName_ShouldCreateUsageStatus()
    {
        // Arrange
        string code = "AVAILABLE";
        string name = "Available";

        // Act
        UsageStatus status = UsageStatus.Create(code, name);

        // Assert
        status.Should().NotBeNull();
        status.Code.Should().Be(code);
        status.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithAllParameters_ShouldCreateUsageStatusWithAllFields()
    {
        // Arrange
        string code = "IN_USE";
        string name = "In Use";
        string description = "Asset is currently assigned and in use";
        string color = "#007bff";
        int orderNo = 2;

        // Act
        UsageStatus status = UsageStatus.Create(code, name, description, color, orderNo);

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
        string code = "MAINTENANCE";
        string name = "Under Maintenance";

        // Act
        UsageStatus status = UsageStatus.Create(code, name, null, null, null);

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
        (string, string)[] testCases = new[]
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
        foreach ((string code, string name) in testCases)
        {
            UsageStatus status = UsageStatus.Create(code, name);
            status.Code.Should().Be(code);
            status.Name.Should().Be(name);
        }
    }
}
