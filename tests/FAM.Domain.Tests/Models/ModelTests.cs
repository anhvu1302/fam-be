using FAM.Domain.Common.Base;
using FAM.Domain.Models;

using FluentAssertions;

namespace FAM.Domain.Tests.Entities.Models;

public class ModelTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateModel()
    {
        // Arrange
        string name = "MacBook Pro 16-inch";

        // Act
        Model model = Model.Create(name);

        // Assert
        model.Name.Should().Be(name);
        model.IsActive.Should().BeTrue();
        model.IsDepreciable.Should().BeTrue();
        model.IsTangible.Should().BeTrue();
        model.RequiresMaintenance.Should().BeTrue();
    }

    [Fact]
    public void Create_WithAllParameters_ShouldCreateModel()
    {
        // Arrange
        string name = "MacBook Pro 16-inch";
        int manufacturerId = 1;
        int categoryId = 2;
        string modelNumber = "MBP16-2023";

        // Act
        Model model = Model.Create(name, manufacturerId, categoryId, modelNumber);

        // Assert
        model.Name.Should().Be(name);
        model.ManufacturerId.Should().Be(manufacturerId);
        model.CategoryId.Should().Be(categoryId);
        model.ModelNumber.Should().Be(modelNumber);
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowDomainException()
    {
        // Arrange
        string? nullName = null;

        // Act
        Action act = () => Model.Create(nullName!);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Model name is required");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        string emptyName = string.Empty;

        // Act
        Action act = () => Model.Create(emptyName);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Model name is required");
    }

    [Fact]
    public void UpdateBasicInfo_WithValidData_ShouldUpdateModel()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        string newName = "Updated Model";
        string modelNumber = "UPD-001";
        string sku = "SKU001";
        string partNumber = "PN001";
        string description = "Updated description";

        // Act
        model.UpdateBasicInfo(newName, modelNumber, sku, partNumber, description);

        // Assert
        model.Name.Should().Be(newName);
        model.ModelNumber.Should().Be(modelNumber);
        model.SKU.Should().Be(sku);
        model.PartNumber.Should().Be(partNumber);
        model.Description.Should().Be(description);
    }

    [Fact]
    public void UpdateBasicInfo_WithNullName_ShouldThrowDomainException()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        string? nullName = null;

        // Act
        Action act = () => model.UpdateBasicInfo(nullName!, null, null, null, null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Model name is required");
    }

    [Fact]
    public void UpdatePhysicalSpecs_WithNegativeWeight_ShouldThrowDomainException()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        decimal negativeWeight = -5.5m;

        // Act
        Action act = () => model.UpdatePhysicalSpecs(negativeWeight, "kg", "30x20x5", "cm", "Black", "Plastic");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Weight cannot be negative");
    }

    [Fact]
    public void UpdatePowerEnvironmental_WithNegativePowerConsumption_ShouldThrowDomainException()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        decimal negativePowerConsumption = -100m;

        // Act
        Action act = () =>
            model.UpdatePowerEnvironmental("100-240V", negativePowerConsumption, "Energy Star", "0-40°C", "10-90%");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Power consumption cannot be negative");
    }

    [Fact]
    public void UpdatePricing_WithNegativeMSRP_ShouldThrowDomainException()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        decimal negativeMSRP = -1999.99m;

        // Act
        Action act = () => model.UpdatePricing(negativeMSRP, "USD", 1500m, "USD");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("MSRP cannot be negative");
    }

    [Fact]
    public void UpdatePricing_WithNegativeAverageCost_ShouldThrowDomainException()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        decimal negativeAverageCost = -1200m;

        // Act
        Action act = () => model.UpdatePricing(1999.99m, "USD", negativeAverageCost, "USD");

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Average cost cannot be negative");
    }

    [Fact]
    public void UpdateDepreciation_WithNegativeUsefulLifeMonths_ShouldThrowDomainException()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        int negativeUsefulLifeMonths = -60;

        // Act
        Action act = () => model.UpdateDepreciation(negativeUsefulLifeMonths, "Straight Line", 10m);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Useful life months cannot be negative");
    }

    [Fact]
    public void UpdateDepreciation_WithResidualValuePercentageOver100_ShouldThrowDomainException()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        decimal residualValuePercentageOver100 = 150m;

        // Act
        Action act = () => model.UpdateDepreciation(60, "Straight Line", residualValuePercentageOver100);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Residual value percentage must be between 0 and 100");
    }

    [Fact]
    public void UpdateDepreciation_WithNegativeResidualValuePercentage_ShouldThrowDomainException()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        decimal negativeResidualValuePercentage = -10m;

        // Act
        Action act = () => model.UpdateDepreciation(60, "Straight Line", negativeResidualValuePercentage);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Residual value percentage must be between 0 and 100");
    }

    [Fact]
    public void UpdateSoftwareLicensing_WithNegativeLicenseDurationMonths_ShouldThrowDomainException()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        int negativeLicenseDurationMonths = -12;

        // Act
        Action act = () => model.UpdateSoftwareLicensing("Subscription", negativeLicenseDurationMonths, true, 5);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("License duration months cannot be negative");
    }

    [Fact]
    public void UpdateSoftwareLicensing_WithNegativeMaxInstallations_ShouldThrowDomainException()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        int negativeMaxInstallations = -5;

        // Act
        Action act = () => model.UpdateSoftwareLicensing("Subscription", 12, true, negativeMaxInstallations);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Maximum installations cannot be negative");
    }

    [Fact]
    public void UpdateInventory_WithNegativeReorderLevel_ShouldThrowDomainException()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        int negativeReorderLevel = -10;

        // Act
        Action act = () => model.UpdateInventory(negativeReorderLevel, 100, DateTime.UtcNow);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Reorder level cannot be negative");
    }

    [Fact]
    public void UpdateInventory_WithNegativeCurrentStock_ShouldThrowDomainException()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        int negativeCurrentStock = -50;

        // Act
        Action act = () => model.UpdateInventory(10, negativeCurrentStock, DateTime.UtcNow);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Current stock cannot be negative");
    }

    [Fact]
    public void UpdateMedia_WithValidUrls_ShouldUpdateMedia()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        string imageUrl = "https://example.com/image.jpg";
        string thumbnailUrl = "https://example.com/thumb.jpg";
        string productPageUrl = "https://example.com/product";
        string datasheetUrl = "https://example.com/datasheet.pdf";
        string videoUrl = "https://example.com/video.mp4";

        // Act
        model.UpdateMedia(imageUrl, thumbnailUrl, productPageUrl, datasheetUrl, videoUrl);

        // Assert
        model.ImageUrl.Should().NotBeNull();
        model.ImageUrl.Should().Be(imageUrl);
        model.ThumbnailUrl.Should().NotBeNull();
        model.ThumbnailUrl.Should().Be(thumbnailUrl);
        model.ProductPageUrl.Should().NotBeNull();
        model.ProductPageUrl.Should().Be(productPageUrl);
        model.DatasheetUrl.Should().NotBeNull();
        model.DatasheetUrl.Should().Be(datasheetUrl);
        model.VideoUrl.Should().NotBeNull();
        model.VideoUrl.Should().Be(videoUrl);
    }

    [Fact]
    public void UpdateMedia_WithInvalidImageUrl_ShouldThrowDomainException()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        string invalidImageUrl = "not-a-url";

        // Act
        Action act = () => model.UpdateMedia(invalidImageUrl, null, null, null, null);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Invalid URL format");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        model.Deactivate();

        // Act
        model.Activate();

        // Assert
        model.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        Model model = Model.Create("Test Model");

        // Act
        model.Deactivate();

        // Assert
        model.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Discontinue_ShouldSetIsAvailableToFalseAndUpdateStatus()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        DateTime discontinuedDate = new(2024, 12, 31);

        // Act
        model.Discontinue(discontinuedDate);

        // Assert
        model.IsAvailable.Should().BeFalse();
        model.LifecycleStatus.Should().Be("Discontinued");
        model.DiscontinuedDate.Should().Be(discontinuedDate);
    }

    [Fact]
    public void IsEndOfLife_WithDiscontinuedStatus_ShouldReturnTrue()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        model.Discontinue();

        // Act
        bool isEndOfLife = model.IsEndOfLife();

        // Assert
        isEndOfLife.Should().BeTrue();
    }

    [Fact]
    public void IsEndOfLife_WithEOLStatus_ShouldReturnTrue()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        model.UpdateProductInfo(null, null, null, null, "EOL");

        // Act
        bool isEndOfLife = model.IsEndOfLife();

        // Assert
        isEndOfLife.Should().BeTrue();
    }

    [Fact]
    public void NeedsReorder_WithStockBelowReorderLevel_ShouldReturnTrue()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        model.UpdateInventory(10, 5, null); // Reorder level: 10, Current stock: 5

        // Act
        bool needsReorder = model.NeedsReorder();

        // Assert
        needsReorder.Should().BeTrue();
    }

    [Fact]
    public void NeedsReorder_WithStockAboveReorderLevel_ShouldReturnFalse()
    {
        // Arrange
        Model model = Model.Create("Test Model");
        model.UpdateInventory(10, 15, null); // Reorder level: 10, Current stock: 15

        // Act
        bool needsReorder = model.NeedsReorder();

        // Assert
        needsReorder.Should().BeFalse();
    }
}
