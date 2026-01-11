using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class UrlTests
{
    [Fact]
    public void Create_WithValidHttpsUrl_ShouldCreateUrl()
    {
        // Arrange
        string validUrl = "https://www.example.com";

        // Act
        Url url = Url.Create(validUrl);

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
        string validUrl = "http://www.example.com";

        // Act
        Url url = Url.Create(validUrl);

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
        string validUrl = "https://www.example.com/path/to/resource";

        // Act
        Url url = Url.Create(validUrl);

        // Assert
        url.Value.Should().Be(validUrl);
        url.GetDomain().Should().Be("www.example.com");
    }

    [Fact]
    public void Create_WithUrlContainingQueryString_ShouldCreateUrl()
    {
        // Arrange
        string validUrl = "https://www.example.com/search?q=test";

        // Act
        Url url = Url.Create(validUrl);

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
        string emptyUrl = string.Empty;

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
        string whitespaceUrl = "   ";

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
        string invalidUrl = "not-a-url";

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
        string urlWithoutScheme = "www.example.com";

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
        string ftpUrl = "ftp://ftp.example.com";

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
        string urlWithSpaces = "  https://www.example.com  ";

        // Act
        Url url = Url.Create(urlWithSpaces);

        // Assert
        url.Value.Should().Be("https://www.example.com");
    }

    [Fact]
    public void ImplicitOperator_ToString_ShouldReturnValue()
    {
        // Arrange
        Url url = Url.Create("https://www.example.com");

        // Act
        string urlString = url;

        // Assert
        urlString.Should().Be("https://www.example.com");
    }

    [Fact]
    public void ExplicitOperator_FromString_ShouldCreateUrl()
    {
        // Arrange
        string urlString = "https://www.example.com";

        // Act
        Url url = (Url)urlString;

        // Assert
        url.Value.Should().Be(urlString);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        Url url = Url.Create("https://www.example.com");

        // Act
        string result = url.ToString();

        // Assert
        result.Should().Be("https://www.example.com");
    }

    [Fact]
    public void Equality_WithSameUrls_ShouldBeEqual()
    {
        // Arrange
        Url url1 = Url.Create("https://www.example.com");
        Url url2 = Url.Create("https://www.example.com");

        // Act & Assert
        url1.Should().Be(url2);
        url1.GetHashCode().Should().Be(url2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentUrls_ShouldNotBeEqual()
    {
        // Arrange
        Url url1 = Url.Create("https://www.example.com");
        Url url2 = Url.Create("https://www.test.com");

        // Act & Assert
        url1.Should().NotBe(url2);
    }

    [Fact]
    public void GetDomain_WithValidUrl_ShouldReturnDomain()
    {
        // Arrange
        Url url = Url.Create("https://subdomain.example.com/path");

        // Act
        string domain = url.GetDomain();

        // Assert
        domain.Should().Be("subdomain.example.com");
    }

    [Fact]
    public void GetScheme_WithHttpsUrl_ShouldReturnHttps()
    {
        // Arrange
        Url url = Url.Create("https://www.example.com");

        // Act
        string scheme = url.GetScheme();

        // Assert
        scheme.Should().Be("https");
    }

    [Fact]
    public void IsHttps_WithHttpsUrl_ShouldReturnTrue()
    {
        // Arrange
        Url url = Url.Create("https://www.example.com");

        // Act
        bool isHttps = url.IsHttps();

        // Assert
        isHttps.Should().BeTrue();
    }

    [Fact]
    public void IsHttp_WithHttpUrl_ShouldReturnTrue()
    {
        // Arrange
        Url url = Url.Create("http://www.example.com");

        // Act
        bool isHttp = url.IsHttp();

        // Assert
        isHttp.Should().BeTrue();
    }
}
