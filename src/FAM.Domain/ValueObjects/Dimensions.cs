using FAM.Domain.Common.Base;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Value Object cho kích thước vật lý
/// </summary>
public sealed class Dimensions : ValueObject
{
    public decimal Length { get; private set; }
    public decimal Width { get; private set; }
    public decimal Height { get; private set; }
    public string Unit { get; private set; } = null!;

    // Constructor for EF Core
    private Dimensions()
    {
    }

    private Dimensions(decimal length, decimal width, decimal height, string unit)
    {
        Length = length;
        Width = width;
        Height = height;
        Unit = unit;
    }

    public static Dimensions Create(decimal length, decimal width, decimal height, string unit = "cm")
    {
        if (length <= 0)
        {
            throw new DomainException(ErrorCodes.VO_DIMENSION_INVALID);
        }

        if (width <= 0)
        {
            throw new DomainException(ErrorCodes.VO_DIMENSION_INVALID);
        }

        if (height <= 0)
        {
            throw new DomainException(ErrorCodes.VO_DIMENSION_INVALID);
        }

        string[] validUnits = new[] { "cm", "m", "mm", "in", "ft" };
        if (!validUnits.Contains(unit.ToLowerInvariant()))
        {
            throw new DomainException(ErrorCodes.VO_DIMENSION_UNIT_EMPTY);
        }

        return new Dimensions(length, width, height, unit.ToLowerInvariant());
    }

    public static Dimensions? Parse(string dimensionsString)
    {
        if (string.IsNullOrWhiteSpace(dimensionsString))
        {
            return null;
        }

        // Format: "L x W x H" or "L x W x H cm"
        string[] parts = dimensionsString.Split(new[] { 'x', 'X', '*' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            return null;
        }

        string[] lastPart = parts[2].Trim().Split(' ');
        string heightStr = lastPart[0];
        string unit = lastPart.Length > 1 ? lastPart[1] : "cm";

        if (decimal.TryParse(parts[0].Trim(), out decimal length) &&
            decimal.TryParse(parts[1].Trim(), out decimal width) &&
            decimal.TryParse(heightStr, out decimal height))
        {
            try
            {
                return Create(length, width, height, unit);
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    public decimal Volume()
    {
        return Length * Width * Height;
    }

    public Dimensions ConvertTo(string targetUnit)
    {
        decimal multiplier = GetConversionMultiplier(Unit, targetUnit);
        return new Dimensions(
            Length * multiplier,
            Width * multiplier,
            Height * multiplier,
            targetUnit.ToLowerInvariant()
        );
    }

    private static decimal GetConversionMultiplier(string fromUnit, string toUnit)
    {
        // All conversions via cm as base
        decimal toCm = fromUnit.ToLowerInvariant() switch
        {
            "mm" => 0.1m,
            "cm" => 1m,
            "m" => 100m,
            "in" => 2.54m,
            "ft" => 30.48m,
            _ => 1m
        };

        decimal fromCm = toUnit.ToLowerInvariant() switch
        {
            "mm" => 10m,
            "cm" => 1m,
            "m" => 0.01m,
            "in" => 0.393701m,
            "ft" => 0.0328084m,
            _ => 1m
        };

        return toCm * fromCm;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Length;
        yield return Width;
        yield return Height;
        yield return Unit;
    }

    public override string ToString()
    {
        return $"{Length} x {Width} x {Height} {Unit}";
    }
}
