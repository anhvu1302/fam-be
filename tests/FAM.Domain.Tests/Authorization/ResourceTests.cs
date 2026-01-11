using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using FAM.Domain.Organizations;

using FluentAssertions;

namespace FAM.Domain.Tests.Entities.Authorization;

public class ResourceTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateResource()
    {
        // Arrange
        string type = "asset";
        OrgNode node = CreateTestOrgNode();
        string name = "Test Resource";

        // Act
        Resource resource = Resource.Create(type, node, name);

        // Assert
        resource.Should().NotBeNull();
        string typeValue = resource.Type;
        typeValue.Should().Be("asset");
        resource.NodeId.Should().Be(node.Id);
        resource.Node.Should().Be(node);
        resource.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithEmptyType_ShouldThrowDomainException()
    {
        // Arrange
        string type = "";
        OrgNode node = CreateTestOrgNode();
        string name = "Test Resource";

        // Act
        Action act = () => Resource.Create(type, node, name);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Resource type cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        string type = "asset";
        OrgNode node = CreateTestOrgNode();
        string name = "";

        // Act
        Action act = () => Resource.Create(type, node, name);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Resource name cannot be empty");
    }

    [Fact]
    public void Update_WithValidName_ShouldUpdateResource()
    {
        // Arrange
        Resource resource = Resource.Create("asset", CreateTestOrgNode(), "Original Name");
        string newName = "Updated Name";

        // Act
        resource.Update(newName);

        // Assert
        resource.Name.Should().Be(newName);
    }

    [Fact]
    public void Update_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        Resource resource = Resource.Create("asset", CreateTestOrgNode(), "Original Name");
        string newName = "";

        // Act
        Action act = () => resource.Update(newName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Resource name cannot be empty");
    }

    private static OrgNode CreateTestOrgNode()
    {
        CompanyDetails details = CompanyDetails.Create();
        return OrgNode.CreateCompany("Test Company", details);
    }
}
