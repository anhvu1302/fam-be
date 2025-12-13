using FAM.Domain.Authorization;
using FAM.Domain.Authorization.Events;
using FAM.Domain.Common.Base;

using FluentAssertions;

namespace FAM.Domain.Tests.Entities.Authorization;

public class RoleTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateRole()
    {
        // Arrange
        var code = "admin";
        var name = "Administrator";
        var description = "System administrator role";
        var rank = 1;

        // Act
        var role = Role.Create(code, name, rank, description);

        // Assert
        role.Should().NotBeNull();
        string codeValue = role.Code;
        codeValue.Should().Be("ADMIN");
        role.Name.Should().Be(name);
        role.Description.Should().Be(description);
        role.Rank.Should().Be(rank);
        role.IsSystemRole.Should().BeFalse();
    }

    [Fact]
    public void Create_WithSystemRoleFlag_ShouldCreateSystemRole()
    {
        // Arrange
        var code = "admin";
        var name = "Administrator";
        var rank = 1;

        // Act
        var role = Role.Create(code, name, rank, isSystemRole: true);

        // Assert
        role.Should().NotBeNull();
        role.IsSystemRole.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        // Arrange
        var code = "admin";
        var name = "Administrator";
        var rank = 1;

        // Act
        var role = Role.Create(code, name, rank);

        // Assert
        role.DomainEvents.Should().HaveCount(1);
        IDomainEvent domainEvent = role.DomainEvents.First();
        domainEvent.Should().BeOfType<RoleCreated>();
        var roleCreatedEvent = (RoleCreated)domainEvent;
        roleCreatedEvent.RoleId.Should().Be(role.Id);
        roleCreatedEvent.Code.Should().Be(code); // Event stores original input code
        roleCreatedEvent.Name.Should().Be(name);
        roleCreatedEvent.Rank.Should().Be(rank);
    }

    [Fact]
    public void Create_WithEmptyCode_ShouldThrowDomainException()
    {
        // Arrange
        var code = "";
        var name = "Administrator";
        var rank = 1;

        // Act
        Action act = () => Role.Create(code, name, rank);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Role code cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        var code = "admin";
        var name = "";
        var rank = 1;

        // Act
        Action act = () => Role.Create(code, name, rank);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(ErrorCodes.ROLE_NAME_REQUIRED);
    }

    [Fact]
    public void Create_WithNegativeRank_ShouldThrowDomainException()
    {
        // Arrange
        var code = "admin";
        var name = "Administrator";
        var rank = -1;

        // Act
        Action act = () => Role.Create(code, name, rank);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(ErrorCodes.ROLE_INVALID_RANK);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateRole()
    {
        // Arrange
        var role = Role.Create("admin", "Administrator", 1);
        var newName = "Super Admin";
        var newDescription = "Super administrator role";
        var newRank = 2;

        // Act
        role.Update(newName, newRank, newDescription);

        // Assert
        role.Name.Should().Be(newName);
        role.Description.Should().Be(newDescription);
        role.Rank.Should().Be(newRank);
        role.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Update_ShouldRaiseDomainEvent()
    {
        // Arrange
        var role = Role.Create("admin", "Administrator", 1);
        role.ClearDomainEvents();
        var newName = "Super Admin";
        var newRank = 2;

        // Act
        role.Update(newName, newRank);

        // Assert
        role.DomainEvents.Should().HaveCount(1);
        IDomainEvent domainEvent = role.DomainEvents.First();
        domainEvent.Should().BeOfType<RoleUpdated>();
        var roleUpdatedEvent = (RoleUpdated)domainEvent;
        roleUpdatedEvent.RoleId.Should().Be(role.Id);
        roleUpdatedEvent.Name.Should().Be(newName);
        roleUpdatedEvent.Rank.Should().Be(newRank);
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        var role = Role.Create("admin", "Administrator", 1);
        var newName = "";
        var newRank = 2;

        // Act
        Action act = () => role.Update(newName, newRank);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(ErrorCodes.ROLE_NAME_REQUIRED);
    }

    [Fact]
    public void Update_SystemRole_ShouldThrowDomainException()
    {
        // Arrange
        var role = Role.Create("admin", "Administrator", 1, isSystemRole: true);
        var newName = "Super Admin";
        var newRank = 2;

        // Act
        Action act = () => role.Update(newName, newRank);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(ErrorCodes.ROLE_SYSTEM_ROLE_CANNOT_UPDATE);
    }

    [Fact]
    public void Update_WithNegativeRank_ShouldThrowDomainException()
    {
        // Arrange
        var role = Role.Create("admin", "Administrator", 1);
        var newName = "Super Admin";
        var newRank = -1;

        // Act
        Action act = () => role.Update(newName, newRank);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(ErrorCodes.ROLE_INVALID_RANK);
    }

    [Fact]
    public void ValidateCanDelete_WithNonSystemRole_ShouldNotThrowException()
    {
        // Arrange
        var role = Role.Create("custom", "Custom Role", 50);

        // Act
        Action act = () => role.ValidateCanDelete();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateCanDelete_WithSystemRole_ShouldThrowDomainException()
    {
        // Arrange
        var role = Role.Create("admin", "Administrator", 1, isSystemRole: true);

        // Act
        Action act = () => role.ValidateCanDelete();

        // Assert
        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(ErrorCodes.ROLE_SYSTEM_ROLE_CANNOT_DELETE);
    }

    [Fact]
    public void AssignPermissions_WithValidPermissions_ShouldRaiseDomainEvent()
    {
        // Arrange
        var role = Role.Create("admin", "Administrator", 1);
        role.ClearDomainEvents();
        var permissions = new List<Permission>
        {
            Permission.Create("assets", "view"),
            Permission.Create("assets", "create")
        };

        // Act
        role.AssignPermissions(permissions);

        // Assert
        role.DomainEvents.Should().HaveCount(1);
        IDomainEvent domainEvent = role.DomainEvents.First();
        domainEvent.Should().BeOfType<PermissionsAssignedToRole>();
        var assignedEvent = (PermissionsAssignedToRole)domainEvent;
        assignedEvent.RoleId.Should().Be(role.Id);
        assignedEvent.PermissionIds.Should().HaveCount(2);
    }

    [Fact]
    public void AssignPermissions_WithEmptyList_ShouldThrowDomainException()
    {
        // Arrange
        var role = Role.Create("admin", "Administrator", 1);
        var permissions = new List<Permission>();

        // Act
        Action act = () => role.AssignPermissions(permissions);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(ErrorCodes.ROLE_NO_PERMISSIONS_PROVIDED);
    }

    [Fact]
    public void RevokePermissions_WithValidIds_ShouldRaiseDomainEvent()
    {
        // Arrange
        var role = Role.Create("admin", "Administrator", 1);
        role.ClearDomainEvents();
        var permissionIds = new List<long> { 1, 2, 3 };

        // Act
        role.RevokePermissions(permissionIds);

        // Assert
        role.DomainEvents.Should().HaveCount(1);
        IDomainEvent domainEvent = role.DomainEvents.First();
        domainEvent.Should().BeOfType<PermissionsRevokedFromRole>();
        var revokedEvent = (PermissionsRevokedFromRole)domainEvent;
        revokedEvent.RoleId.Should().Be(role.Id);
        revokedEvent.PermissionIds.Should().BeEquivalentTo(permissionIds);
    }

    [Fact]
    public void RevokePermissions_WithEmptyList_ShouldThrowDomainException()
    {
        // Arrange
        var role = Role.Create("admin", "Administrator", 1);
        var permissionIds = new List<long>();

        // Act
        Action act = () => role.RevokePermissions(permissionIds);

        // Assert
        act.Should().Throw<DomainException>()
            .Which.ErrorCode.Should().Be(ErrorCodes.ROLE_NO_PERMISSIONS_PROVIDED);
    }
}
