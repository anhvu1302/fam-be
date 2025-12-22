using FAM.Domain.Organizations;

using FluentAssertions;

namespace FAM.Domain.Tests.Entities.Organizations;

public class CompanyDetailsTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCompanyDetails()
    {
        // Arrange
        var taxCode = "1234567890";
        var domain = "example.com";
        var address = "123 Main St, City, Country";
        var establishedOn = new DateTime(2020, 1, 1);

        // Act
        var details = CompanyDetails.Create(taxCode, domain, address, establishedOn);

        // Assert
        details.Should().NotBeNull();
        var taxCodeValue = details.TaxCode!;
        taxCodeValue.Should().Be("1234567890");
        var domainValue = details.Domain!;
        domainValue.Should().Be("example.com");
        var addressValue = details.Address!.ToString();
        addressValue.Should().Be("123 Main St, City, Country, Unknown, VN");
        details.EstablishedOn.Should().Be(establishedOn);
    }

    [Fact]
    public void Create_WithNullValues_ShouldCreateCompanyDetails()
    {
        // Act
        var details = CompanyDetails.Create();

        // Assert
        details.Should().NotBeNull();
        details.TaxCode.Should().BeNull();
        details.Domain.Should().BeNull();
        details.Address.Should().BeNull();
        details.EstablishedOn.Should().BeNull();
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateCompanyDetails()
    {
        // Arrange
        var details = CompanyDetails.Create();
        var taxCode = "1234567890";
        var domain = "example.com";
        var address = "123 Main St, City, Country";
        var establishedOn = new DateTime(2020, 1, 1);

        // Act
        details.Update(taxCode, domain, address, establishedOn);

        // Assert
        var taxCodeValue = details.TaxCode!;
        taxCodeValue.Should().Be("1234567890");
        var domainValue = details.Domain!;
        domainValue.Should().Be("example.com");
        var addressValue = details.Address!.ToString();
        addressValue.Should().Be("123 Main St, City, Country, Unknown, VN");
        details.EstablishedOn.Should().Be(establishedOn);
    }
}
