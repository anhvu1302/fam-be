using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Value Object cho khoảng thời gian
/// </summary>
public sealed class DateRange : ValueObject
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    private DateRange(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public static DateRange Create(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new DomainException("End date must be after or equal to start date");

        return new DateRange(startDate, endDate);
    }

    public static DateRange CreateFromDuration(DateTime startDate, int durationInDays)
    {
        return new DateRange(startDate, startDate.AddDays(durationInDays));
    }

    public int GetDurationInDays()
    {
        return (EndDate - StartDate).Days;
    }

    public bool Contains(DateTime date)
    {
        return date >= StartDate && date <= EndDate;
    }

    public bool Overlaps(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }

    public override string ToString()
    {
        return $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
    }
}