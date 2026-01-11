using FAM.Domain.Common.Base;
using FAM.Domain.Geography;

using FluentAssertions;

namespace FAM.Domain.Tests.Geography;

public class CountryTests
{
    [Fact]
    public void Create_WithValidCodeAndName_ShouldCreateCountry()
    {
        // Arrange
        string code = "VN";
        string name = "Vietnam";

        // Act
        Country country = Country.Create(code, name);

        // Assert
        country.Should().NotBeNull();
        country.Code.Should().Be(code);
        country.Name.Should().Be(name);
        country.IsActive.Should().BeTrue();
        country.IsUNMember.Should().BeTrue();
        country.IsIndependent.Should().BeTrue();
    }

    [Fact]
    public void Create_WithAllParameters_ShouldCreateCountryWithAllFields()
    {
        // Arrange
        string code = "US";
        string name = "United States";
        string iso3Code = "USA";
        string numericCode = "840";

        // Act
        Country country = Country.Create(code, name, iso3Code, numericCode);

        // Assert
        country.Code.Should().Be(code);
        country.Name.Should().Be(name);
        country.Iso3Code.Should().Be(iso3Code);
        country.NumericCode.Should().Be(numericCode);
    }

    [Fact]
    public void Create_WithLowercaseIso3Code_ShouldConvertToUppercase()
    {
        // Arrange
        string code = "VN";
        string name = "Vietnam";
        string iso3Code = "vnm";

        // Act
        Country country = Country.Create(code, name, iso3Code);

        // Assert
        country.Iso3Code.Should().Be("VNM");
    }

    [Fact]
    public void UpdateBasicInfo_WithValidData_ShouldUpdateBasicInfo()
    {
        // Arrange
        Country country = Country.Create("VN", "Vietnam");

        // Act
        country.UpdateBasicInfo("Việt Nam", "Việt Nam", "Cộng hòa Xã hội chủ nghĩa Việt Nam");

        // Assert
        country.Name.Should().Be("Việt Nam");
        country.NativeName.Should().Be("Việt Nam");
        country.OfficialName.Should().Be("Cộng hòa Xã hội chủ nghĩa Việt Nam");
    }

    [Fact]
    public void UpdateRegionalInfo_WithValidData_ShouldUpdateRegionalInfo()
    {
        // Arrange
        Country country = Country.Create("VN", "Vietnam");

        // Act
        country.UpdateRegionalInfo("Asia", "South-Eastern Asia", "AS", 14.0583m, 108.2772m);

        // Assert
        country.Region.Should().Be("Asia");
        country.SubRegion.Should().Be("South-Eastern Asia");
        country.Continent.Should().Be("AS");
        country.Latitude.Should().Be(14.0583m);
        country.Longitude.Should().Be(108.2772m);
    }

    [Fact]
    public void UpdateCommunication_WithValidData_ShouldUpdateCommunication()
    {
        // Arrange
        Country country = Country.Create("VN", "Vietnam");

        // Act
        country.UpdateCommunication("+84", ".vn");

        // Assert
        country.PhoneCode.Should().Be("+84");
        country.TLD.Should().Be(".vn");
    }

    [Fact]
    public void UpdateCurrency_WithValidData_ShouldUpdateCurrency()
    {
        // Arrange
        Country country = Country.Create("VN", "Vietnam");

        // Act
        country.UpdateCurrency("VND", "Vietnamese Dong", "₫");

        // Assert
        country.CurrencyCode.Should().Be("VND");
        country.CurrencyName.Should().Be("Vietnamese Dong");
        country.CurrencySymbol.Should().Be("₫");
    }

    [Fact]
    public void UpdateLanguage_WithValidData_ShouldUpdateLanguage()
    {
        // Arrange
        Country country = Country.Create("VN", "Vietnam");

        // Act
        country.UpdateLanguage("vi", "[\"vi\",\"en\"]");

        // Assert
        country.PrimaryLanguage.Should().Be("vi");
        country.Languages.Should().Be("[\"vi\",\"en\"]");
    }

    [Fact]
    public void UpdateAdministrative_WithValidData_ShouldUpdateAdministrative()
    {
        // Arrange
        Country country = Country.Create("VN", "Vietnam");

        // Act
        country.UpdateAdministrative("Hanoi", "Vietnamese", "[\"UTC+07:00\"]");

        // Assert
        country.Capital.Should().Be("Hanoi");
        country.Nationality.Should().Be("Vietnamese");
        country.TimeZones.Should().Be("[\"UTC+07:00\"]");
    }

    [Fact]
    public void UpdateStatus_WithValidData_ShouldUpdateStatus()
    {
        // Arrange
        Country country = Country.Create("VN", "Vietnam");

        // Act
        country.UpdateStatus(false, false, true, true);

        // Assert
        country.IsActive.Should().BeFalse();
        country.IsEUMember.Should().BeFalse();
        country.IsUNMember.Should().BeTrue();
        country.IsIndependent.Should().BeTrue();
    }

    [Fact]
    public void UpdateAdditionalInfo_WithValidData_ShouldUpdateAdditionalInfo()
    {
        // Arrange
        Country country = Country.Create("VN", "Vietnam");

        // Act
        country.UpdateAdditionalInfo("🇻🇳", "https://example.com/coat.png", 97338583, 331212.0m);

        // Assert
        country.Flag.Should().Be("🇻🇳");
        country.CoatOfArms.Should().Be("https://example.com/coat.png");
        country.Population.Should().Be(97338583);
        country.Area.Should().Be(331212.0m);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        Country country = Country.Create("VN", "Vietnam");
        country.Deactivate();

        // Act
        country.Activate();

        // Assert
        country.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        Country country = Country.Create("VN", "Vietnam");

        // Act
        country.Deactivate();

        // Assert
        country.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Create_WithInvalidCountryCode_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => Country.Create("XX", "Invalid Country"));
    }
}
