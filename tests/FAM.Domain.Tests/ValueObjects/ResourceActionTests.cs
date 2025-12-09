using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;
using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class ResourceActionTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateResourceAction()
    {
        // Arrange
        var value = "read";

        // Act
        var resourceAction = ResourceAction.Create(value);

        // Assert
        resourceAction.Should().NotBeNull();
        resourceAction.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        var value = "";

        // Act
        Action act = () => ResourceAction.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Resource action cannot be empty");
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowDomainException()
    {
        // Arrange
        var value = new string('A', 51);

        // Act
        Action act = () => ResourceAction.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Resource action cannot exceed 50 characters");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var resourceAction = ResourceAction.Create("read");

        // Act
        string value = resourceAction;

        // Assert
        value.Should().Be("read");
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromString()
    {
        // Arrange
        var value = "read";

        // Act
        var resourceAction = (ResourceAction)value;

        // Assert
        resourceAction.Value.Should().Be(value);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var resourceAction = ResourceAction.Create("read");

        // Act
        var result = resourceAction.ToString();

        // Assert
        result.Should().Be("read");
    }
}