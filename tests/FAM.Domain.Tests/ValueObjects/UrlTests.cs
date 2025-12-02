using FAM.Domain.Common;
using FAM.Domain.ValueObjects;
using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class UrlTests
{
    [Fact]
    public void Create_WithValidHttpsUrl_ShouldCreateUrl()
    {
        // Arrange
        var validUrl = "https://www.example.com";

        // Act
        var url = Url.Create(validUrl);

        // Assert
        url.Value.Should().Be(validUrl);
        url.IsHttps().Should().BeTrue();
        url.GetDomain().Should().Be("www.example.com");
        url.GetScheme().Should().Be("https");
    }

    [Fact]
    public void Create_WithValidHttpUrl_ShouldCreateUrl()
    {
        // Arrange
        var validUrl = "http://www.example.com";

        // Act
        var url = Url.Create(validUrl);

        // Assert
        url.Value.Should().Be(validUrl);
        url.IsHttp().Should().BeTrue();
        url.GetDomain().Should().Be("www.example.com");
        url.GetScheme().Should().Be("http");
    }

    [Fact]
    public void Create_WithUrlContainingPath_ShouldCreateUrl()
    {
        // Arrange
        var validUrl = "https://www.example.com/path/to/resource";

        // Act
        var url = Url.Create(validUrl);

        // Assert
        url.Value.Should().Be(validUrl);
        url.GetDomain().Should().Be("www.example.com");
    }

    [Fact]
    public void Create_WithUrlContainingQueryString_ShouldCreateUrl()
    {
        // Arrange
        var validUrl = "https://www.example.com/search?q=test";

        // Act
        var url = Url.Create(validUrl);

        // Assert
        url.Value.Should().Be(validUrl);
        url.GetDomain().Should().Be("www.example.com");
    }

    [Fact]
    public void Create_WithNullUrl_ShouldThrowDomainException()
    {
        // Arrange
        string? nullUrl = null;

        // Act
        Action act = () => Url.Create(nullUrl!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("URL cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyUrl_ShouldThrowDomainException()
    {
        // Arrange
        var emptyUrl = string.Empty;

        // Act
        Action act = () => Url.Create(emptyUrl);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("URL cannot be empty");
    }

    [Fact]
    public void Create_WithWhitespaceUrl_ShouldThrowDomainException()
    {
        // Arrange
        var whitespaceUrl = "   ";

        // Act
        Action act = () => Url.Create(whitespaceUrl);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("URL cannot be empty");
    }

    [Fact]
    public void Create_WithInvalidUrlFormat_ShouldThrowDomainException()
    {
        // Arrange
        var invalidUrl = "not-a-url";

        // Act
        Action act = () => Url.Create(invalidUrl);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid URL format");
    }

    [Fact]
    public void Create_WithUrlWithoutScheme_ShouldThrowDomainException()
    {
        // Arrange
        var urlWithoutScheme = "www.example.com";

        // Act
        Action act = () => Url.Create(urlWithoutScheme);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid URL format");
    }

    [Fact]
    public void Create_WithFtpUrl_ShouldThrowDomainException()
    {
        // Arrange
        var ftpUrl = "ftp://ftp.example.com";

        // Act
        Action act = () => Url.Create(ftpUrl);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid URL format");
    }

    [Fact]
    public void Create_WithUrlContainingSpaces_ShouldTrimAndValidate()
    {
        // Arrange
        var urlWithSpaces = "  https://www.example.com  ";

        // Act
        var url = Url.Create(urlWithSpaces);

        // Assert
        url.Value.Should().Be("https://www.example.com");
    }

    [Fact]
    public void ImplicitOperator_ToString_ShouldReturnValue()
    {
        // Arrange
        var url = Url.Create("https://www.example.com");

        // Act
        string urlString = url;

        // Assert
        urlString.Should().Be("https://www.example.com");
    }

    [Fact]
    public void ExplicitOperator_FromString_ShouldCreateUrl()
    {
        // Arrange
        var urlString = "https://www.example.com";

        // Act
        var url = (Url)urlString;

        // Assert
        url.Value.Should().Be(urlString);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var url = Url.Create("https://www.example.com");

        // Act
        var result = url.ToString();

        // Assert
        result.Should().Be("https://www.example.com");
    }

    [Fact]
    public void Equality_WithSameUrls_ShouldBeEqual()
    {
        // Arrange
        var url1 = Url.Create("https://www.example.com");
        var url2 = Url.Create("https://www.example.com");

        // Act & Assert
        url1.Should().Be(url2);
        url1.GetHashCode().Should().Be(url2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentUrls_ShouldNotBeEqual()
    {
        // Arrange
        var url1 = Url.Create("https://www.example.com");
        var url2 = Url.Create("https://www.test.com");

        // Act & Assert
        url1.Should().NotBe(url2);
    }

    [Fact]
    public void GetDomain_WithValidUrl_ShouldReturnDomain()
    {
        // Arrange
        var url = Url.Create("https://subdomain.example.com/path");

        // Act
        var domain = url.GetDomain();

        // Assert
        domain.Should().Be("subdomain.example.com");
    }

    [Fact]
    public void GetScheme_WithHttpsUrl_ShouldReturnHttps()
    {
        // Arrange
        var url = Url.Create("https://www.example.com");

        // Act
        var scheme = url.GetScheme();

        // Assert
        scheme.Should().Be("https");
    }

    [Fact]
    public void IsHttps_WithHttpsUrl_ShouldReturnTrue()
    {
        // Arrange
        var url = Url.Create("https://www.example.com");

        // Act
        var isHttps = url.IsHttps();

        // Assert
        isHttps.Should().BeTrue();
    }

    [Fact]
    public void IsHttp_WithHttpUrl_ShouldReturnTrue()
    {
        // Arrange
        var url = Url.Create("http://www.example.com");

        // Act
        var isHttp = url.IsHttp();

        // Assert
        isHttp.Should().BeTrue();
    }
}