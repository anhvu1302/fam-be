using FluentAssertions;
using FAM.Domain.Statuses;
using Xunit;

namespace FAM.Domain.Tests.Statuses;

public class AssetEventTypeTests
{
    [Fact]
    public void Create_WithValidCodeAndName_ShouldCreateAssetEventType()
    {
        // Arrange
        var code = "CREATED";
        var name = "Asset Created";

        // Act
        var eventType = AssetEventType.Create(code, name);

        // Assert
        eventType.Should().NotBeNull();
        eventType.Code.Should().Be(code);
        eventType.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithAllParameters_ShouldCreateAssetEventTypeWithAllFields()
    {
        // Arrange
        var code = "APPROVED";
        var name = "Asset Approved";
        var description = "Asset has been approved for use";
        var color = "#28a745";
        var orderNo = 2;

        // Act
        var eventType = AssetEventType.Create(code, name, description, color, orderNo);

        // Assert
        eventType.Code.Should().Be(code);
        eventType.Name.Should().Be(name);
        eventType.Description.Should().Be(description);
        eventType.Color.Should().Be(color);
        eventType.OrderNo.Should().Be(orderNo);
    }

    [Fact]
    public void Create_WithNullOptionalParameters_ShouldCreateAssetEventTypeWithNullValues()
    {
        // Arrange
        var code = "TRANSFERRED";
        var name = "Asset Transferred";

        // Act
        var eventType = AssetEventType.Create(code, name, null, null, null);

        // Assert
        eventType.Code.Should().Be(code);
        eventType.Name.Should().Be(name);
        eventType.Description.Should().BeNull();
        eventType.Color.Should().BeNull();
        eventType.OrderNo.Should().BeNull();
    }

    [Fact]
    public void Create_WithDifferentCodes_ShouldCreateAssetEventType()
    {
        // Arrange
        var testCases = new[]
        {
            ("CREATED", "Asset Created"),
            ("APPROVED", "Asset Approved"),
            ("ASSIGNED", "Asset Assigned"),
            ("RETURNED", "Asset Returned"),
            ("MAINTAINED", "Asset Maintained"),
            ("DISPOSED", "Asset Disposed")
        };

        // Act & Assert
        foreach (var (code, name) in testCases)
        {
            var eventType = AssetEventType.Create(code, name);
            eventType.Code.Should().Be(code);
            eventType.Name.Should().Be(name);
        }
    }
}