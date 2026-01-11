using FAM.Domain.Authorization;

using FluentAssertions;

namespace FAM.Domain.Tests.Entities.Authorization;

public class RolePermissionTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateRolePermission()
    {
        // Arrange
        Role role = Role.Create("admin", "Administrator", 1);
        Permission permission = Permission.Create("assets", "view");

        // Act
        RolePermission rolePermission = RolePermission.Create(role, permission);

        // Assert
        rolePermission.Should().NotBeNull();
        rolePermission.RoleId.Should().Be(role.Id);
        rolePermission.Role.Should().Be(role);
        rolePermission.PermissionId.Should().Be(permission.Id);
        rolePermission.Permission.Should().Be(permission);
    }
}
