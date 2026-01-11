using FAM.Domain.Common.Base;
using FAM.Domain.Locations;

using FluentAssertions;

namespace FAM.Domain.Tests.Locations;

public class LocationTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateLocation()
    {
        // Arrange
        string name = "Main Office";

        // Act
        Location location = Location.Create(name);

        // Assert
        location.Should().NotBeNull();
        location.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithNameCodeCompanyIdAndParentId_ShouldCreateLocationWithAllFields()
    {
        // Arrange
        string name = "Branch Office";
        string code = "BO001";
        int companyId = 1;
        int parentId = 2;

        // Act
        Location location = Location.Create(name, code, companyId, parentId);

        // Assert
        location.Name.Should().Be(name);
        location.Code.Should().Be(code);
        location.CompanyId.Should().Be(companyId);
        location.ParentId.Should().Be(parentId);
    }

    [Fact]
    public void Create_WithNullOrEmptyName_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => Location.Create(null!));
        Assert.Throws<DomainException>(() => Location.Create(""));
        Assert.Throws<DomainException>(() => Location.Create("   "));
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateLocation()
    {
        // Arrange
        Location location = Location.Create("Old Name");
        string newName = "New Name";
        string newDescription = "New Description";
        int newCountryId = 1;

        // Act
        location.Update(newName, newDescription, newCountryId);

        // Assert
        location.Name.Should().Be(newName);
        location.Description.Should().Be(newDescription);
        location.CountryId.Should().Be(newCountryId);
    }

    [Fact]
    public void Update_WithNullValues_ShouldUpdateLocation()
    {
        // Arrange
        Location location = Location.Create("Test Location", "CODE", 1, 2);

        // Act
        location.Update("Updated Name", null, null);

        // Assert
        location.Name.Should().Be("Updated Name");
        location.Description.Should().BeNull();
        location.CountryId.Should().BeNull();
    }

    [Fact]
    public void SetParent_WithValidParentId_ShouldSetParent()
    {
        // Arrange
        Location location = Location.Create("Child Location");

        // Act
        location.SetParent(5);

        // Assert
        location.ParentId.Should().Be(5);
    }

    [Fact]
    public void SetParent_WithNullParentId_ShouldSetParentToNull()
    {
        // Arrange
        Location location = Location.Create("Root Location", null, null, 1);

        // Act
        location.SetParent(null);

        // Assert
        location.ParentId.Should().BeNull();
    }

    [Fact]
    public void BuildPath_WithValidData_ShouldBuildPath()
    {
        // Arrange
        Location location = Location.Create("Location");

        // Act
        location.BuildPath("/Company/Main Office/Branch", "1,2,3");

        // Assert
        location.FullPath.Should().Be("/Company/Main Office/Branch");
        location.PathIds.Should().Be("1,2,3");
    }
}
