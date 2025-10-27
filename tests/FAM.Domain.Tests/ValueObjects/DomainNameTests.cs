using FluentAssertions;
using FAM.Domain.ValueObjects;
using FAM.Domain.Common;

namespace FAM.Domain.Tests.ValueObjects;

public class DomainNameTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateDomainName()
    {
        // Arrange
        var value = "example.com";

        // Act
        var domainName = DomainName.Create(value);

        // Assert
        domainName.Should().NotBeNull();
        domainName.Value.Should().Be(value.ToLowerInvariant());
    }

    [Fact]
    public void Create_WithEmptyString_ShouldThrowDomainException()
    {
        // Arrange
        var value = "";

        // Act
        Action act = () => DomainName.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Domain cannot be empty");
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldThrowDomainException()
    {
        // Arrange
        var value = new string('a', 254) + ".com";

        // Act
        Action act = () => DomainName.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Domain cannot exceed 253 characters");
    }

    [Fact]
    public void Create_WithInvalidFormat_ShouldThrowDomainException()
    {
        // Arrange
        var value = "invalid..domain";

        // Act
        Action act = () => DomainName.Create(value);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid domain format");
    }

    [Fact]
    public void Create_WithUppercase_ShouldConvertToLowercase()
    {
        // Arrange
        var value = "EXAMPLE.COM";

        // Act
        var domainName = DomainName.Create(value);

        // Assert
        domainName.Value.Should().Be("example.com");
    }

    [Fact]
    public void Create_WithValidSubdomain_ShouldCreateDomainName()
    {
        // Arrange
        var value = "sub.example.com";

        // Act
        var domainName = DomainName.Create(value);

        // Assert
        domainName.Value.Should().Be("sub.example.com");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var domainName = DomainName.Create("example.com");

        // Act
        string value = domainName;

        // Assert
        value.Should().Be("example.com");
    }

    [Fact]
    public void ExplicitOperator_ShouldConvertFromString()
    {
        // Arrange
        var value = "example.com";

        // Act
        DomainName domainName = (DomainName)value;

        // Assert
        domainName.Value.Should().Be("example.com");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var domainName = DomainName.Create("example.com");

        // Act
        var result = domainName.ToString();

        // Assert
        result.Should().Be("example.com");
    }
}