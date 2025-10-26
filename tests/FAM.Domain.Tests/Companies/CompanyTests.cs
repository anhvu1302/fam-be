using FluentAssertions;
using FAM.Domain.Common;
using FAM.Domain.Companies;
using Xunit;

namespace FAM.Domain.Tests.Companies;

public class CompanyTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateCompany()
    {
        // Arrange
        var name = "ABC Corporation";

        // Act
        var company = Company.Create(name);

        // Assert
        company.Should().NotBeNull();
        company.Name.Should().Be(name);
    }

    [Fact]
    public void Create_WithNameWithWhitespace_ShouldTrimName()
    {
        // Arrange
        var name = "  ABC Corporation  ";

        // Act
        var company = Company.Create(name);

        // Assert
        company.Name.Should().Be("ABC Corporation");
    }

    [Fact]
    public void Create_WithTaxCodeAndAddress_ShouldCreateCompanyWithAllFields()
    {
        // Arrange
        var name = "ABC Corporation";
        var taxCode = "123456789";
        var address = "123 Main St, City, State";

        // Act
        var company = Company.Create(name, taxCode, address);

        // Assert
        company.Name.Should().Be(name);
        company.TaxCode.Should().Be(taxCode);
        company.Address.Should().Be(address);
    }

    [Fact]
    public void Create_WithNullOrEmptyName_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => Company.Create(null!));
        Assert.Throws<DomainException>(() => Company.Create(""));
        Assert.Throws<DomainException>(() => Company.Create("   "));
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateCompany()
    {
        // Arrange
        var company = Company.Create("Old Name");
        var newName = "New Name";
        var newTaxCode = "987654321";
        var newAddress = "456 New St, New City";

        // Act
        company.Update(newName, newTaxCode, newAddress);

        // Assert
        company.Name.Should().Be(newName);
        company.TaxCode.Should().Be(newTaxCode);
        company.Address.Should().Be(newAddress);
    }

    [Fact]
    public void Update_WithNullValues_ShouldUpdateCompany()
    {
        // Arrange
        var company = Company.Create("Test Company", "123456789", "123 Main St");

        // Act
        company.Update("Updated Name", null, null);

        // Assert
        company.Name.Should().Be("Updated Name");
        company.TaxCode.Should().BeNull();
        company.Address.Should().BeNull();
    }
}