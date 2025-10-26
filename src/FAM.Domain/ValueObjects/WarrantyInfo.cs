using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Value Object cho thông tin bảo hành
/// </summary>
public sealed class WarrantyInfo : ValueObject
{
    public DateTime? StartDate { get; }
    public DateTime? EndDate { get; }
    public int? DurationMonths { get; }
    public string? Terms { get; }
    public string? Provider { get; }

    private WarrantyInfo(
        DateTime? startDate,
        DateTime? endDate,
        int? durationMonths,
        string? terms,
        string? provider)
    {
        StartDate = startDate;
        EndDate = endDate;
        DurationMonths = durationMonths;
        Terms = terms;
        Provider = provider;
    }

    public static WarrantyInfo Create(
        DateTime? startDate,
        int durationMonths,
        string? terms = null,
        string? provider = null)
    {
        if (durationMonths <= 0)
            throw new DomainException("Warranty duration must be positive");

        var endDate = startDate?.AddMonths(durationMonths);
        return new WarrantyInfo(startDate, endDate, durationMonths, terms, provider);
    }

    public static WarrantyInfo CreateWithEndDate(
        DateTime startDate,
        DateTime endDate,
        string? terms = null,
        string? provider = null)
    {
        if (endDate < startDate)
            throw new DomainException("End date must be after start date");

        var months = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month;
        return new WarrantyInfo(startDate, endDate, months, terms, provider);
    }

    public bool IsActive() => EndDate.HasValue && EndDate.Value >= DateTime.UtcNow;
    
    public bool IsExpired() => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;
    
    public int? DaysRemaining()
    {
        if (!EndDate.HasValue) return null;
        var days = (EndDate.Value - DateTime.UtcNow).Days;
        return days >= 0 ? days : 0;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
        yield return DurationMonths;
        yield return Terms;
        yield return Provider;
    }

    public override string ToString() =>
        EndDate.HasValue 
            ? $"Warranty until {EndDate.Value:yyyy-MM-dd}" 
            : "No warranty";
}
