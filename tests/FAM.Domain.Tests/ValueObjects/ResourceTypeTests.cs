using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class ResourceTypeTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateResourceType()
    {
        // Arrange
        string value = "asset";

        // Act
        ResourceType resourceType = ResourceType.Create(value);

        // Assert
        resourceType.Should().NotBeNull();
        resourceType.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        string value = "";

        // Act
        Action act = () => ResourceType.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Resource type cannot be empty");
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowDomainException()
    {
        // Arrange
        string value = new('A', 51);

        // Act
        Action act = () => ResourceType.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Resource type cannot exceed 50 characters");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        ResourceType resourceType = ResourceType.Create("asset");

        // Act
        string value = resourceType;

        // Assert
        value.Should().Be("asset");
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromString()
    {
        // Arrange
        string value = "asset";

        // Act
        ResourceType resourceType = (ResourceType)value;

        // Assert
        resourceType.Value.Should().Be(value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        ResourceType resourceType = ResourceType.Create("asset");

        // Act
        string result = resourceType.ToString();

        // Assert
        result.Should().Be("asset");
    }
}
