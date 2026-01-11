using FAM.Domain.Statuses;

using FluentAssertions;

namespace FAM.Domain.Tests.Statuses;

public class AssetEventTypeTests
{
    [Fact]
    public void Create_WithValidCodeAndName_ShouldCreateAssetEventType()
    {
        // Arrange
        string code = "CREATED";
        string name = "Asset Created";

        // Act
        AssetEventType eventType = AssetEventType.Create(code, name);

        // Assert
        eventType.Should().NotBeNull();
        eventType.Code.Should().Be(code);
        eventType.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithAllParameters_ShouldCreateAssetEventTypeWithAllFields()
    {
        // Arrange
        string code = "APPROVED";
        string name = "Asset Approved";
        string description = "Asset has been approved for use";
        string color = "#28a745";
        int orderNo = 2;

        // Act
        AssetEventType eventType = AssetEventType.Create(code, name, description, color, orderNo);

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
        string code = "TRANSFERRED";
        string name = "Asset Transferred";

        // Act
        AssetEventType eventType = AssetEventType.Create(code, name, null, null, null);

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
        (string, string)[] testCases = new[]
        {
            ("CREATED", "Asset Created"),
            ("APPROVED", "Asset Approved"),
            ("ASSIGNED", "Asset Assigned"),
            ("RETURNED", "Asset Returned"),
            ("MAINTAINED", "Asset Maintained"),
            ("DISPOSED", "Asset Disposed")
        };

        // Act & Assert
        foreach ((string code, string name) in testCases)
        {
            AssetEventType eventType = AssetEventType.Create(code, name);
            eventType.Code.Should().Be(code);
            eventType.Name.Should().Be(name);
        }
    }
}
