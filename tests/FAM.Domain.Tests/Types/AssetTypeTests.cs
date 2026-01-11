using FAM.Domain.Common.Base;
using FAM.Domain.Types;

using FluentAssertions;

namespace FAM.Domain.Tests.Types;

public class AssetTypeTests
{
    [Fact]
    public void Create_WithValidCodeAndName_ShouldCreateAssetType()
    {
        // Arrange
        string code = "IT";
        string name = "Information Technology";

        // Act
        AssetType assetType = AssetType.Create(code, name);

        // Assert
        assetType.Should().NotBeNull();
        assetType.Code.Should().Be(code.ToUpper());
        assetType.Name.Should().Be(name);
        assetType.IsActive.Should().BeTrue();
        assetType.IsDepreciable.Should().BeTrue();
        assetType.IsAssignable.Should().BeTrue();
        assetType.IsTangible.Should().BeTrue();
        assetType.IsCapitalized.Should().BeTrue();
        assetType.RequiresMaintenance.Should().BeTrue();
        assetType.Level.Should().Be(0);
    }

    [Fact]
    public void Create_WithLowercaseCode_ShouldConvertToUppercase()
    {
        // Arrange
        string code = "it";
        string name = "Information Technology";

        // Act
        AssetType assetType = AssetType.Create(code, name);

        // Assert
        assetType.Code.Should().Be("IT");
    }

    [Fact]
    public void Create_WithNullOrEmptyCode_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => AssetType.Create(null!, "Name"));
        Assert.Throws<DomainException>(() => AssetType.Create("", "Name"));
        Assert.Throws<DomainException>(() => AssetType.Create("   ", "Name"));
    }

    [Fact]
    public void Create_WithNullOrEmptyName_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => AssetType.Create("IT", null!));
        Assert.Throws<DomainException>(() => AssetType.Create("IT", ""));
        Assert.Throws<DomainException>(() => AssetType.Create("IT", "   "));
    }

    [Fact]
    public void UpdateBasicInfo_WithValidData_ShouldUpdateAssetType()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Old Name");
        string newName = "New Name";
        string newDescription = "New Description";
        string newLongDescription = "New Long Description";

        // Act
        assetType.UpdateBasicInfo(newName, newDescription, newLongDescription);

        // Assert
        assetType.Name.Should().Be(newName);
        assetType.Description.Should().Be(newDescription);
        assetType.LongDescription.Should().Be(newLongDescription);
    }

    [Fact]
    public void UpdateBasicInfo_WithNullOrEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => assetType.UpdateBasicInfo(null!, null, null));
        Assert.Throws<DomainException>(() => assetType.UpdateBasicInfo("", null, null));
        Assert.Throws<DomainException>(() => assetType.UpdateBasicInfo("   ", null, null));
    }

    [Fact]
    public void UpdateClassification_WithValidData_ShouldUpdateClassification()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.UpdateClassification("Fixed Asset", "Hardware", "Computer");

        // Assert
        assetType.AssetClass.Should().Be("Fixed Asset");
        assetType.Category.Should().Be("Hardware");
        assetType.Subcategory.Should().Be("Computer");
    }

    [Fact]
    public void UpdateProperties_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.UpdateProperties(
            false, false, false, true, false, true, false, true, true, true);

        // Assert
        assetType.IsDepreciable.Should().BeFalse();
        assetType.IsAssignable.Should().BeFalse();
        assetType.IsTangible.Should().BeFalse();
        assetType.IsConsumable.Should().BeTrue();
        assetType.IsCapitalized.Should().BeFalse();
        assetType.RequiresLicense.Should().BeTrue();
        assetType.RequiresMaintenance.Should().BeFalse();
        assetType.RequiresCalibration.Should().BeTrue();
        assetType.RequiresInsurance.Should().BeTrue();
        assetType.IsITAsset.Should().BeTrue();
    }

    [Fact]
    public void UpdateDepreciationDefaults_WithValidData_ShouldUpdateDefaults()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.UpdateDepreciationDefaults("Straight Line", 60, 10.5m, "DEP001", "ACCDEP001");

        // Assert
        assetType.DefaultDepreciationMethod.Should().Be("Straight Line");
        assetType.DefaultUsefulLifeMonths.Should().Be(60);
        assetType.DefaultResidualValuePercentage.Should().Be(10.5m);
        assetType.DepreciationAccountCode.Should().Be("DEP001");
        assetType.AccumulatedDepreciationAccountCode.Should().Be("ACCDEP001");
    }

    [Fact]
    public void UpdateDepreciationDefaults_WithNegativeUsefulLife_ShouldThrowDomainException()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => assetType.UpdateDepreciationDefaults("Method", -1, 10, null, null));
    }

    [Fact]
    public void UpdateDepreciationDefaults_WithInvalidResidualValuePercentage_ShouldThrowDomainException()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => assetType.UpdateDepreciationDefaults("Method", 60, -1, null, null));
        Assert.Throws<DomainException>(() => assetType.UpdateDepreciationDefaults("Method", 60, 150, null, null));
    }

    [Fact]
    public void UpdateAccounting_WithValidData_ShouldUpdateAccounting()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.UpdateAccounting("GL001", "ASSET001", "EXP001", "CC001");

        // Assert
        assetType.GLAccountCode.Should().Be("GL001");
        assetType.AssetAccountCode.Should().Be("ASSET001");
        assetType.ExpenseAccountCode.Should().Be("EXP001");
        assetType.CostCenter.Should().Be("CC001");
    }

    [Fact]
    public void UpdateLifecycleDefaults_WithValidData_ShouldUpdateLifecycle()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.UpdateLifecycleDefaults(24, 365, "Preventive");

        // Assert
        assetType.DefaultWarrantyMonths.Should().Be(24);
        assetType.DefaultMaintenanceIntervalDays.Should().Be(365);
        assetType.DefaultMaintenanceType.Should().Be("Preventive");
    }

    [Fact]
    public void UpdateLifecycleDefaults_WithNegativeWarranty_ShouldThrowDomainException()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => assetType.UpdateLifecycleDefaults(-1, 365, "Type"));
    }

    [Fact]
    public void UpdateLifecycleDefaults_WithNegativeMaintenanceInterval_ShouldThrowDomainException()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => assetType.UpdateLifecycleDefaults(24, -1, "Type"));
    }

    [Fact]
    public void UpdateValuation_WithValidData_ShouldUpdateValuation()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.UpdateValuation(1000.50m, "USD", "Cost");

        // Assert
        assetType.MinimumCapitalizationValue.Should().Be(1000.50m);
        assetType.ValuationCurrency.Should().Be("USD");
        assetType.ValuationMethod.Should().Be("Cost");
    }

    [Fact]
    public void UpdateValuation_WithNegativeMinimumValue_ShouldThrowDomainException()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => assetType.UpdateValuation(-100, "USD", "Cost"));
    }

    [Fact]
    public void UpdateCompliance_WithValidData_ShouldUpdateCompliance()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.UpdateCompliance(true, "ISO 9001", "Regulatory Req", true, 12);

        // Assert
        assetType.RequiresCompliance.Should().BeTrue();
        assetType.ComplianceStandards.Should().Be("ISO 9001");
        assetType.RegulatoryRequirements.Should().Be("Regulatory Req");
        assetType.RequiresAudit.Should().BeTrue();
        assetType.AuditIntervalMonths.Should().Be(12);
    }

    [Fact]
    public void UpdateCompliance_WithNegativeAuditInterval_ShouldThrowDomainException()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => assetType.UpdateCompliance(true, null, null, true, -1));
    }

    [Fact]
    public void UpdateSecurity_WithValidData_ShouldUpdateSecurity()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.UpdateSecurity("Confidential", true, true);

        // Assert
        assetType.DefaultSecurityClassification.Should().Be("Confidential");
        assetType.RequiresBackgroundCheck.Should().BeTrue();
        assetType.RequiresAccessControl.Should().BeTrue();
    }

    [Fact]
    public void UpdateWorkflow_WithValidData_ShouldUpdateWorkflow()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.UpdateWorkflow(true, true, "Approval Workflow JSON");

        // Assert
        assetType.RequiresApprovalToAcquire.Should().BeTrue();
        assetType.RequiresApprovalToDispose.Should().BeTrue();
        assetType.ApprovalWorkflow.Should().Be("Approval Workflow JSON");
    }

    [Fact]
    public void UpdateCustomFields_WithValidData_ShouldUpdateCustomFields()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.UpdateCustomFields("Schema JSON", "Required Fields JSON");

        // Assert
        assetType.CustomFieldsSchema.Should().Be("Schema JSON");
        assetType.RequiredFields.Should().Be("Required Fields JSON");
    }

    [Fact]
    public void UpdateDisplay_WithValidData_ShouldUpdateDisplay()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.UpdateDisplay("icon.png", "https://example.com/icon.png", "#FF0000", 5);

        // Assert
        assetType.IconName.Should().Be("icon.png");
        assetType.IconUrl.Should().NotBeNull();
        assetType.IconUrl!.ToString().Should().Be("https://example.com/icon.png");
        assetType.Color.Should().Be("#FF0000");
        assetType.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void UpdateDisplay_WithInvalidUrl_ShouldThrowDomainException()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => assetType.UpdateDisplay("icon", "invalid-url", null, 0));
    }

    [Fact]
    public void SetParent_WithValidData_ShouldSetParent()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Child");

        // Act
        assetType.SetParent(1, 2, "/IT/Hardware");

        // Assert
        assetType.ParentId.Should().Be(1);
        assetType.Level.Should().Be(2);
        assetType.Path.Should().Be("/IT/Hardware");
    }

    [Fact]
    public void UpdateSearchTags_WithValidData_ShouldUpdateTags()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.UpdateSearchTags("tag1,tag2", "keyword1 keyword2", "alias1,alias2");

        // Assert
        assetType.Tags.Should().Be("tag1,tag2");
        assetType.SearchKeywords.Should().Be("keyword1 keyword2");
        assetType.Aliases.Should().Be("alias1,alias2");
    }

    [Fact]
    public void AddNotes_WithValidData_ShouldAddNotes()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.AddNotes("Internal notes", "Procurement notes");

        // Assert
        assetType.InternalNotes.Should().Be("Internal notes");
        assetType.ProcurementNotes.Should().Be("Procurement notes");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");
        assetType.Deactivate();

        // Act
        assetType.Activate();

        // Assert
        assetType.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.Deactivate();

        // Assert
        assetType.IsActive.Should().BeFalse();
    }

    [Fact]
    public void MarkAsSystemType_ShouldSetIsSystemTypeToTrue()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.MarkAsSystemType();

        // Assert
        assetType.IsSystemType.Should().BeTrue();
    }

    [Fact]
    public void UpdateStatistics_WithValidData_ShouldUpdateStatistics()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act
        assetType.UpdateStatistics(10, 10000.50m);

        // Assert
        assetType.AssetCount.Should().Be(10);
        assetType.TotalValue.Should().Be(10000.50m);
    }

    [Fact]
    public void UpdateStatistics_WithNegativeAssetCount_ShouldThrowDomainException()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => assetType.UpdateStatistics(-1, 1000));
    }

    [Fact]
    public void UpdateStatistics_WithNegativeTotalValue_ShouldThrowDomainException()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => assetType.UpdateStatistics(10, -1000));
    }

    [Fact]
    public void IsHierarchical_WithParent_ShouldReturnTrue()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Child");
        assetType.SetParent(1, 1, "/IT");

        // Act
        bool result = assetType.IsHierarchical();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRoot_WithNoParent_ShouldReturnTrue()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Root");

        // Act
        bool result = assetType.IsRoot();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLeaf_WithNoChildren_ShouldReturnTrue()
    {
        // Arrange
        AssetType assetType = AssetType.Create("IT", "Leaf");

        // Act
        bool result = assetType.IsLeaf();

        // Assert
        result.Should().BeTrue();
    }
}
