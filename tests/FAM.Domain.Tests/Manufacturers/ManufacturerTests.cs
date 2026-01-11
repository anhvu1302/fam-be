using FAM.Domain.Common.Base;
using FAM.Domain.Manufacturers;

using FluentAssertions;

namespace FAM.Domain.Tests.Entities.Manufacturers;

public class ManufacturerTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateManufacturer()
    {
        // Arrange
        string name = "Apple Inc.";

        // Act
        Manufacturer manufacturer = Manufacturer.Create(name);

        // Assert
        manufacturer.Name.Should().Be(name);
        manufacturer.IsActive.Should().BeTrue();
        manufacturer.IsApproved.Should().BeTrue();
    }

    [Fact]
    public void Create_WithValidNameAndWebsite_ShouldCreateManufacturer()
    {
        // Arrange
        string name = "Apple Inc.";
        string website = "https://www.apple.com";

        // Act
        Manufacturer manufacturer = Manufacturer.Create(name, website);

        // Assert
        manufacturer.Name.Should().Be(name);
        manufacturer.Website.Should().NotBeNull();
        manufacturer.Website.Should().Be(website);
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowDomainException()
    {
        // Arrange
        string? nullName = null;

        // Act
        Action act = () => Manufacturer.Create(nullName!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Manufacturer name is required");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        string emptyName = string.Empty;

        // Act
        Action act = () => Manufacturer.Create(emptyName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Manufacturer name is required");
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowDomainException()
    {
        // Arrange
        string whitespaceName = "   ";

        // Act
        Action act = () => Manufacturer.Create(whitespaceName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Manufacturer name is required");
    }

    [Fact]
    public void Create_WithInvalidWebsite_ShouldThrowDomainException()
    {
        // Arrange
        string name = "Test Manufacturer";
        string invalidWebsite = "not-a-website";

        // Act
        Action act = () => Manufacturer.Create(name, invalidWebsite);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid URL format");
    }

    [Fact]
    public void UpdateBasicInfo_WithValidData_ShouldUpdateManufacturer()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        string newName = "Updated Manufacturer";
        string shortName = "Updated";
        string legalName = "Updated Manufacturer Inc.";
        string description = "Updated description";

        // Act
        manufacturer.UpdateBasicInfo(newName, shortName, legalName, description);

        // Assert
        manufacturer.Name.Should().Be(newName);
        manufacturer.ShortName.Should().Be(shortName);
        manufacturer.LegalName.Should().Be(legalName);
        manufacturer.Description.Should().Be(description);
    }

    [Fact]
    public void UpdateBasicInfo_WithNullName_ShouldThrowDomainException()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        string? nullName = null;

        // Act
        Action act = () => manufacturer.UpdateBasicInfo(nullName!, null, null, null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Manufacturer name is required");
    }

    [Fact]
    public void UpdateBrand_WithValidLogoUrl_ShouldUpdateBrand()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        string brandName = "Test Brand";
        string logoUrl = "https://example.com/logo.png";
        string tagline = "Test Tagline";

        // Act
        manufacturer.UpdateBrand(brandName, logoUrl, tagline);

        // Assert
        manufacturer.BrandName.Should().Be(brandName);
        manufacturer.LogoUrl.Should().NotBeNull();
        manufacturer.LogoUrl.Should().Be(logoUrl);
        manufacturer.Tagline.Should().Be(tagline);
    }

    [Fact]
    public void UpdateBrand_WithInvalidLogoUrl_ShouldThrowDomainException()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        string invalidLogoUrl = "not-a-url";

        // Act
        Action act = () => manufacturer.UpdateBrand("Brand", invalidLogoUrl, "Tagline");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid URL format");
    }

    [Fact]
    public void UpdateContactInfo_WithValidWebsite_ShouldUpdateContactInfo()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        string website = "https://www.example.com";
        string email = "contact@example.com";
        string phone = "+1-555-0123";
        string fax = "+1-555-0124";

        // Act
        manufacturer.UpdateContactInfo(website, email, phone, fax);

        // Assert
        manufacturer.Website.Should().NotBeNull();
        manufacturer.Website.Should().Be(website);
        manufacturer.Email.Should().Be(email);
        manufacturer.Phone.Should().Be(phone);
        manufacturer.Fax.Should().Be(fax);
    }

    [Fact]
    public void UpdateSupport_WithValidSupportWebsite_ShouldUpdateSupport()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        string supportEmail = "support@example.com";
        string supportPhone = "+1-555-0199";
        string supportWebsite = "https://support.example.com";
        string supportHours = "24/7";

        // Act
        manufacturer.UpdateSupport(supportEmail, supportPhone, supportWebsite, supportHours);

        // Assert
        manufacturer.SupportEmail.Should().Be(supportEmail);
        manufacturer.SupportPhone.Should().Be(supportPhone);
        manufacturer.SupportWebsite.Should().NotBeNull();
        manufacturer.SupportWebsite.Should().Be(supportWebsite);
        manufacturer.SupportHours.Should().Be(supportHours);
    }

    [Fact]
    public void UpdateSocialMedia_WithValidUrls_ShouldUpdateSocialMedia()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        string linkedIn = "https://linkedin.com/company/example";
        string twitter = "@example";
        string facebook = "https://facebook.com/example";

        // Act
        manufacturer.UpdateSocialMedia(linkedIn, twitter, facebook);

        // Assert
        manufacturer.LinkedInUrl.Should().NotBeNull();
        manufacturer.LinkedInUrl.Should().Be(linkedIn);
        manufacturer.TwitterHandle.Should().Be(twitter);
        manufacturer.FacebookUrl.Should().NotBeNull();
        manufacturer.FacebookUrl.Should().Be(facebook);
    }

    [Fact]
    public void UpdateBusinessInfo_WithNegativeEmployeeCount_ShouldThrowDomainException()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        int negativeEmployeeCount = -10;

        // Act
        Action act = () => manufacturer.UpdateBusinessInfo("Tech", "Private", null, negativeEmployeeCount);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Employee count cannot be negative");
    }

    [Fact]
    public void UpdateFinancialInfo_WithNegativeAnnualRevenue_ShouldThrowDomainException()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        decimal negativeRevenue = -1000000m;

        // Act
        Action act = () => manufacturer.UpdateFinancialInfo(negativeRevenue, "USD", "Net 30", "USD", 10m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Annual revenue cannot be negative");
    }

    [Fact]
    public void UpdateFinancialInfo_WithDiscountRateOver100_ShouldThrowDomainException()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        decimal discountRateOver100 = 150m;

        // Act
        Action act = () => manufacturer.UpdateFinancialInfo(1000000m, "USD", "Net 30", "USD", discountRateOver100);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Discount rate must be between 0 and 100");
    }

    [Fact]
    public void UpdateFinancialInfo_WithNegativeDiscountRate_ShouldThrowDomainException()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        decimal negativeDiscountRate = -5m;

        // Act
        Action act = () => manufacturer.UpdateFinancialInfo(1000000m, "USD", "Net 30", "USD", negativeDiscountRate);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Discount rate must be between 0 and 100");
    }

    [Fact]
    public void UpdateWarrantySupport_WithNegativeWarrantyMonths_ShouldThrowDomainException()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        int negativeWarrantyMonths = -12;

        // Act
        Action act = () =>
            manufacturer.UpdateWarrantySupport("Standard Warranty", negativeWarrantyMonths, "https://example.com/sla");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Standard warranty months cannot be negative");
    }

    [Fact]
    public void UpdateRatings_WithRatingBelow1_ShouldThrowDomainException()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        decimal ratingBelow1 = 0.5m;

        // Act
        Action act = () => manufacturer.UpdateRatings(ratingBelow1, 4m, 3m, "Good manufacturer");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Rating must be between 1 and 5");
    }

    [Fact]
    public void UpdateRatings_WithRatingAbove5_ShouldThrowDomainException()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        decimal ratingAbove5 = 6m;

        // Act
        Action act = () => manufacturer.UpdateRatings(4m, ratingAbove5, 3m, "Good manufacturer");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Rating must be between 1 and 5");
    }

    [Fact]
    public void GetAverageRating_WithNoRatings_ShouldReturnZero()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");

        // Act
        decimal averageRating = manufacturer.GetAverageRating();

        // Assert
        averageRating.Should().Be(0);
    }

    [Fact]
    public void GetAverageRating_WithMultipleRatings_ShouldReturnCorrectAverage()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        manufacturer.UpdateRatings(4m, 3m, 5m, "Good manufacturer");

        // Act
        decimal averageRating = manufacturer.GetAverageRating();

        // Assert
        averageRating.Should().Be(4m);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        manufacturer.Deactivate();

        // Act
        manufacturer.Activate();

        // Assert
        manufacturer.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");

        // Act
        manufacturer.Deactivate();

        // Assert
        manufacturer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Approve_ShouldSetIsApprovedToTrue()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        manufacturer.Reject();

        // Act
        manufacturer.Approve();

        // Assert
        manufacturer.IsApproved.Should().BeTrue();
    }

    [Fact]
    public void Reject_ShouldSetIsApprovedToFalse()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");

        // Act
        manufacturer.Reject();

        // Assert
        manufacturer.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void SetAsPreferred_ShouldSetIsPreferredToTrue()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");

        // Act
        manufacturer.SetAsPreferred();

        // Assert
        manufacturer.IsPreferred.Should().BeTrue();
    }

    [Fact]
    public void RemovePreferred_ShouldSetIsPreferredToFalse()
    {
        // Arrange
        Manufacturer manufacturer = Manufacturer.Create("Test Manufacturer");
        manufacturer.SetAsPreferred();

        // Act
        manufacturer.RemovePreferred();

        // Assert
        manufacturer.IsPreferred.Should().BeFalse();
    }
}
