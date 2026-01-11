using FAM.Domain.Conditions;

using FluentAssertions;

namespace FAM.Domain.Tests.Conditions;

public class AssetConditionTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateAssetCondition()
    {
        // Arrange
        string name = "Good";

        // Act
        AssetCondition condition = AssetCondition.Create(name);

        // Assert
        condition.Should().NotBeNull();
        condition.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithNameAndDescription_ShouldCreateAssetConditionWithDescription()
    {
        // Arrange
        string name = "Excellent";
        string description = "Asset is in excellent condition";

        // Act
        AssetCondition condition = AssetCondition.Create(name, description);

        // Assert
        condition.Name.Should().Be(name);
        condition.Description.Should().Be(description);
    }

    [Fact]
    public void Create_WithNullDescription_ShouldCreateAssetConditionWithNullDescription()
    {
        // Arrange
        string name = "Fair";

        // Act
        AssetCondition condition = AssetCondition.Create(name, null);

        // Assert
        condition.Name.Should().Be(name);
        condition.Description.Should().BeNull();
    }
}
