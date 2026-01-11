using FAM.Domain.Organizations;

using FluentAssertions;

namespace FAM.Domain.Tests.Entities.Organizations;

public class CompanyDetailsTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateCompanyDetails()
    {
        // Arrange
        string taxCode = "1234567890";
        string domain = "example.com";
        string address = "123 Main St, City, Country";
        DateTime establishedOn = new(2020, 1, 1);

        // Act
        CompanyDetails details = CompanyDetails.Create(taxCode, domain, address, establishedOn);

        // Assert
        details.Should().NotBeNull();
        string taxCodeValue = details.TaxCode!;
        taxCodeValue.Should().Be("1234567890");
        string domainValue = details.Domain!;
        domainValue.Should().Be("example.com");
        string addressValue = details.Address!.ToString();
        addressValue.Should().Be("123 Main St, City, Country, Unknown, VN");
        details.EstablishedOn.Should().Be(establishedOn);
    }

    [Fact]
    public void Create_WithNullValues_ShouldCreateCompanyDetails()
    {
        // Act
        CompanyDetails details = CompanyDetails.Create();

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
        CompanyDetails details = CompanyDetails.Create();
        string taxCode = "1234567890";
        string domain = "example.com";
        string address = "123 Main St, City, Country";
        DateTime establishedOn = new(2020, 1, 1);

        // Act
        details.Update(taxCode, domain, address, establishedOn);

        // Assert
        string taxCodeValue = details.TaxCode!;
        taxCodeValue.Should().Be("1234567890");
        string domainValue = details.Domain!;
        domainValue.Should().Be("example.com");
        string addressValue = details.Address!.ToString();
        addressValue.Should().Be("123 Main St, City, Country, Unknown, VN");
        details.EstablishedOn.Should().Be(establishedOn);
    }
}
