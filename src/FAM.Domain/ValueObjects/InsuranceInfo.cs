using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Value Object cho thông tin bảo hiểm
/// </summary>
public sealed class InsuranceInfo : ValueObject
{
    public string PolicyNumber { get; }
    public decimal InsuredValue { get; }
    public DateTime? ExpiryDate { get; }
    public string? Provider { get; }
    public string? CoverageType { get; }

    private InsuranceInfo(
        string policyNumber,
        decimal insuredValue,
        DateTime? expiryDate,
        string? provider,
        string? coverageType)
    {
        PolicyNumber = policyNumber;
        InsuredValue = insuredValue;
        ExpiryDate = expiryDate;
        Provider = provider;
        CoverageType = coverageType;
    }

    public static InsuranceInfo Create(string policyNumber, decimal insuredValue, DateTime? expiryDate = null)
    {
        if (string.IsNullOrWhiteSpace(policyNumber))
            throw new DomainException("Policy number is required");

        if (insuredValue <= 0)
            throw new DomainException("Insured value must be positive");

        return new InsuranceInfo(policyNumber, insuredValue, expiryDate, null, null);
    }

    public bool IsActive()
    {
        return ExpiryDate.HasValue && ExpiryDate.Value >= DateTime.UtcNow;
    }

    public bool IsExpired()
    {
        return ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    }

    public bool IsExpiringSoon(int daysThreshold = 30)
    {
        return ExpiryDate.HasValue &&
               ExpiryDate.Value <= DateTime.UtcNow.AddDays(daysThreshold) &&
               ExpiryDate.Value > DateTime.UtcNow;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return PolicyNumber;
        yield return InsuredValue;
        yield return ExpiryDate;
        yield return Provider;
        yield return CoverageType;
    }

    public override string ToString()
    {
        return $"Policy {PolicyNumber}, Insured: {InsuredValue:N2}";
    }
}