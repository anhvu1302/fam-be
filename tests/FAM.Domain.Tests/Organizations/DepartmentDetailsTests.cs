using FAM.Domain.Common.Base;
using FAM.Domain.Organizations;

using FluentAssertions;

namespace FAM.Domain.Tests.Entities.Organizations;

public class DepartmentDetailsTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateDepartmentDetails()
    {
        // Arrange
        var costCenter = "CC001";
        var headcount = 10;
        var budgetYear = 1000000m;

        // Act
        var details = DepartmentDetails.Create(costCenter, headcount, budgetYear);

        // Assert
        details.Should().NotBeNull();
        string costCenterValue = details.CostCenter!;
        costCenterValue.Should().Be("CC001");
        details.Headcount.Should().Be(headcount);
        details.BudgetYear.Should().Be(budgetYear);
    }

    [Fact]
    public void Create_WithNullValues_ShouldCreateDepartmentDetails()
    {
        // Act
        var details = DepartmentDetails.Create();

        // Assert
        details.Should().NotBeNull();
        details.CostCenter.Should().BeNull();
        details.Headcount.Should().BeNull();
        details.BudgetYear.Should().BeNull();
    }

    [Fact]
    public void Create_WithNegativeHeadcount_ShouldThrowDomainException()
    {
        // Arrange
        var costCenter = "CC001";
        var headcount = -1;
        var budgetYear = 1000000m;

        // Act
        Action act = () => DepartmentDetails.Create(costCenter, headcount, budgetYear);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Headcount cannot be negative");
    }

    [Fact]
    public void Create_WithNegativeBudgetYear_ShouldThrowDomainException()
    {
        // Arrange
        var costCenter = "CC001";
        var headcount = 10;
        var budgetYear = -1000m;

        // Act
        Action act = () => DepartmentDetails.Create(costCenter, headcount, budgetYear);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Budget year cannot be negative");
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateDepartmentDetails()
    {
        // Arrange
        var details = DepartmentDetails.Create();
        var costCenter = "CC001";
        var headcount = 10;
        var budgetYear = 1000000m;

        // Act
        details.Update(costCenter, headcount, budgetYear);

        // Assert
        string costCenterValue = details.CostCenter!;
        costCenterValue.Should().Be("CC001");
        details.Headcount.Should().Be(headcount);
        details.BudgetYear.Should().Be(budgetYear);
    }

    [Fact]
    public void Update_WithNegativeHeadcount_ShouldThrowDomainException()
    {
        // Arrange
        var details = DepartmentDetails.Create();
        var costCenter = "CC001";
        var headcount = -1;
        var budgetYear = 1000000m;

        // Act
        Action act = () => details.Update(costCenter, headcount, budgetYear);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Headcount cannot be negative");
    }

    [Fact]
    public void Update_WithNegativeBudgetYear_ShouldThrowDomainException()
    {
        // Arrange
        var details = DepartmentDetails.Create();
        var costCenter = "CC001";
        var headcount = 10;
        var budgetYear = -1000m;

        // Act
        Action act = () => details.Update(costCenter, headcount, budgetYear);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Budget year cannot be negative");
    }
}
