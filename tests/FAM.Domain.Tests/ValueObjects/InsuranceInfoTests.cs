using FAM.Domain.Common.Base;
using FAM.Domain.ValueObjects;

using FluentAssertions;

namespace FAM.Domain.Tests.ValueObjects;

public class InsuranceInfoTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateInsuranceInfo()
    {
        // Arrange
        string policyNumber = "POL-123456";
        decimal insuredValue = 100000.00m;

        // Act
        InsuranceInfo insuranceInfo = InsuranceInfo.Create(policyNumber, insuredValue);

        // Assert
        insuranceInfo.Should().NotBeNull();
        insuranceInfo.PolicyNumber.Should().Be(policyNumber);
        insuranceInfo.InsuredValue.Should().Be(insuredValue);
        insuranceInfo.ExpiryDate.Should().BeNull();
        insuranceInfo.Provider.Should().BeNull();
        insuranceInfo.CoverageType.Should().BeNull();
    }

    [Fact]
    public void Create_WithExpiryDate_ShouldCreateInsuranceInfo()
    {
        // Arrange
        string policyNumber = "POL-123456";
        decimal insuredValue = 100000.00m;
        DateTime expiryDate = DateTime.UtcNow.AddYears(1);

        // Act
        InsuranceInfo insuranceInfo = InsuranceInfo.Create(policyNumber, insuredValue, expiryDate);

        // Assert
        insuranceInfo.ExpiryDate.Should().Be(expiryDate);
    }

    [Fact]
    public void Create_WithEmptyPolicyNumber_ShouldThrowDomainException()
    {
        // Arrange
        string policyNumber = "";
        decimal insuredValue = 100000.00m;

        // Act
        Action act = () => InsuranceInfo.Create(policyNumber, insuredValue);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Policy number is required");
    }

    [Fact]
    public void Create_WithWhitespacePolicyNumber_ShouldThrowDomainException()
    {
        // Arrange
        string policyNumber = "   ";
        decimal insuredValue = 100000.00m;

        // Act
        Action act = () => InsuranceInfo.Create(policyNumber, insuredValue);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Policy number is required");
    }

    [Fact]
    public void Create_WithZeroInsuredValue_ShouldThrowDomainException()
    {
        // Arrange
        string policyNumber = "POL-123456";
        decimal insuredValue = 0m;

        // Act
        Action act = () => InsuranceInfo.Create(policyNumber, insuredValue);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Insured value must be positive");
    }

    [Fact]
    public void Create_WithNegativeInsuredValue_ShouldThrowDomainException()
    {
        // Arrange
        string policyNumber = "POL-123456";
        decimal insuredValue = -1000m;

        // Act
        Action act = () => InsuranceInfo.Create(policyNumber, insuredValue);

        // Assert
        act.Should().Throw<DomainException>()
            .WithMessage("Insured value must be positive");
    }

    [Fact]
    public void IsActive_WithFutureExpiryDate_ShouldReturnTrue()
    {
        // Arrange
        InsuranceInfo insuranceInfo = InsuranceInfo.Create("POL-123456", 100000m, DateTime.UtcNow.AddDays(30));

        // Act
        bool result = insuranceInfo.IsActive();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsActive_WithPastExpiryDate_ShouldReturnFalse()
    {
        // Arrange
        InsuranceInfo insuranceInfo = InsuranceInfo.Create("POL-123456", 100000m, DateTime.UtcNow.AddDays(-1));

        // Act
        bool result = insuranceInfo.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsActive_WithNoExpiryDate_ShouldReturnFalse()
    {
        // Arrange
        InsuranceInfo insuranceInfo = InsuranceInfo.Create("POL-123456", 100000m);

        // Act
        bool result = insuranceInfo.IsActive();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WithPastExpiryDate_ShouldReturnTrue()
    {
        // Arrange
        InsuranceInfo insuranceInfo = InsuranceInfo.Create("POL-123456", 100000m, DateTime.UtcNow.AddDays(-1));

        // Act
        bool result = insuranceInfo.IsExpired();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WithFutureExpiryDate_ShouldReturnFalse()
    {
        // Arrange
        InsuranceInfo insuranceInfo = InsuranceInfo.Create("POL-123456", 100000m, DateTime.UtcNow.AddDays(30));

        // Act
        bool result = insuranceInfo.IsExpired();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsExpiringSoon_WithExpiryWithinThreshold_ShouldReturnTrue()
    {
        // Arrange
        InsuranceInfo insuranceInfo = InsuranceInfo.Create("POL-123456", 100000m, DateTime.UtcNow.AddDays(15));

        // Act
        bool result = insuranceInfo.IsExpiringSoon(30);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsExpiringSoon_WithExpiryOutsideThreshold_ShouldReturnFalse()
    {
        // Arrange
        InsuranceInfo insuranceInfo = InsuranceInfo.Create("POL-123456", 100000m, DateTime.UtcNow.AddDays(60));

        // Act
        bool result = insuranceInfo.IsExpiringSoon(30);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        InsuranceInfo insuranceInfo = InsuranceInfo.Create("POL-123456", 100000.50m);

        // Act
        string result = insuranceInfo.ToString();

        // Assert
        result.Should().Be("Policy POL-123456, Insured: 100,000.50");
    }
}
