using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;
using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class IPAddressTests
{
    [Fact]
    public void Create_WithValidIPv4_ShouldCreateIPAddress()
    {
        // Arrange
        var value = "192.168.1.1";

        // Act
        var ipAddress = IPAddress.Create(value);

        // Assert
        ipAddress.Should().NotBeNull();
        ipAddress.Value.Should().Be(value);
        ipAddress.Type.Should().Be(IPAddressType.IPv4);
    }

    [Fact]
    public void Create_WithValidIPv6_ShouldCreateIPAddress()
    {
        // Arrange
        var value = "2001:0db8:85a3:0000:0000:8a2e:0370:7334";

        // Act
        var ipAddress = IPAddress.Create(value);

        // Assert
        ipAddress.Should().NotBeNull();
        ipAddress.Value.Should().Be(value);
        ipAddress.Type.Should().Be(IPAddressType.IPv6);
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        var value = "";

        // Act
        Action act = () => IPAddress.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("IP address is required");
    }

    [Fact]
    public void Create_WithWhitespace_ShouldThrowDomainException()
    {
        // Arrange
        var value = "   ";

        // Act
        Action act = () => IPAddress.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("IP address is required");
    }

    [Fact]
    public void Create_WithInvalidFormat_ShouldThrowDomainException()
    {
        // Arrange
        var value = "999.999.999.999";

        // Act
        Action act = () => IPAddress.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid IP address format");
    }

    [Fact]
    public void IsPrivate_WithPrivateIPv4_ShouldReturnTrue()
    {
        // Arrange
        var ipAddress = IPAddress.Create("192.168.1.1");

        // Act
        var result = ipAddress.IsPrivate();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsPrivate_WithPublicIPv4_ShouldReturnFalse()
    {
        // Arrange
        var ipAddress = IPAddress.Create("8.8.8.8");

        // Act
        var result = ipAddress.IsPrivate();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsPrivate_WithIPv6_ShouldReturnFalse()
    {
        // Arrange
        var ipAddress = IPAddress.Create("2001:0db8:85a3::8a2e:0370:7334");

        // Act
        var result = ipAddress.IsPrivate();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsLoopback_WithLoopbackIPv4_ShouldReturnTrue()
    {
        // Arrange
        var ipAddress = IPAddress.Create("127.0.0.1");

        // Act
        var result = ipAddress.IsLoopback();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLoopback_WithLoopbackIPv6_ShouldReturnTrue()
    {
        // Arrange
        var ipAddress = IPAddress.Create("::1");

        // Act
        var result = ipAddress.IsLoopback();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLoopback_WithNonLoopback_ShouldReturnFalse()
    {
        // Arrange
        var ipAddress = IPAddress.Create("192.168.1.1");

        // Act
        var result = ipAddress.IsLoopback();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var ipAddress = IPAddress.Create("192.168.1.1");

        // Act
        string value = ipAddress;

        // Assert
        value.Should().Be("192.168.1.1");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var ipAddress = IPAddress.Create("192.168.1.1");

        // Act
        var result = ipAddress.ToString();

        // Assert
        result.Should().Be("192.168.1.1");
    }
}