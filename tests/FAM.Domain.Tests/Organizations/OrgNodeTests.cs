using FluentAssertions;
using FAM.Domain.Organizations;
using FAM.Domain.Common;

namespace FAM.Domain.Tests.Entities.Organizations;

public class OrgNodeTests
{
    [Fact]
    public void CreateCompany_WithValidData_ShouldCreateCompanyNode()
    {
        // Arrange
        var name = "Test Company";
        var details = CompanyDetails.Create("1234567890", "example.com");

        // Act
        var node = OrgNode.CreateCompany(name, details);

        // Assert
        node.Should().NotBeNull();
        node.Type.Should().Be(OrgNodeType.Company);
        node.Name.Should().Be(name);
        node.ParentId.Should().BeNull();
        node.CompanyDetails.Should().Be(details);
        node.DepartmentDetails.Should().BeNull();
    }

    [Fact]
    public void CreateCompany_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        var name = "";
        var details = CompanyDetails.Create();

        // Act
        Action act = () => OrgNode.CreateCompany(name, details);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("OrgNode name cannot be empty");
    }

    [Fact]
    public void CreateDepartment_WithValidData_ShouldCreateDepartmentNode()
    {
        // Arrange
        var companyDetails = CompanyDetails.Create();
        var company = OrgNode.CreateCompany("Test Company", companyDetails);
        var name = "IT Department";
        var departmentDetails = DepartmentDetails.Create("CC001", 10);

        // Act
        var node = OrgNode.CreateDepartment(name, departmentDetails, company);

        // Assert
        node.Should().NotBeNull();
        node.Type.Should().Be(OrgNodeType.Department);
        node.Name.Should().Be(name);
        node.ParentId.Should().Be(company.Id);
        node.Parent.Should().Be(company);
        node.CompanyDetails.Should().BeNull();
        node.DepartmentDetails.Should().Be(departmentDetails);
    }

    [Fact]
    public void CreateDepartment_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        var companyDetails = CompanyDetails.Create();
        var company = OrgNode.CreateCompany("Test Company", companyDetails);
        var name = "";
        var departmentDetails = DepartmentDetails.Create();

        // Act
        Action act = () => OrgNode.CreateDepartment(name, departmentDetails, company);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("OrgNode name cannot be empty");
    }

    [Fact]
    public void CreateDepartment_WithNonCompanyParent_ShouldThrowDomainException()
    {
        // Arrange
        var companyDetails = CompanyDetails.Create();
        var company = OrgNode.CreateCompany("Test Company", companyDetails);
        var departmentDetails1 = DepartmentDetails.Create();
        var department = OrgNode.CreateDepartment("IT Department", departmentDetails1, company);
        var name = "Sub Department";
        var departmentDetails2 = DepartmentDetails.Create();

        // Act
        Action act = () => OrgNode.CreateDepartment(name, departmentDetails2, department);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Department parent must be a Company");
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var details = CompanyDetails.Create();
        var node = OrgNode.CreateCompany("Original Name", details);
        var newName = "Updated Name";

        // Act
        node.UpdateName(newName);

        // Assert
        node.Name.Should().Be(newName);
    }

    [Fact]
    public void UpdateName_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        var details = CompanyDetails.Create();
        var node = OrgNode.CreateCompany("Original Name", details);
        var newName = "";

        // Act
        var act = () => node.UpdateName(newName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("OrgNode name cannot be empty");
    }
}