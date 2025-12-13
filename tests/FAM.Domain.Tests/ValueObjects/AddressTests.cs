using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAddress()
    {
        // Arrange
        var street = "123 Main Street";
        var city = "Ho Chi Minh City";
        var countryCode = "VN";
        var ward = "Ward 1";
        var district = "District 1";
        var province = "Ho Chi Minh";
        var postalCode = "70000";

        // Act
        var address = Address.Create(street, city, countryCode, ward, district, province, postalCode);

        // Assert
        address.Should().NotBeNull();
        address.Street.Should().Be(street);
        address.City.Should().Be(city);
        address.CountryCode.Should().Be(countryCode.ToUpperInvariant());
        address.Ward.Should().Be(ward);
        address.District.Should().Be(district);
        address.Province.Should().Be(province);
        address.PostalCode.Should().Be(postalCode);
    }

    [Fact]
    public void Create_WithNullStreet_ShouldThrowDomainException()
    {
        // Arrange
        string? street = null;
        var city = "Ho Chi Minh City";
        var countryCode = "VN";

        // Act
        Action act = () => Address.Create(street!, city, countryCode);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Street is required");
    }

    [Fact]
    public void Create_WithEmptyStreet_ShouldThrowDomainException()
    {
        // Arrange
        var street = string.Empty;
        var city = "Ho Chi Minh City";
        var countryCode = "VN";

        // Act
        Action act = () => Address.Create(street, city, countryCode);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Street is required");
    }

    [Fact]
    public void Create_WithWhitespaceStreet_ShouldThrowDomainException()
    {
        // Arrange
        var street = "   ";
        var city = "Ho Chi Minh City";
        var countryCode = "VN";

        // Act
        Action act = () => Address.Create(street, city, countryCode);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Street is required");
    }

    [Fact]
    public void Create_WithNullCity_ShouldThrowDomainException()
    {
        // Arrange
        var street = "123 Main Street";
        string? city = null;
        var countryCode = "VN";

        // Act
        Action act = () => Address.Create(street, city!, countryCode);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("City is required");
    }

    [Fact]
    public void Create_WithEmptyCity_ShouldThrowDomainException()
    {
        // Arrange
        var street = "123 Main Street";
        var city = string.Empty;
        var countryCode = "VN";

        // Act
        Action act = () => Address.Create(street, city, countryCode);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("City is required");
    }

    [Fact]
    public void Create_WithNullCountryCode_ShouldThrowDomainException()
    {
        // Arrange
        var street = "123 Main Street";
        var city = "Ho Chi Minh City";
        string? countryCode = null;

        // Act
        Action act = () => Address.Create(street, city, countryCode!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Country code is required");
    }

    [Fact]
    public void Create_WithEmptyCountryCode_ShouldThrowDomainException()
    {
        // Arrange
        var street = "123 Main Street";
        var city = "Ho Chi Minh City";
        var countryCode = string.Empty;

        // Act
        Action act = () => Address.Create(street, city, countryCode);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Country code is required");
    }

    [Fact]
    public void Create_WithLowercaseCountryCode_ShouldNormalizeToUppercase()
    {
        // Arrange
        var street = "123 Main Street";
        var city = "Ho Chi Minh City";
        var countryCode = "vn";

        // Act
        var address = Address.Create(street, city, countryCode);

        // Assert
        address.CountryCode.Should().Be("VN");
    }

    [Fact]
    public void Create_WithMinimalRequiredFields_ShouldCreateAddress()
    {
        // Arrange
        var street = "123 Main Street";
        var city = "Ho Chi Minh City";
        var countryCode = "VN";

        // Act
        var address = Address.Create(street, city, countryCode);

        // Assert
        address.Should().NotBeNull();
        address.Street.Should().Be(street);
        address.City.Should().Be(city);
        address.CountryCode.Should().Be(countryCode);
        address.Ward.Should().BeNull();
        address.District.Should().BeNull();
        address.Province.Should().BeNull();
        address.PostalCode.Should().BeNull();
    }

    [Fact]
    public void Equality_WithSameAddresses_ShouldBeEqual()
    {
        // Arrange
        var address1 = Address.Create("123 Main St", "HCMC", "VN", "Ward 1", "District 1", "Province", "70000");
        var address2 = Address.Create("123 Main St", "HCMC", "VN", "Ward 1", "District 1", "Province", "70000");

        // Act & Assert
        address1.Should().Be(address2);
    }

    [Fact]
    public void Equality_WithDifferentAddresses_ShouldNotBeEqual()
    {
        // Arrange
        var address1 = Address.Create("123 Main St", "HCMC", "VN");
        var address2 = Address.Create("456 Main St", "HCMC", "VN");

        // Act & Assert
        address1.Should().NotBe(address2);
    }

    [Fact]
    public void Equality_WithDifferentCountryCodeCase_ShouldBeEqual()
    {
        // Arrange
        var address1 = Address.Create("123 Main St", "HCMC", "VN");
        var address2 = Address.Create("123 Main St", "HCMC", "vn");

        // Act & Assert
        address1.Should().Be(address2);
    }

    [Fact]
    public void ToString_WithAllFields_ShouldReturnFormattedAddress()
    {
        // Arrange
        var address = Address.Create("123 Main Street", "Ho Chi Minh City", "VN",
            "Ward 1", "District 1", "Ho Chi Minh Province", "70000");

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Be("123 Main Street, Ward 1, District 1, Ho Chi Minh City, Ho Chi Minh Province, VN, 70000");
    }

    [Fact]
    public void ToString_WithMinimalFields_ShouldReturnFormattedAddress()
    {
        // Arrange
        var address = Address.Create("123 Main Street", "Ho Chi Minh City", "VN");

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Be("123 Main Street, Ho Chi Minh City, VN");
    }

    [Fact]
    public void ToString_WithNullOptionalFields_ShouldSkipNullFields()
    {
        // Arrange
        var address = Address.Create("123 Main Street", "Ho Chi Minh City", "VN",
            null, null, null, null);

        // Act
        var result = address.ToString();

        // Assert
        result.Should().Be("123 Main Street, Ho Chi Minh City, VN");
    }
}
