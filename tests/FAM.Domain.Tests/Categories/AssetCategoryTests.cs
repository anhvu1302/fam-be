using FAM.Domain.Categories;
using FAM.Domain.Common.Base;

using FluentAssertions;

namespace FAM.Domain.Tests.Categories;

public class AssetCategoryTests
{
    [Fact]
    public void Create_WithValidName_ShouldCreateCategory()
    {
        // Arrange
        var name = "Office Equipment";

        // Act
        var category = AssetCategory.Create(name);

        // Assert
        category.Should().NotBeNull();
        category.Name.Should().Be(name);
        category.IsActive.Should().BeTrue();
        category.IsDepreciable.Should().BeTrue();
        category.IsCapitalized.Should().BeTrue();
        category.RequiresMaintenance.Should().BeTrue();
        category.Level.Should().Be(0);
    }

    [Fact]
    public void Create_WithNameAndCode_ShouldCreateCategoryWithCode()
    {
        // Arrange
        var name = "Office Equipment";
        var code = "OE";

        // Act
        var category = AssetCategory.Create(name, code);

        // Assert
        category.Code.Should().Be("OE");
    }

    [Fact]
    public void Create_WithNullOrEmptyName_ShouldThrowDomainException()
    {
        // Arrange & Act & Assert
        Assert.Throws<DomainException>(() => AssetCategory.Create(null!));
        Assert.Throws<DomainException>(() => AssetCategory.Create(""));
        Assert.Throws<DomainException>(() => AssetCategory.Create("   "));
    }

    [Fact]
    public void Create_WithLowercaseCode_ShouldConvertToUppercase()
    {
        // Arrange
        var name = "Office Equipment";
        var code = "oe";

        // Act
        var category = AssetCategory.Create(name, code);

        // Assert
        category.Code.Should().Be("OE");
    }

    [Fact]
    public void UpdateBasicInfo_WithValidData_ShouldUpdateCategory()
    {
        // Arrange
        var category = AssetCategory.Create("Old Name");
        var newName = "New Name";
        var newCode = "NN";
        var newDescription = "New Description";
        var newLongDescription = "New Long Description";

        // Act
        category.UpdateBasicInfo(newName, newCode, newDescription, newLongDescription);

        // Assert
        category.Name.Should().Be(newName);
        category.Code.Should().Be(newCode);
        category.Description.Should().Be(newDescription);
        category.LongDescription.Should().Be(newLongDescription);
    }

    [Fact]
    public void UpdateBasicInfo_WithNullOrEmptyName_ShouldThrowDomainException()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => category.UpdateBasicInfo(null!, null, null, null));
        Assert.Throws<DomainException>(() => category.UpdateBasicInfo("", null, null, null));
        Assert.Throws<DomainException>(() => category.UpdateBasicInfo("   ", null, null, null));
    }

    [Fact]
    public void UpdateClassification_WithValidData_ShouldUpdateClassification()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act
        category.UpdateClassification("Functional", "Healthcare", "Medical");

        // Assert
        category.CategoryType.Should().Be("Functional");
        category.Industry.Should().Be("Healthcare");
        category.Sector.Should().Be("Medical");
    }

    [Fact]
    public void UpdateAccounting_WithValidData_ShouldUpdateAccounting()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act
        category.UpdateAccounting("GL123", "DEP456", "CC789");

        // Assert
        category.GLAccountCode.Should().Be("GL123");
        category.DepreciationAccountCode.Should().Be("DEP456");
        category.CostCenter.Should().Be("CC789");
    }

    [Fact]
    public void UpdateDepreciationDefaults_WithValidData_ShouldUpdateDefaults()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act
        category.UpdateDepreciationDefaults("Straight Line", 60, 10.5m);

        // Assert
        category.DefaultDepreciationMethod.Should().Be("Straight Line");
        category.DefaultUsefulLifeMonths.Should().Be(60);
        category.DefaultResidualValuePercentage.Should().Be(10.5m);
    }

    [Fact]
    public void UpdateDepreciationDefaults_WithNegativeUsefulLife_ShouldThrowDomainException()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => category.UpdateDepreciationDefaults("Method", -1, 10));
    }

    [Fact]
    public void UpdateDepreciationDefaults_WithInvalidResidualValuePercentage_ShouldThrowDomainException()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => category.UpdateDepreciationDefaults("Method", 60, -1));
        Assert.Throws<DomainException>(() => category.UpdateDepreciationDefaults("Method", 60, 150));
    }

    [Fact]
    public void UpdateProperties_WithValidData_ShouldUpdateProperties()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act
        category.UpdateProperties(false, false, false, true);

        // Assert
        category.IsDepreciable.Should().BeFalse();
        category.IsCapitalized.Should().BeFalse();
        category.RequiresMaintenance.Should().BeFalse();
        category.RequiresInsurance.Should().BeTrue();
    }

    [Fact]
    public void UpdateValuation_WithValidData_ShouldUpdateValuation()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act
        category.UpdateValuation(1000.50m, "Fair Value");

        // Assert
        category.MinimumCapitalizationValue.Should().Be(1000.50m);
        category.ValuationMethod.Should().Be("Fair Value");
    }

    [Fact]
    public void UpdateValuation_WithNegativeMinimumValue_ShouldThrowDomainException()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => category.UpdateValuation(-100, "Method"));
    }

    [Fact]
    public void UpdateCompliance_WithValidData_ShouldUpdateCompliance()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act
        category.UpdateCompliance(true, "ISO 9001", true, 12);

        // Assert
        category.RequiresCompliance.Should().BeTrue();
        category.ComplianceStandards.Should().Be("ISO 9001");
        category.RequiresAudit.Should().BeTrue();
        category.AuditIntervalMonths.Should().Be(12);
    }

    [Fact]
    public void UpdateCompliance_WithNegativeAuditInterval_ShouldThrowDomainException()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => category.UpdateCompliance(true, null, true, -1));
    }

    [Fact]
    public void UpdateDisplay_WithValidData_ShouldUpdateDisplay()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act
        category.UpdateDisplay("icon.png", "https://example.com/icon.png", "#FF0000", 5);

        // Assert
        category.IconName.Should().Be("icon.png");
        category.IconUrl.Should().NotBeNull();
        category.IconUrl!.ToString().Should().Be("https://example.com/icon.png");
        category.Color.Should().Be("#FF0000");
        category.DisplayOrder.Should().Be(5);
    }

    [Fact]
    public void UpdateDisplay_WithInvalidUrl_ShouldThrowDomainException()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => category.UpdateDisplay("icon", "invalid-url", null, 0));
    }

    [Fact]
    public void SetParent_WithValidData_ShouldSetParent()
    {
        // Arrange
        var category = AssetCategory.Create("Child");

        // Act
        category.SetParent(1, 2, "1.2");

        // Assert
        category.ParentId.Should().Be(1);
        category.Level.Should().Be(2);
        category.Path.Should().Be("1.2");
    }

    [Fact]
    public void UpdateSearchTags_WithValidData_ShouldUpdateTags()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act
        category.UpdateSearchTags("tag1,tag2", "keyword1 keyword2", "alias1,alias2");

        // Assert
        category.Tags.Should().Be("tag1,tag2");
        category.SearchKeywords.Should().Be("keyword1 keyword2");
        category.Aliases.Should().Be("alias1,alias2");
    }

    [Fact]
    public void AddNotes_WithValidData_ShouldAddNotes()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act
        category.AddNotes("Internal notes");

        // Assert
        category.InternalNotes.Should().Be("Internal notes");
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var category = AssetCategory.Create("Test");
        category.Deactivate();

        // Act
        category.Activate();

        // Assert
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.Should().BeFalse();
    }

    [Fact]
    public void MarkAsSystemCategory_ShouldSetIsSystemCategoryToTrue()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act
        category.MarkAsSystemCategory();

        // Assert
        category.IsSystemCategory.Should().BeTrue();
    }

    [Fact]
    public void UpdateStatistics_WithValidData_ShouldUpdateStatistics()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act
        category.UpdateStatistics(10, 10000.50m);

        // Assert
        category.AssetCount.Should().Be(10);
        category.TotalValue.Should().Be(10000.50m);
    }

    [Fact]
    public void UpdateStatistics_WithNegativeAssetCount_ShouldThrowDomainException()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => category.UpdateStatistics(-1, 1000));
    }

    [Fact]
    public void UpdateStatistics_WithNegativeTotalValue_ShouldThrowDomainException()
    {
        // Arrange
        var category = AssetCategory.Create("Test");

        // Act & Assert
        Assert.Throws<DomainException>(() => category.UpdateStatistics(10, -1000));
    }

    [Fact]
    public void IsHierarchical_WithParent_ShouldReturnTrue()
    {
        // Arrange
        var category = AssetCategory.Create("Child");
        category.SetParent(1, 1, "1");

        // Act
        var result = category.IsHierarchical();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsRoot_WithNoParent_ShouldReturnTrue()
    {
        // Arrange
        var category = AssetCategory.Create("Root");

        // Act
        var result = category.IsRoot();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsLeaf_WithNoChildren_ShouldReturnTrue()
    {
        // Arrange
        var category = AssetCategory.Create("Leaf");

        // Act
        var result = category.IsLeaf();

        // Assert
        result.Should().BeTrue();
    }
}
