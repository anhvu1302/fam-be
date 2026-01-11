using FAM.Domain.Statuses;

using FluentAssertions;

namespace FAM.Domain.Tests.Statuses;

public class LifecycleStatusTests
{
    [Fact]
    public void Create_WithValidCodeAndName_ShouldCreateLifecycleStatus()
    {
        // Arrange
        string code = "DRAFT";
        string name = "Draft";

        // Act
        LifecycleStatus status = LifecycleStatus.Create(code, name);

        // Assert
        status.Should().NotBeNull();
        status.Code.Should().Be(code);
        status.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithAllParameters_ShouldCreateLifecycleStatusWithAllFields()
    {
        // Arrange
        string code = "ACTIVE";
        string name = "Active";
        string description = "Asset is currently in active use";
        string color = "#28a745";
        int orderNo = 3;

        // Act
        LifecycleStatus status = LifecycleStatus.Create(code, name, description, color, orderNo);

        // Assert
        status.Code.Should().Be(code);
        status.Name.Should().Be(name);
        status.Description.Should().Be(description);
        status.Color.Should().Be(color);
        status.OrderNo.Should().Be(orderNo);
    }

    [Fact]
    public void Create_WithNullOptionalParameters_ShouldCreateLifecycleStatusWithNullValues()
    {
        // Arrange
        string code = "RETIRED";
        string name = "Retired";

        // Act
        LifecycleStatus status = LifecycleStatus.Create(code, name, null, null, null);

        // Assert
        status.Code.Should().Be(code);
        status.Name.Should().Be(name);
        status.Description.Should().BeNull();
        status.Color.Should().BeNull();
        status.OrderNo.Should().BeNull();
    }

    [Fact]
    public void Create_WithDifferentStatuses_ShouldCreateLifecycleStatus()
    {
        // Arrange
        (string, string)[] testCases = new[]
        {
            ("DRAFT", "Draft"),
            ("PENDING_APPROVAL", "Pending Approval"),
            ("APPROVED", "Approved"),
            ("ACTIVE", "Active"),
            ("MAINTENANCE", "Under Maintenance"),
            ("RETIRED", "Retired"),
            ("DISPOSED", "Disposed")
        };

        // Act & Assert
        foreach ((string code, string name) in testCases)
        {
            LifecycleStatus status = LifecycleStatus.Create(code, name);
            status.Code.Should().Be(code);
            status.Name.Should().Be(name);
        }
    }
}
