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
        string costCenter = "CC001";
        int headcount = 10;
        decimal budgetYear = 1000000m;

        // Act
        DepartmentDetails details = DepartmentDetails.Create(costCenter, headcount, budgetYear);

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
        DepartmentDetails details = DepartmentDetails.Create();

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
        string costCenter = "CC001";
        int headcount = -1;
        decimal budgetYear = 1000000m;

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
        string costCenter = "CC001";
        int headcount = 10;
        decimal budgetYear = -1000m;

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
        DepartmentDetails details = DepartmentDetails.Create();
        string costCenter = "CC001";
        int headcount = 10;
        decimal budgetYear = 1000000m;

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
        DepartmentDetails details = DepartmentDetails.Create();
        string costCenter = "CC001";
        int headcount = -1;
        decimal budgetYear = 1000000m;

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
        DepartmentDetails details = DepartmentDetails.Create();
        string costCenter = "CC001";
        int headcount = 10;
        decimal budgetYear = -1000m;

        // Act
        Action act = () => details.Update(costCenter, headcount, budgetYear);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Budget year cannot be negative");
    }
}
