using FluentAssertions;
using FAM.Domain.ValueObjects;
using FAM.Domain.Common;

namespace FAM.Domain.Tests.ValueObjects;

public class MACAddressTests
{
    [Fact]
    public void Create_WithColonSeparatedFormat_ShouldCreateMACAddress()
    {
        // Arrange
        var value = "00:11:22:33:44:55";

        // Act
        var macAddress = MACAddress.Create(value);

        // Assert
        macAddress.Should().NotBeNull();
        macAddress.Value.Should().Be(value.ToUpper());
        macAddress.Format.Should().Be(MACAddressFormat.ColonSeparated);
    }

    [Fact]
    public void Create_WithHyphenSeparatedFormat_ShouldCreateMACAddress()
    {
        // Arrange
        var value = "00-11-22-33-44-55";

        // Act
        var macAddress = MACAddress.Create(value);

        // Assert
        macAddress.Should().NotBeNull();
        macAddress.Value.Should().Be(value.ToUpper());
        macAddress.Format.Should().Be(MACAddressFormat.HyphenSeparated);
    }

    [Fact]
    public void Create_WithDotSeparatedFormat_ShouldCreateMACAddress()
    {
        // Arrange
        var value = "0011.2233.4455";

        // Act
        var macAddress = MACAddress.Create(value);

        // Assert
        macAddress.Should().NotBeNull();
        macAddress.Value.Should().Be(value.ToUpper());
        macAddress.Format.Should().Be(MACAddressFormat.DotSeparated);
    }

    [Fact]
    public void Create_WithNoSeparatorsFormat_ShouldCreateMACAddress()
    {
        // Arrange
        var value = "001122334455";

        // Act
        var macAddress = MACAddress.Create(value);

        // Assert
        macAddress.Should().NotBeNull();
        macAddress.Value.Should().Be(value.ToUpper());
        macAddress.Format.Should().Be(MACAddressFormat.NoSeparators);
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        var value = "";

        // Act
        Action act = () => MACAddress.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("MAC address is required");
    }

    [Fact]
    public void Create_WithInvalidLength_ShouldThrowDomainException()
    {
        // Arrange
        var value = "00:11:22:33:44";

        // Act
        Action act = () => MACAddress.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"Invalid MAC address format: {value.ToUpper()}");
    }

    [Fact]
    public void Create_WithInvalidCharacters_ShouldThrowDomainException()
    {
        // Arrange
        var value = "ZZ:11:22:33:44:55";

        // Act
        Action act = () => MACAddress.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage($"Invalid MAC address format: {value.ToUpper()}");
    }

    [Fact]
    public void IsMulticast_WithMulticastAddress_ShouldReturnTrue()
    {
        // Arrange
        var macAddress = MACAddress.Create("01:11:22:33:44:55");

        // Act
        var result = macAddress.IsMulticast();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsMulticast_WithUnicastAddress_ShouldReturnFalse()
    {
        // Arrange
        var macAddress = MACAddress.Create("00:11:22:33:44:55");

        // Act
        var result = macAddress.IsMulticast();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsUnicast_WithUnicastAddress_ShouldReturnTrue()
    {
        // Arrange
        var macAddress = MACAddress.Create("00:11:22:33:44:55");

        // Act
        var result = macAddress.IsUnicast();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLocallyAdministered_WithLocallyAdministeredAddress_ShouldReturnTrue()
    {
        // Arrange
        var macAddress = MACAddress.Create("02:11:22:33:44:55");

        // Act
        var result = macAddress.IsLocallyAdministered();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void GetNormalizedFormat_ShouldReturnColonSeparatedFormat()
    {
        // Arrange
        var macAddress = MACAddress.Create("001122334455");

        // Act
        var result = macAddress.GetNormalizedFormat();

        // Assert
        result.Should().Be("00:11:22:33:44:55");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var macAddress = MACAddress.Create("00:11:22:33:44:55");

        // Act
        string value = macAddress;

        // Assert
        value.Should().Be("00:11:22:33:44:55");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var macAddress = MACAddress.Create("00:11:22:33:44:55");

        // Act
        var result = macAddress.ToString();

        // Assert
        result.Should().Be("00:11:22:33:44:55");
    }
}