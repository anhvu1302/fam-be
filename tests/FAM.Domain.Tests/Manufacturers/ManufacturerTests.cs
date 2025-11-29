using FAM.Domain.Manufacturers;
using FAM.Domain.Common;
using FAM.Domain.ValueObjects;
using FluentAssertions;

namespace FAM.Domain.Tests.Entities.Manufacturers;

public class ManufacturerTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateManufacturer()
    {
        // Arrange
        var name = "Apple Inc.";

        // Act
        var manufacturer = Manufacturer.Create(name);

        // Assert
        manufacturer.Name.Should().Be(name);
        manufacturer.IsActive.Should().BeTrue();
        manufacturer.IsApproved.Should().BeTrue();
    }

    [Fact]
    public void Create_WithValidNameAndWebsite_ShouldCreateManufacturer()
    {
        // Arrange
        var name = "Apple Inc.";
        var website = "https://www.apple.com";

        // Act
        var manufacturer = Manufacturer.Create(name, website);

        // Assert
        manufacturer.Name.Should().Be(name);
        manufacturer.Website.Should().NotBeNull();
        manufacturer.Website!.Value.Should().Be(website);
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
        var emptyName = string.Empty;

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
        var whitespaceName = "   ";

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
        var name = "Test Manufacturer";
        var invalidWebsite = "not-a-website";

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
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        var newName = "Updated Manufacturer";
        var shortName = "Updated";
        var legalName = "Updated Manufacturer Inc.";
        var description = "Updated description";

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
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        string? nullName = null;

        // Act
        var act = () => manufacturer.UpdateBasicInfo(nullName!, null, null, null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Manufacturer name is required");
    }

    [Fact]
    public void UpdateBrand_WithValidLogoUrl_ShouldUpdateBrand()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        var brandName = "Test Brand";
        var logoUrl = "https://example.com/logo.png";
        var tagline = "Test Tagline";

        // Act
        manufacturer.UpdateBrand(brandName, logoUrl, tagline);

        // Assert
        manufacturer.BrandName.Should().Be(brandName);
        manufacturer.LogoUrl.Should().NotBeNull();
        manufacturer.LogoUrl!.Value.Should().Be(logoUrl);
        manufacturer.Tagline.Should().Be(tagline);
    }

    [Fact]
    public void UpdateBrand_WithInvalidLogoUrl_ShouldThrowDomainException()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        var invalidLogoUrl = "not-a-url";

        // Act
        var act = () => manufacturer.UpdateBrand("Brand", invalidLogoUrl, "Tagline");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid URL format");
    }

    [Fact]
    public void UpdateContactInfo_WithValidWebsite_ShouldUpdateContactInfo()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        var website = "https://www.example.com";
        var email = "contact@example.com";
        var phone = "+1-555-0123";
        var fax = "+1-555-0124";

        // Act
        manufacturer.UpdateContactInfo(website, email, phone, fax);

        // Assert
        manufacturer.Website.Should().NotBeNull();
        manufacturer.Website!.Value.Should().Be(website);
        manufacturer.Email.Should().Be(email);
        manufacturer.Phone.Should().Be(phone);
        manufacturer.Fax.Should().Be(fax);
    }

    [Fact]
    public void UpdateSupport_WithValidSupportWebsite_ShouldUpdateSupport()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        var supportEmail = "support@example.com";
        var supportPhone = "+1-555-0199";
        var supportWebsite = "https://support.example.com";
        var supportHours = "24/7";

        // Act
        manufacturer.UpdateSupport(supportEmail, supportPhone, supportWebsite, supportHours);

        // Assert
        manufacturer.SupportEmail.Should().Be(supportEmail);
        manufacturer.SupportPhone.Should().Be(supportPhone);
        manufacturer.SupportWebsite.Should().NotBeNull();
        manufacturer.SupportWebsite!.Value.Should().Be(supportWebsite);
        manufacturer.SupportHours.Should().Be(supportHours);
    }

    [Fact]
    public void UpdateSocialMedia_WithValidUrls_ShouldUpdateSocialMedia()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        var linkedIn = "https://linkedin.com/company/example";
        var twitter = "@example";
        var facebook = "https://facebook.com/example";

        // Act
        manufacturer.UpdateSocialMedia(linkedIn, twitter, facebook);

        // Assert
        manufacturer.LinkedInUrl.Should().NotBeNull();
        manufacturer.LinkedInUrl!.Value.Should().Be(linkedIn);
        manufacturer.TwitterHandle.Should().Be(twitter);
        manufacturer.FacebookUrl.Should().NotBeNull();
        manufacturer.FacebookUrl!.Value.Should().Be(facebook);
    }

    [Fact]
    public void UpdateBusinessInfo_WithNegativeEmployeeCount_ShouldThrowDomainException()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        var negativeEmployeeCount = -10;

        // Act
        var act = () => manufacturer.UpdateBusinessInfo("Tech", "Private", null, negativeEmployeeCount);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Employee count cannot be negative");
    }

    [Fact]
    public void UpdateFinancialInfo_WithNegativeAnnualRevenue_ShouldThrowDomainException()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        var negativeRevenue = -1000000m;

        // Act
        var act = () => manufacturer.UpdateFinancialInfo(negativeRevenue, "USD", "Net 30", "USD", 10m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Annual revenue cannot be negative");
    }

    [Fact]
    public void UpdateFinancialInfo_WithDiscountRateOver100_ShouldThrowDomainException()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        var discountRateOver100 = 150m;

        // Act
        var act = () => manufacturer.UpdateFinancialInfo(1000000m, "USD", "Net 30", "USD", discountRateOver100);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Discount rate must be between 0 and 100");
    }

    [Fact]
    public void UpdateFinancialInfo_WithNegativeDiscountRate_ShouldThrowDomainException()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        var negativeDiscountRate = -5m;

        // Act
        var act = () => manufacturer.UpdateFinancialInfo(1000000m, "USD", "Net 30", "USD", negativeDiscountRate);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Discount rate must be between 0 and 100");
    }

    [Fact]
    public void UpdateWarrantySupport_WithNegativeWarrantyMonths_ShouldThrowDomainException()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        var negativeWarrantyMonths = -12;

        // Act
        var act = () =>
            manufacturer.UpdateWarrantySupport("Standard Warranty", negativeWarrantyMonths, "https://example.com/sla");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Standard warranty months cannot be negative");
    }

    [Fact]
    public void UpdateRatings_WithRatingBelow1_ShouldThrowDomainException()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        var ratingBelow1 = 0.5m;

        // Act
        var act = () => manufacturer.UpdateRatings(ratingBelow1, 4m, 3m, "Good manufacturer");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Rating must be between 1 and 5");
    }

    [Fact]
    public void UpdateRatings_WithRatingAbove5_ShouldThrowDomainException()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        var ratingAbove5 = 6m;

        // Act
        var act = () => manufacturer.UpdateRatings(4m, ratingAbove5, 3m, "Good manufacturer");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Rating must be between 1 and 5");
    }

    [Fact]
    public void GetAverageRating_WithNoRatings_ShouldReturnZero()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");

        // Act
        var averageRating = manufacturer.GetAverageRating();

        // Assert
        averageRating.Should().Be(0);
    }

    [Fact]
    public void GetAverageRating_WithMultipleRatings_ShouldReturnCorrectAverage()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        manufacturer.UpdateRatings(4m, 3m, 5m, "Good manufacturer");

        // Act
        var averageRating = manufacturer.GetAverageRating();

        // Assert
        averageRating.Should().Be(4m);
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
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
        var manufacturer = Manufacturer.Create("Test Manufacturer");

        // Act
        manufacturer.Deactivate();

        // Assert
        manufacturer.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Approve_ShouldSetIsApprovedToTrue()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
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
        var manufacturer = Manufacturer.Create("Test Manufacturer");

        // Act
        manufacturer.Reject();

        // Assert
        manufacturer.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void SetAsPreferred_ShouldSetIsPreferredToTrue()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");

        // Act
        manufacturer.SetAsPreferred();

        // Assert
        manufacturer.IsPreferred.Should().BeTrue();
    }

    [Fact]
    public void RemovePreferred_ShouldSetIsPreferredToFalse()
    {
        // Arrange
        var manufacturer = Manufacturer.Create("Test Manufacturer");
        manufacturer.SetAsPreferred();

        // Act
        manufacturer.RemovePreferred();

        // Assert
        manufacturer.IsPreferred.Should().BeFalse();
    }
}