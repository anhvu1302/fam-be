using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class PhoneNumberTests
{
    [Fact]
    public void Create_WithValidPhoneNumber_ShouldCreatePhoneNumber()
    {
        // Arrange
        var phoneNumber = "0912345678";
        var countryCode = "+84";

        // Act
        var phone = PhoneNumber.Create(phoneNumber, countryCode);

        // Assert
        phone.Should().NotBeNull();
        phone.Value.Should().Be("0912345678");
        phone.CountryCode.Should().Be(countryCode);
    }

    [Fact]
    public void Create_WithPhoneNumberWithSpaces_ShouldCleanAndCreate()
    {
        // Arrange
        var phoneNumber = "091 234 5678";
        var countryCode = "+84";

        // Act
        var phone = PhoneNumber.Create(phoneNumber, countryCode);

        // Assert
        phone.Value.Should().Be("0912345678");
    }

    [Fact]
    public void Create_WithPhoneNumberWithDashes_ShouldCleanAndCreate()
    {
        // Arrange
        var phoneNumber = "091-234-5678";
        var countryCode = "+84";

        // Act
        var phone = PhoneNumber.Create(phoneNumber, countryCode);

        // Assert
        phone.Value.Should().Be("0912345678");
    }

    [Fact]
    public void Create_WithPhoneNumberWithParentheses_ShouldCleanAndCreate()
    {
        // Arrange
        var phoneNumber = "(091)234-5678";
        var countryCode = "+84";

        // Act
        var phone = PhoneNumber.Create(phoneNumber, countryCode);

        // Assert
        phone.Value.Should().Be("0912345678");
    }

    [Fact]
    public void Create_WithNullPhoneNumber_ShouldThrowDomainException()
    {
        // Arrange
        string? phoneNumber = null;
        var countryCode = "+84";

        // Act
        Action act = () => PhoneNumber.Create(phoneNumber!, countryCode);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Phone number is required");
    }

    [Fact]
    public void Create_WithEmptyPhoneNumber_ShouldThrowDomainException()
    {
        // Arrange
        var phoneNumber = string.Empty;
        var countryCode = "+84";

        // Act
        Action act = () => PhoneNumber.Create(phoneNumber, countryCode);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Phone number is required");
    }

    [Fact]
    public void Create_WithWhitespacePhoneNumber_ShouldThrowDomainException()
    {
        // Arrange
        var phoneNumber = "   ";
        var countryCode = "+84";

        // Act
        Action act = () => PhoneNumber.Create(phoneNumber, countryCode);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Phone number is required");
    }

    [Fact]
    public void Create_WithTooShortPhoneNumber_ShouldThrowDomainException()
    {
        // Arrange
        var phoneNumber = "12345678"; // 8 digits
        var countryCode = "+84";

        // Act
        Action act = () => PhoneNumber.Create(phoneNumber, countryCode);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid phone number length");
    }

    [Fact]
    public void Create_WithTooLongPhoneNumber_ShouldThrowDomainException()
    {
        // Arrange
        var phoneNumber = "1234567890123456"; // 16 digits
        var countryCode = "+84";

        // Act
        Action act = () => PhoneNumber.Create(phoneNumber, countryCode);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid phone number length");
    }

    [Fact]
    public void Create_WithMinimumValidLength_ShouldCreatePhoneNumber()
    {
        // Arrange
        var phoneNumber = "123456789"; // 9 digits
        var countryCode = "+84";

        // Act
        var phone = PhoneNumber.Create(phoneNumber, countryCode);

        // Assert
        phone.Value.Should().Be(phoneNumber);
    }

    [Fact]
    public void Create_WithMaximumValidLength_ShouldCreatePhoneNumber()
    {
        // Arrange
        var phoneNumber = "123456789012345"; // 15 digits
        var countryCode = "+84";

        // Act
        var phone = PhoneNumber.Create(phoneNumber, countryCode);

        // Assert
        phone.Value.Should().Be(phoneNumber);
    }

    [Fact]
    public void Create_WithDefaultCountryCode_ShouldUseDefault()
    {
        // Arrange
        var phoneNumber = "0912345678";

        // Act
        var phone = PhoneNumber.Create(phoneNumber);

        // Assert
        phone.CountryCode.Should().Be("+84");
    }

    [Fact]
    public void Create_WithNullCountryCode_ShouldUseDefault()
    {
        // Arrange
        var phoneNumber = "0912345678";

        // Act
        var phone = PhoneNumber.Create(phoneNumber, null);

        // Assert
        phone.CountryCode.Should().Be("+84");
    }

    [Fact]
    public void Equality_WithSamePhoneNumbers_ShouldBeEqual()
    {
        // Arrange
        var phone1 = PhoneNumber.Create("0912345678", "+84");
        var phone2 = PhoneNumber.Create("0912345678", "+84");

        // Act & Assert
        phone1.Should().Be(phone2);
    }

    [Fact]
    public void Equality_WithDifferentPhoneNumbers_ShouldNotBeEqual()
    {
        // Arrange
        var phone1 = PhoneNumber.Create("0912345678", "+84");
        var phone2 = PhoneNumber.Create("0912345679", "+84");

        // Act & Assert
        phone1.Should().NotBe(phone2);
    }

    [Fact]
    public void Equality_WithDifferentCountryCodes_ShouldNotBeEqual()
    {
        // Arrange
        var phone1 = PhoneNumber.Create("0912345678", "+84");
        var phone2 = PhoneNumber.Create("0912345678", "+1");

        // Act & Assert
        phone1.Should().NotBe(phone2);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var phone = PhoneNumber.Create("0912345678", "+84");

        // Act
        var result = phone.ToString();

        // Assert
        result.Should().Be("+840912345678");
    }

    [Fact]
    public void ToString_WithNullCountryCode_ShouldReturnValueOnly()
    {
        // Arrange
        var phone = PhoneNumber.Create("0912345678", null);

        // Act
        var result = phone.ToString();

        // Assert
        result.Should().Be("+840912345678");
    }

    [Fact]
    public void ToFormattedString_WithVietnamesePhone_ShouldFormatCorrectly()
    {
        // Arrange
        var phone = PhoneNumber.Create("0912345678");

        // Act
        var result = phone.ToFormattedString();

        // Assert
        result.Should().Be("0912 345 678");
    }

    [Fact]
    public void ToFormattedString_WithNonVietnamesePhone_ShouldReturnUnformatted()
    {
        // Arrange
        var phone = PhoneNumber.Create("1234567890");

        // Act
        var result = phone.ToFormattedString();

        // Assert
        result.Should().Be("1234567890");
    }

    [Fact]
    public void ToFormattedString_WithShortPhone_ShouldReturnUnformatted()
    {
        // Arrange
        var phone = PhoneNumber.Create("091234567"); // 9 digits, doesn't start with 0

        // Act
        var result = phone.ToFormattedString();

        // Assert
        result.Should().Be("091234567");
    }
}
