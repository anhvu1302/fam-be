using FluentAssertions;
using FAM.Domain.Authorization;
using FAM.Domain.Common;

namespace FAM.Domain.Tests.Entities.Authorization;

public class PermissionTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreatePermission()
    {
        // Arrange
        var resource = "asset";
        var action = "read";

        // Act
        var permission = Permission.Create(resource, action);

        // Assert
        permission.Should().NotBeNull();
        string resourceValue = permission.Resource;
        string actionValue = permission.Action;
        resourceValue.Should().Be(resource);
        actionValue.Should().Be(action);
    }

    [Fact]
    public void Create_WithEmptyResource_ShouldThrowDomainException()
    {
        // Arrange
        var resource = "";
        var action = "read";

        // Act
        Action act = () => Permission.Create(resource, action);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Resource type cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyAction_ShouldThrowDomainException()
    {
        // Arrange
        var resource = "asset";
        var action = "";

        // Act
        Action act = () => Permission.Create(resource, action);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Resource action cannot be empty");
    }

    [Fact]
    public void Create_WithNullResource_ShouldThrowDomainException()
    {
        // Arrange
        string? resource = null;
        var action = "read";

        // Act
        Action act = () => Permission.Create(resource!, action);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Resource type cannot be empty");
    }

    [Fact]
    public void Create_WithNullAction_ShouldThrowDomainException()
    {
        // Arrange
        var resource = "asset";
        string? action = null;

        // Act
        Action act = () => Permission.Create(resource, action!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Resource action cannot be empty");
    }

    [Fact]
    public void Create_WithWhitespaceResource_ShouldThrowDomainException()
    {
        // Arrange
        var resource = "   ";
        var action = "read";

        // Act
        Action act = () => Permission.Create(resource, action);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Resource type cannot be empty");
    }

    [Fact]
    public void Create_WithWhitespaceAction_ShouldThrowDomainException()
    {
        // Arrange
        var resource = "asset";
        var action = "   ";

        // Act
        Action act = () => Permission.Create(resource, action);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Resource action cannot be empty");
    }
}