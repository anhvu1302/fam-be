using FluentAssertions;
using FAM.Domain.Common;
using FAM.Domain.Departments;
using Xunit;

namespace FAM.Domain.Tests.Departments;

public class DepartmentTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateDepartment()
    {
        // Arrange
        var name = "Human Resources";

        // Act
        var department = Department.Create(name);

        // Assert
        department.Should().NotBeNull();
        department.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithNameCodeAndDescription_ShouldCreateDepartmentWithAllFields()
    {
        // Arrange
        var name = "Information Technology";
        var code = "IT";
        var description = "IT Department";

        // Act
        var department = Department.Create(name, code, description);

        // Assert
        department.Name.Should().Be(name);
        department.Code.Should().Be(code);
        department.Description.Should().Be(description);
    }

    [Fact]
    public void Create_WithNullOrEmptyName_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => Department.Create(null!));
        Assert.Throws<DomainException>(() => Department.Create(""));
        Assert.Throws<DomainException>(() => Department.Create("   "));
    }

    [Fact]
    public void Create_WithNullCodeAndDescription_ShouldCreateDepartment()
    {
        // Arrange
        var name = "Finance";

        // Act
        var department = Department.Create(name, null, null);

        // Assert
        department.Name.Should().Be(name);
        department.Code.Should().BeNull();
        department.Description.Should().BeNull();
    }
}