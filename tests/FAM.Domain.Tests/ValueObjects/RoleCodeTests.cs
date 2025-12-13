using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class RoleCodeTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateRoleCode()
    {
        // Arrange
        var value = "admin";

        // Act
        var roleCode = RoleCode.Create(value);

        // Assert
        roleCode.Should().NotBeNull();
        roleCode.Value.Should().Be(value.ToUpperInvariant());
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        var value = "";

        // Act
        Action act = () => RoleCode.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Role code cannot be empty");
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowDomainException()
    {
        // Arrange
        var value = new string('A', 21);

        // Act
        Action act = () => RoleCode.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Role code cannot exceed 20 characters");
    }

    [Fact]
    public void Create_WithInvalidCharacters_ShouldThrowDomainException()
    {
        // Arrange
        var value = "admin@123";

        // Act
        Action act = () => RoleCode.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Role code can only contain uppercase letters, numbers, and underscores");
    }

    [Fact]
    public void Create_WithLowercase_ShouldConvertToUppercase()
    {
        // Arrange
        var value = "admin";

        // Act
        var roleCode = RoleCode.Create(value);

        // Assert
        roleCode.Value.Should().Be("ADMIN");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var roleCode = RoleCode.Create("admin");

        // Act
        string value = roleCode;

        // Assert
        value.Should().Be("ADMIN");
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromString()
    {
        // Arrange
        var value = "admin";

        // Act
        var roleCode = (RoleCode)value;

        // Assert
        roleCode.Value.Should().Be("ADMIN");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var roleCode = RoleCode.Create("admin");

        // Act
        var result = roleCode.ToString();

        // Assert
        result.Should().Be("ADMIN");
    }
}
