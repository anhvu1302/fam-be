using FAM.Domain.Common.Base;
using FAM.Domain.Organizations;

using FluentAssertions;

namespace FAM.Domain.Tests.Entities.Organizations;

public class OrgNodeTests
{
    [Fact]
    public void CreateCompany_WithValidData_ShouldCreateCompanyNode()
    {
        // Arrange
        string name = "Test Company";
        CompanyDetails details = CompanyDetails.Create("1234567890", "example.com");

        // Act
        OrgNode node = OrgNode.CreateCompany(name, details);

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
        string name = "";
        CompanyDetails details = CompanyDetails.Create();

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
        CompanyDetails companyDetails = CompanyDetails.Create();
        OrgNode company = OrgNode.CreateCompany("Test Company", companyDetails);
        string name = "IT Department";
        DepartmentDetails departmentDetails = DepartmentDetails.Create("CC001", 10);

        // Act
        OrgNode node = OrgNode.CreateDepartment(name, departmentDetails, company);

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
        CompanyDetails companyDetails = CompanyDetails.Create();
        OrgNode company = OrgNode.CreateCompany("Test Company", companyDetails);
        string name = "";
        DepartmentDetails departmentDetails = DepartmentDetails.Create();

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
        CompanyDetails companyDetails = CompanyDetails.Create();
        OrgNode company = OrgNode.CreateCompany("Test Company", companyDetails);
        DepartmentDetails departmentDetails1 = DepartmentDetails.Create();
        OrgNode department = OrgNode.CreateDepartment("IT Department", departmentDetails1, company);
        string name = "Sub Department";
        DepartmentDetails departmentDetails2 = DepartmentDetails.Create();

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
        CompanyDetails details = CompanyDetails.Create();
        OrgNode node = OrgNode.CreateCompany("Original Name", details);
        string newName = "Updated Name";

        // Act
        node.UpdateName(newName);

        // Assert
        node.Name.Should().Be(newName);
    }

    [Fact]
    public void UpdateName_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        CompanyDetails details = CompanyDetails.Create();
        OrgNode node = OrgNode.CreateCompany("Original Name", details);
        string newName = "";

        // Act
        Action act = () => node.UpdateName(newName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("OrgNode name cannot be empty");
    }
}
