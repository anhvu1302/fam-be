using FluentAssertions;
using FAM.Domain.Conditions;
using Xunit;

namespace FAM.Domain.Tests.Conditions;

public class AssetConditionTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateAssetCondition()
    {
        // Arrange
        var name = "Good";

        // Act
        var condition = AssetCondition.Create(name);

        // Assert
        condition.Should().NotBeNull();
        condition.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithNameAndDescription_ShouldCreateAssetConditionWithDescription()
    {
        // Arrange
        var name = "Excellent";
        var description = "Asset is in excellent condition";

        // Act
        var condition = AssetCondition.Create(name, description);

        // Assert
        condition.Name.Should().Be(name);
        condition.Description.Should().Be(description);
    }

    [Fact]
    public void Create_WithNullDescription_ShouldCreateAssetConditionWithNullDescription()
    {
        // Arrange
        var name = "Fair";

        // Act
        var condition = AssetCondition.Create(name, null);

        // Assert
        condition.Name.Should().Be(name);
        condition.Description.Should().BeNull();
    }
}