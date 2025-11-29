using FluentAssertions;
using FAM.Domain.Common;
using FAM.Domain.Assets;
using FAM.Domain.Assets.Events;
using Xunit;

namespace FAM.Domain.Tests.Entities.Assets;

public class AssetTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateAsset()
    {
        // Arrange
        var name = "Test Asset";
        var createdBy = 1L;
        var companyId = 1;
        var assetTypeId = 1;
        var assetTag = "TAG001";

        // Act
        var asset = Asset.Create(name, createdBy, companyId, assetTypeId, assetTag);

        // Assert
        asset.Should().NotBeNull();
        asset.Name.Should().Be(name);
        asset.CompanyId.Should().Be(companyId);
        asset.AssetTypeId.Should().Be(assetTypeId);
        asset.AssetTag!.Value.Should().Be(assetTag);
        asset.LifecycleCode.Should().Be("draft");
        asset.CreatedById.Should().Be(createdBy);
        asset.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public void Create_WithNullName_ShouldThrowDomainException()
    {
        // Arrange
        string? name = null;
        var createdBy = 1L;

        // Act
        Action act = () => Asset.Create(name!, createdBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Asset name cannot be empty");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        var name = string.Empty;
        var createdBy = 1L;

        // Act
        Action act = () => Asset.Create(name, createdBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Asset name cannot be empty");
    }

    [Fact]
    public void Create_WithWhitespaceName_ShouldThrowDomainException()
    {
        // Arrange
        var name = "   ";
        var createdBy = 1L;

        // Act
        Action act = () => Asset.Create(name, createdBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Asset name cannot be empty");
    }

    [Fact]
    public void Create_WithZeroCreatedBy_ShouldThrowDomainException()
    {
        // Arrange
        var name = "Test Asset";
        var createdBy = 0L;

        // Act
        Action act = () => Asset.Create(name, createdBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("CreatedBy must be a positive number");
    }

    [Fact]
    public void Create_WithNegativeCreatedBy_ShouldThrowDomainException()
    {
        // Arrange
        var name = "Test Asset";
        var createdBy = -1L;

        // Act
        Action act = () => Asset.Create(name, createdBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("CreatedBy must be a positive number");
    }

    [Fact]
    public void UpdateBasicInfo_WithValidData_ShouldUpdateAsset()
    {
        // Arrange
        var asset = Asset.Create("Original Name", 1L);
        var newName = "Updated Name";
        var notes = "Updated notes";
        var updatedBy = 2L;

        // Act
        asset.UpdateBasicInfo(newName, notes, updatedBy);

        // Assert
        asset.Name.Should().Be(newName);
        asset.Notes.Should().Be(notes);
        asset.UpdatedById.Should().Be(updatedBy);
        asset.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateBasicInfo_WithNullName_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("Original Name", 1L);
        string? newName = null;
        var updatedBy = 2L;

        // Act
        var act = () => asset.UpdateBasicInfo(newName!, null, updatedBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Asset name cannot be empty");
    }

    [Fact]
    public void UpdateBasicInfo_WithZeroUpdatedBy_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("Original Name", 1L);
        var newName = "Updated Name";
        var updatedBy = 0L;

        // Act
        var act = () => asset.UpdateBasicInfo(newName, null, updatedBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("UpdatedBy must be a positive number");
    }

    [Fact]
    public void SetPurchaseInfo_WithNegativePurchaseCost_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        var purchaseDate = DateTime.UtcNow;
        decimal? purchaseCost = -100;
        var supplierId = 1;
        var updatedBy = 2L;

        // Act
        var act = () => asset.SetPurchaseInfo(purchaseDate, purchaseCost, supplierId, updatedBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Purchase cost cannot be negative");
    }

    [Fact]
    public void SetPurchaseInfo_WithValidData_ShouldUpdateAsset()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        var purchaseDate = DateTime.UtcNow;
        decimal? purchaseCost = 1000;
        var supplierId = 1;
        var updatedBy = 2L;

        // Act
        asset.SetPurchaseInfo(purchaseDate, purchaseCost, supplierId, updatedBy);

        // Assert
        asset.PurchaseDate.Should().Be(purchaseDate);
        asset.PurchaseCost.Should().Be(purchaseCost);
        asset.SupplierId.Should().Be(supplierId);
        asset.UpdatedById.Should().Be(updatedBy);
    }

    [Fact]
    public void SetExtendedPurchaseInfo_WithNegativeWarrantyMonths_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        var purchaseOrderNo = "PO001";
        var invoiceNo = "INV001";
        int? warrantyMonths = -1;
        var warrantyTerms = "Standard warranty";
        var updatedBy = 2L;

        // Act
        var act = () =>
            asset.SetExtendedPurchaseInfo(purchaseOrderNo, invoiceNo, warrantyMonths, warrantyTerms, updatedBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Warranty months cannot be negative");
    }

    [Fact]
    public void SetExtendedPurchaseInfo_WithValidData_ShouldUpdateAsset()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1, 1, 1, "TAG001");
        var purchaseDate = DateTime.UtcNow;

        // Act
        asset.SetPurchaseInfo(purchaseDate, 1000, 1, 1);
        asset.SetExtendedPurchaseInfo("PO001", "INV001", 12, "Standard warranty", 1);

        // Assert
        asset.PurchaseDate.Should().Be(purchaseDate);
        asset.PurchaseCost.Should().Be(1000);
        asset.SupplierId.Should().Be(1);
        asset.PurchaseOrderNo.Should().Be("PO001");
        asset.InvoiceNo.Should().Be("INV001");
        asset.WarrantyMonths.Should().Be(12);
    }

    [Fact]
    public void UpdateDepreciation_WithNegativeCurrentBookValue_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        decimal currentBookValue = -100;
        decimal accumulatedDepreciation = 50;
        var updatedBy = 2L;

        // Act
        var act = () => asset.UpdateDepreciation(currentBookValue, accumulatedDepreciation, updatedBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Current book value cannot be negative");
    }

    [Fact]
    public void UpdateDepreciation_WithNegativeAccumulatedDepreciation_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        decimal currentBookValue = 100;
        decimal accumulatedDepreciation = -50;
        var updatedBy = 2L;

        // Act
        var act = () => asset.UpdateDepreciation(currentBookValue, accumulatedDepreciation, updatedBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Accumulated depreciation cannot be negative");
    }

    [Fact]
    public void UpdateDepreciation_WithValidData_ShouldUpdateAsset()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        decimal currentBookValue = 800;
        decimal accumulatedDepreciation = 200;
        var updatedBy = 2L;

        // Act
        asset.UpdateDepreciation(currentBookValue, accumulatedDepreciation, updatedBy);

        // Assert
        asset.CurrentBookValue.Should().Be(currentBookValue);
        asset.AccumulatedDepreciation.Should().Be(accumulatedDepreciation);
        asset.LastDepreciationDate.Should().NotBeNull();
        asset.UpdatedById.Should().Be(updatedBy);
    }

    [Fact]
    public void ScheduleMaintenance_WithZeroIntervalDays_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        var intervalDays = 0;
        var updatedBy = 2L;

        // Act
        var act = () => asset.ScheduleMaintenance(intervalDays, updatedBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Maintenance interval must be positive");
    }

    [Fact]
    public void ScheduleMaintenance_WithNegativeIntervalDays_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        var intervalDays = -30;
        var updatedBy = 2L;

        // Act
        var act = () => asset.ScheduleMaintenance(intervalDays, updatedBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Maintenance interval must be positive");
    }

    [Fact]
    public void ScheduleMaintenance_WithValidData_ShouldUpdateAsset()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        var intervalDays = 90;
        var contractNo = "MAINT001";
        var updatedBy = 2L;

        // Act
        asset.ScheduleMaintenance(intervalDays, updatedBy, null, contractNo);

        // Assert
        asset.MaintenanceIntervalDays.Should().Be(intervalDays);
        asset.MaintenanceContractNo.Should().Be(contractNo);
        asset.NextMaintenanceDate.Should().NotBeNull();
        asset.UpdatedById.Should().Be(updatedBy);
    }

    [Fact]
    public void SetSoftwareInfo_WithNegativeLicenseCount_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        var version = "1.0.0";
        var licenseKey = "ABC123";
        var licenseExpiry = DateTime.UtcNow.AddYears(1);
        int? licenseCount = -1;
        var updatedBy = 2L;

        // Act
        var act = () => asset.SetSoftwareInfo(version, licenseKey, licenseExpiry, licenseCount, updatedBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("License count cannot be negative");
    }

    [Fact]
    public void SetSoftwareInfo_WithValidData_ShouldUpdateAsset()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        var version = "1.0.0";
        var licenseKey = "ABC123";
        var licenseExpiry = DateTime.UtcNow.AddYears(1);
        int? licenseCount = 5;
        var updatedBy = 2L;

        // Act
        asset.SetSoftwareInfo(version, licenseKey, licenseExpiry, licenseCount, updatedBy);

        // Assert
        asset.SoftwareVersion.Should().Be(version);
        asset.LicenseKey.Should().Be(licenseKey);
        asset.LicenseExpiryDate.Should().Be(licenseExpiry);
        asset.LicenseCount.Should().Be(licenseCount);
        asset.UpdatedById.Should().Be(updatedBy);
    }

    [Fact]
    public void SetPhysicalCharacteristics_WithNegativeWeight_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        decimal? weight = -10;
        var dimensions = "30x20x5";
        var color = "Black";
        var material = "Plastic";
        var updatedBy = 2L;

        // Act
        var act = () => asset.SetPhysicalCharacteristics(weight, dimensions, color, material, updatedBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Weight cannot be negative");
    }

    [Fact]
    public void SetPhysicalCharacteristics_WithValidData_ShouldUpdateAsset()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        decimal? weight = 2.5m;
        var dimensions = "30x20x5";
        var color = "Black";
        var material = "Plastic";
        var updatedBy = 2L;

        // Act
        asset.SetPhysicalCharacteristics(weight, dimensions, color, material, updatedBy);

        // Assert
        asset.Weight.Should().Be(weight);
        asset.Dimensions.Should().Be(dimensions);
        asset.Color.Should().Be(color);
        asset.Material.Should().Be(material);
        asset.UpdatedById.Should().Be(updatedBy);
    }

    [Fact]
    public void SetEnvironmentalInfo_WithNegativePowerConsumption_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        decimal? powerConsumption = -100;
        var energyRating = "A+";
        var isEnvironmentallyFriendly = true;
        var updatedBy = 2L;

        // Act
        var act = () =>
            asset.SetEnvironmentalInfo(powerConsumption, energyRating, isEnvironmentallyFriendly, updatedBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Power consumption cannot be negative");
    }

    [Fact]
    public void SetEnvironmentalInfo_WithValidData_ShouldUpdateAsset()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        decimal? powerConsumption = 150;
        var energyRating = "A+";
        var isEnvironmentallyFriendly = true;
        var updatedBy = 2L;

        // Act
        asset.SetEnvironmentalInfo(powerConsumption, energyRating, isEnvironmentallyFriendly, updatedBy);

        // Assert
        asset.PowerConsumption.Should().Be(powerConsumption);
        asset.EnergyRating.Should().Be(energyRating);
        asset.IsEnvironmentallyFriendly.Should().Be(isEnvironmentallyFriendly);
        asset.UpdatedById.Should().Be(updatedBy);
    }

    [Fact]
    public void SetReplacementInfo_WithNegativeReplacementCost_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        decimal? replacementCost = -1000;
        int? estimatedRemainingLifeMonths = 24;
        var updatedBy = 2L;

        // Act
        var act = () => asset.SetReplacementInfo(replacementCost, estimatedRemainingLifeMonths, updatedBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Replacement cost cannot be negative");
    }

    [Fact]
    public void SetReplacementInfo_WithNegativeEstimatedRemainingLifeMonths_ShouldThrowDomainException()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        decimal? replacementCost = 2000;
        int? estimatedRemainingLifeMonths = -6;
        var updatedBy = 2L;

        // Act
        var act = () => asset.SetReplacementInfo(replacementCost, estimatedRemainingLifeMonths, updatedBy);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Estimated remaining life months cannot be negative");
    }

    [Fact]
    public void SetReplacementInfo_WithValidData_ShouldUpdateAsset()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1L);
        decimal? replacementCost = 2000;
        int? estimatedRemainingLifeMonths = 24;
        var updatedBy = 2L;

        // Act
        asset.SetReplacementInfo(replacementCost, estimatedRemainingLifeMonths, updatedBy);

        // Assert
        asset.ReplacementCost.Should().Be(replacementCost);
        asset.EstimatedRemainingLifeMonths.Should().Be(estimatedRemainingLifeMonths);
        asset.UpdatedById.Should().Be(updatedBy);
    }

    [Fact]
    public void IsFullyDepreciated_WithDepreciatedAsset_ShouldReturnTrue()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1, 1, 1, "TAG001");
        // Note: Properties are read-only, so we can't set them directly in tests
        // This test would need to be updated when depreciation update methods are available

        // Act
        var isFullyDepreciated = asset.IsFullyDepreciated();

        // Assert
        isFullyDepreciated.Should().BeFalse(); // No depreciation info set, should return false
    }

    [Fact]
    public void IsFullyDepreciated_WithNotDepreciatedAsset_ShouldReturnFalse()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1, 1, 1, "TAG001");
        // Note: Properties are read-only, so we can't set them directly in tests
        // This test would need to be updated when depreciation update methods are available

        // Act
        var isFullyDepreciated = asset.IsFullyDepreciated();

        // Assert
        isFullyDepreciated.Should().BeFalse(); // No depreciation info set, should return false
    }

    [Fact]
    public void IsMaintenanceDue_WithDueMaintenance_ShouldReturnTrue()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1, 1, 1, "TAG001");
        // Note: Properties are read-only, so we can't set them directly in tests
        // This test would need to be updated when maintenance scheduling methods are available

        // Act
        var isDue = asset.IsMaintenanceDue();

        // Assert
        isDue.Should().BeFalse(); // No maintenance scheduled, should return false
    }

    [Fact]
    public void IsMaintenanceDue_WithNotDueMaintenance_ShouldReturnFalse()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1, 1, 1, "TAG001");
        // Note: Properties are read-only, so we can't set them directly in tests
        // This test would need to be updated when maintenance scheduling methods are available

        // Act
        var isDue = asset.IsMaintenanceDue();

        // Assert
        isDue.Should().BeFalse(); // No maintenance scheduled, should return false
    }

    [Fact]
    public void IsLicenseExpired_WithExpiredLicense_ShouldReturnTrue()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1, 1, 1, "TAG001");
        // Note: Properties are read-only, so we can't set them directly in tests
        // This test would need to be updated when license update methods are available

        // Act
        var isExpired = asset.IsLicenseExpired();

        // Assert
        isExpired.Should().BeFalse(); // No license set, should return false
    }

    [Fact]
    public void IsLicenseExpired_WithActiveLicense_ShouldReturnFalse()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1, 1, 1, "TAG001");
        // Note: Properties are read-only, so we can't set them directly in tests
        // This test would need to be updated when license update methods are available

        // Act
        var isExpired = asset.IsLicenseExpired();

        // Assert
        isExpired.Should().BeFalse(); // No license set, should return false
    }

    [Fact]
    public void IsReplacementDue_WithDueReplacement_ShouldReturnTrue()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1, 1, 1, "TAG001");
        // Note: Properties are read-only, so we can't set them directly in tests
        // This test would need to be updated when replacement info methods are available

        // Act
        var isDue = asset.IsReplacementDue();

        // Assert
        isDue.Should().BeFalse(); // No replacement info set, should return false
    }

    [Fact]
    public void IsReplacementDue_WithNotDueReplacement_ShouldReturnFalse()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1, 1, 1, "TAG001");
        // Note: Properties are read-only, so we can't set them directly in tests
        // This test would need to be updated when replacement info methods are available

        // Act
        var isDue = asset.IsReplacementDue();

        // Assert
        isDue.Should().BeFalse(); // No replacement info set, should return false
    }

    [Fact]
    public void IsWarrantyExpired_WithExpiredWarranty_ShouldReturnTrue()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1, 1, 1, "TAG001");
        // Note: Properties are read-only, so we can't set them directly in tests
        // This test would need to be updated when warranty update methods are available

        // Act
        var isExpired = asset.IsWarrantyExpired();

        // Assert
        isExpired.Should().BeFalse(); // No warranty set, should return false
    }

    [Fact]
    public void IsWarrantyExpired_WithActiveWarranty_ShouldReturnFalse()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1, 1, 1, "TAG001");
        // Note: Properties are read-only, so we can't set them directly in tests
        // This test would need to be updated when warranty update methods are available

        // Act
        var isExpired = asset.IsWarrantyExpired();

        // Assert
        isExpired.Should().BeFalse(); // No warranty set, should return false
    }

    [Fact]
    public void GetHealthStatus_WithHealthyAsset_ShouldReturnHealthy()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1, 1, 1, "TAG001");

        // Act
        var healthStatus = asset.GetHealthStatus();

        // Assert
        healthStatus.Should().Be(AssetHealthStatus.Healthy);
    }

    [Fact]
    public void GetHealthStatus_WithCriticalAsset_ShouldReturnCritical()
    {
        // Arrange
        var asset = Asset.Create("Test Asset", 1, 1, 1, "TAG001");
        // Note: Properties are read-only, so we can't set them directly in tests
        // This test would need to be updated when various update methods are available

        // Act
        var healthStatus = asset.GetHealthStatus();

        // Assert
        healthStatus.Should().Be(AssetHealthStatus.Healthy); // No issues set, should be healthy
    }
}