using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using FAM.Domain.Organizations;
using FAM.Domain.Users;
using FluentAssertions;

namespace FAM.Domain.Tests.Entities.Authorization;

public class UserNodeRoleTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUserNodeRole()
    {
        // Arrange
        var user = User.Create("testuser", "test@example.com", "MySecurePass123!");
        var node = CreateTestOrgNode();
        var role = Role.Create("admin", "Administrator", 1);
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddDays(30);

        // Act
        var userNodeRole = UserNodeRole.Create(user, node, role, startAt, endAt);

        // Assert
        userNodeRole.Should().NotBeNull();
        userNodeRole.UserId.Should().Be(user.Id);
        userNodeRole.User.Should().Be(user);
        userNodeRole.NodeId.Should().Be(node.Id);
        userNodeRole.Node.Should().Be(node);
        userNodeRole.RoleId.Should().Be(role.Id);
        userNodeRole.Role.Should().Be(role);
        userNodeRole.StartAt.Should().Be(startAt);
        userNodeRole.EndAt.Should().Be(endAt);
    }

    [Fact]
    public void Create_WithEndDateBeforeStartDate_ShouldThrowDomainException()
    {
        // Arrange
        var user = User.Create("testuser", "test@example.com", "MySecurePass123!");
        var node = CreateTestOrgNode();
        var role = Role.Create("admin", "Administrator", 1);
        var startAt = DateTime.UtcNow;
        var endAt = startAt.AddDays(-1);

        // Act
        Action act = () => UserNodeRole.Create(user, node, role, startAt, endAt);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("End date must be after start date");
    }

    [Fact]
    public void UpdateDates_WithValidDates_ShouldUpdateDates()
    {
        // Arrange
        var userNodeRole = UserNodeRole.Create(
            User.Create("testuser", "test@example.com", "MySecurePass123!"),
            CreateTestOrgNode(),
            Role.Create("admin", "Administrator", 1));
        var newStartAt = DateTime.UtcNow;
        var newEndAt = newStartAt.AddDays(60);

        // Act
        userNodeRole.UpdateDates(newStartAt, newEndAt);

        // Assert
        userNodeRole.StartAt.Should().Be(newStartAt);
        userNodeRole.EndAt.Should().Be(newEndAt);
    }

    [Fact]
    public void UpdateDates_WithEndDateBeforeStartDate_ShouldThrowDomainException()
    {
        // Arrange
        var userNodeRole = UserNodeRole.Create(
            User.Create("testuser", "test@example.com", "MySecurePass123!"),
            CreateTestOrgNode(),
            Role.Create("admin", "Administrator", 1));
        var newStartAt = DateTime.UtcNow;
        var newEndAt = newStartAt.AddDays(-1);

        // Act
        var act = () => userNodeRole.UpdateDates(newStartAt, newEndAt);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("End date must be after start date");
    }

    private static OrgNode CreateTestOrgNode()
    {
        var details = CompanyDetails.Create();
        return OrgNode.CreateCompany("Test Company", details);
    }
}