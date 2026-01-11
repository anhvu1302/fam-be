using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class AssetTagTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAssetTag()
    {
        // Arrange
        string value = "TAG001";

        // Act
        AssetTag assetTag = AssetTag.Create(value);

        // Assert
        assetTag.Should().NotBeNull();
        assetTag.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        string value = "";

        // Act
        Action act = () => AssetTag.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Asset tag cannot be empty");
    }

    [Fact]
    public void Create_WithWhitespace_ShouldThrowDomainException()
    {
        // Arrange
        string value = "   ";

        // Act
        Action act = () => AssetTag.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Asset tag cannot be empty");
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowDomainException()
    {
        // Arrange
        string value = new('A', 51);

        // Act
        Action act = () => AssetTag.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Asset tag cannot exceed 50 characters");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        AssetTag assetTag = AssetTag.Create("TAG001");

        // Act
        string value = assetTag;

        // Assert
        value.Should().Be("TAG001");
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromString()
    {
        // Arrange
        string value = "TAG001";

        // Act
        AssetTag assetTag = (AssetTag)value;

        // Assert
        assetTag.Value.Should().Be(value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        AssetTag assetTag = AssetTag.Create("TAG001");

        // Act
        string result = assetTag.ToString();

        // Assert
        result.Should().Be("TAG001");
    }
}
