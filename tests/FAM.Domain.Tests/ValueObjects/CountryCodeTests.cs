using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class CountryCodeTests
{
    [Fact]
    public void Create_WithValidCountryCode_ShouldCreateCountryCode()
    {
        // Arrange
        var code = "VN";

        // Act
        var countryCode = CountryCode.Create(code);

        // Assert
        countryCode.Should().NotBeNull();
        countryCode.Value.Should().Be(code);
    }

    [Fact]
    public void Create_WithLowercaseCode_ShouldConvertToUppercase()
    {
        // Arrange
        var code = "vn";

        // Act
        var countryCode = CountryCode.Create(code);

        // Assert
        countryCode.Value.Should().Be("VN");
    }

    [Fact]
    public void Create_WithCodeWithWhitespace_ShouldTrimAndConvertToUppercase()
    {
        // Arrange
        var code = " vn ";

        // Act
        var countryCode = CountryCode.Create(code);

        // Assert
        countryCode.Value.Should().Be("VN");
    }

    [Fact]
    public void Create_WithNullOrEmptyCode_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => CountryCode.Create(null!));
        Assert.Throws<DomainException>(() => CountryCode.Create(""));
        Assert.Throws<DomainException>(() => CountryCode.Create("   "));
    }

    [Fact]
    public void Create_WithInvalidLength_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => CountryCode.Create("V"));
        Assert.Throws<DomainException>(() => CountryCode.Create("VNM"));
        Assert.Throws<DomainException>(() => CountryCode.Create("123"));
    }

    [Fact]
    public void Create_WithInvalidCountryCode_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => CountryCode.Create("XX"));
        Assert.Throws<DomainException>(() => CountryCode.Create("ZZ"));
    }

    [Fact]
    public void Create_WithValidCountryCodes_ShouldCreateSuccessfully()
    {
        // Arrange
        var validCodes = new[] { "VN", "US", "GB", "DE", "FR", "JP", "CN", "KR", "SG", "TH" };

        // Act & Assert
        foreach (var code in validCodes)
        {
            var countryCode = CountryCode.Create(code);
            countryCode.Value.Should().Be(code);
        }
    }

    [Fact]
    public void GetCountryName_WithKnownCountryCode_ShouldReturnCountryName()
    {
        // Arrange
        var countryCode = CountryCode.Create("VN");

        // Act
        var countryName = countryCode.GetCountryName();

        // Assert
        countryName.Should().Be("Vietnam");
    }

    [Fact]
    public void GetCountryName_WithUnknownCountryCode_ShouldReturnUnknown()
    {
        // Arrange
        var countryCode = CountryCode.Create("AF");

        // Act
        var countryName = countryCode.GetCountryName();

        // Assert
        countryName.Should().Be("Unknown");
    }

    [Fact]
    public void IsAsian_WithAsianCountry_ShouldReturnTrue()
    {
        // Arrange
        var asianCodes = new[] { "VN", "CN", "JP", "KR", "SG", "TH" };

        // Act & Assert
        foreach (var code in asianCodes)
        {
            var countryCode = CountryCode.Create(code);
            countryCode.IsAsian().Should().BeTrue();
        }
    }

    [Fact]
    public void IsAsian_WithNonAsianCountry_ShouldReturnFalse()
    {
        // Arrange
        var countryCode = CountryCode.Create("US");

        // Act
        var result = countryCode.IsAsian();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsEuropean_WithEuropeanCountry_ShouldReturnTrue()
    {
        // Arrange
        var europeanCodes = new[] { "GB", "DE", "FR", "IT", "ES" };

        // Act & Assert
        foreach (var code in europeanCodes)
        {
            var countryCode = CountryCode.Create(code);
            countryCode.IsEuropean().Should().BeTrue();
        }
    }

    [Fact]
    public void IsEuropean_WithNonEuropeanCountry_ShouldReturnFalse()
    {
        // Arrange
        var countryCode = CountryCode.Create("VN");

        // Act
        var result = countryCode.IsEuropean();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsAmerican_WithAmericanCountry_ShouldReturnTrue()
    {
        // Arrange
        var americanCodes = new[] { "US", "CA", "MX", "BR" };

        // Act & Assert
        foreach (var code in americanCodes)
        {
            var countryCode = CountryCode.Create(code);
            countryCode.IsAmerican().Should().BeTrue();
        }
    }

    [Fact]
    public void IsAmerican_WithNonAmericanCountry_ShouldReturnFalse()
    {
        // Arrange
        var countryCode = CountryCode.Create("VN");

        // Act
        var result = countryCode.IsAmerican();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ImplicitOperatorString_ShouldReturnValue()
    {
        // Arrange
        var countryCode = CountryCode.Create("VN");

        // Act
        string value = countryCode;

        // Assert
        value.Should().Be("VN");
    }

    [Fact]
    public void ExplicitOperatorCountryCode_ShouldCreateCountryCode()
    {
        // Arrange
        var code = "VN";

        // Act
        var countryCode = (CountryCode)code;

        // Assert
        countryCode.Value.Should().Be("VN");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var countryCode = CountryCode.Create("VN");

        // Act
        var result = countryCode.ToString();

        // Assert
        result.Should().Be("VN");
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var countryCode1 = CountryCode.Create("VN");
        var countryCode2 = CountryCode.Create("VN");

        // Act & Assert
        countryCode1.Should().Be(countryCode2);
        (countryCode1 == countryCode2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentValue_ShouldNotBeEqual()
    {
        // Arrange
        var countryCode1 = CountryCode.Create("VN");
        var countryCode2 = CountryCode.Create("US");

        // Act & Assert
        countryCode1.Should().NotBe(countryCode2);
        (countryCode1 != countryCode2).Should().BeTrue();
    }
}
