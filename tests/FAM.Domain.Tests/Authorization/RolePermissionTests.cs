using FluentAssertions;
using FAM.Domain.Authorization;

namespace FAM.Domain.Tests.Entities.Authorization;

public class RolePermissionTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateRolePermission()
    {
        // Arrange
        var role = Role.Create("admin", "Administrator", 1);
        var permission = Permission.Create("asset", "read");

        // Act
        var rolePermission = RolePermission.Create(role, permission);

        // Assert
        rolePermission.Should().NotBeNull();
        rolePermission.RoleId.Should().Be(role.Id);
        rolePermission.Role.Should().Be(role);
        rolePermission.PermissionId.Should().Be(permission.Id);
        rolePermission.Permission.Should().Be(permission);
    }
}