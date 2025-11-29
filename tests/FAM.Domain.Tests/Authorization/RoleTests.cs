using FluentAssertions;
using FAM.Domain.Authorization;
using FAM.Domain.Common;

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
            .WithMessage("Role name cannot be empty");
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
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        var role = Role.Create("admin", "Administrator", 1);
        var newName = "";
        var newRank = 2;

        // Act
        var act = () => role.Update(newName, newRank);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Role name cannot be empty");
    }
}