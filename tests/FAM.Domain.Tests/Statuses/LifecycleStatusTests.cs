using FAM.Domain.Statuses;
using FluentAssertions;

namespace FAM.Domain.Tests.Statuses;

public class LifecycleStatusTests
{
    [Fact]
    public void Create_WithValidCodeAndName_ShouldCreateLifecycleStatus()
    {
        // Arrange
        var code = "DRAFT";
        var name = "Draft";

        // Act
        var status = LifecycleStatus.Create(code, name);

        // Assert
        status.Should().NotBeNull();
        status.Code.Should().Be(code);
        status.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithAllParameters_ShouldCreateLifecycleStatusWithAllFields()
    {
        // Arrange
        var code = "ACTIVE";
        var name = "Active";
        var description = "Asset is currently in active use";
        var color = "#28a745";
        var orderNo = 3;

        // Act
        var status = LifecycleStatus.Create(code, name, description, color, orderNo);

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
        var code = "RETIRED";
        var name = "Retired";

        // Act
        var status = LifecycleStatus.Create(code, name, null, null, null);

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
        var testCases = new[]
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
        foreach (var (code, name) in testCases)
        {
            var status = LifecycleStatus.Create(code, name);
            status.Code.Should().Be(code);
            status.Name.Should().Be(name);
        }
    }
}