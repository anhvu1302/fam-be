using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;

using FluentAssertions;

namespace FAM.Domain.Tests.Entities.Authorization;

public class PermissionTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreatePermission()
    {
        // Arrange
        var resource = "assets";
        var action = "view";

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
    public void Create_WithAllValidPermissions_ShouldCreatePermission()
    {
        // Arrange & Act & Assert
        foreach (var (resource, action, _) in Permissions.All)
        {
            var permission = Permission.Create(resource, action);
            permission.Should().NotBeNull();
            string resourceValue = permission.Resource;
            string actionValue = permission.Action;
            resourceValue.Should().Be(resource);
            actionValue.Should().Be(action);
        }
    }

    [Fact]
    public void Create_WithInvalidPermission_ShouldThrowDomainException()
    {
        // Arrange
        var resource = "invalid_resource";
        var action = "invalid_action";

        // Act
        Action act = () => Permission.Create(resource, action);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(ErrorCodes.PERMISSION_INVALID);
    }

    [Fact]
    public void Create_WithValidResourceButInvalidAction_ShouldThrowDomainException()
    {
        // Arrange
        var resource = "assets";
        var action = "invalid_action";

        // Act
        Action act = () => Permission.Create(resource, action);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(ErrorCodes.PERMISSION_INVALID);
    }

    [Fact]
    public void Create_WithInvalidResourceButValidAction_ShouldThrowDomainException()
    {
        // Arrange
        var resource = "invalid_resource";
        var action = "view";

        // Act
        Action act = () => Permission.Create(resource, action);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(ErrorCodes.PERMISSION_INVALID);
    }

    [Fact]
    public void Create_WithDescription_ShouldCreatePermissionWithDescription()
    {
        // Arrange
        var resource = "assets";
        var action = "view";
        var description = "View asset information";

        // Act
        var permission = Permission.Create(resource, action, description);

        // Assert
        permission.Should().NotBeNull();
        permission.Description.Should().Be(description);
    }

    [Fact]
    public void GetPermissionKey_ShouldReturnCorrectFormat()
    {
        // Arrange
        var resource = "assets";
        var action = "view";
        var permission = Permission.Create(resource, action);

        // Act
        var key = permission.GetPermissionKey();

        // Assert
        key.Should().Be($"{resource}:{action}");
    }

    [Fact]
    public void Create_WithEmptyResource_ShouldThrowDomainException()
    {
        // Arrange
        var resource = "";
        var action = "view";

        // Act
        Action act = () => Permission.Create(resource, action);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithEmptyAction_ShouldThrowDomainException()
    {
        // Arrange
        var resource = "assets";
        var action = "";

        // Act
        Action act = () => Permission.Create(resource, action);

        // Assert
        act.Should().Throw<DomainException>();
    }
}
