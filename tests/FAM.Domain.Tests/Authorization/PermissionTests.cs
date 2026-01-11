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
        string resource = "assets";
        string action = "view";

        // Act
        Permission permission = Permission.Create(resource, action);

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
        foreach ((string resource, string action, string _) in Permissions.All)
        {
            Permission permission = Permission.Create(resource, action);
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
        string resource = "invalid_resource";
        string action = "invalid_action";

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
        string resource = "assets";
        string action = "invalid_action";

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
        string resource = "invalid_resource";
        string action = "view";

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
        string resource = "assets";
        string action = "view";
        string description = "View asset information";

        // Act
        Permission permission = Permission.Create(resource, action, description);

        // Assert
        permission.Should().NotBeNull();
        permission.Description.Should().Be(description);
    }

    [Fact]
    public void GetPermissionKey_ShouldReturnCorrectFormat()
    {
        // Arrange
        string resource = "assets";
        string action = "view";
        Permission permission = Permission.Create(resource, action);

        // Act
        string key = permission.GetPermissionKey();

        // Assert
        key.Should().Be($"{resource}:{action}");
    }

    [Fact]
    public void Create_WithEmptyResource_ShouldThrowDomainException()
    {
        // Arrange
        string resource = "";
        string action = "view";

        // Act
        Action act = () => Permission.Create(resource, action);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithEmptyAction_ShouldThrowDomainException()
    {
        // Arrange
        string resource = "assets";
        string action = "";

        // Act
        Action act = () => Permission.Create(resource, action);

        // Assert
        act.Should().Throw<DomainException>();
    }
}
