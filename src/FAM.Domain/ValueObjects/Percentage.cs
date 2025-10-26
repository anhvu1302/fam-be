using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Percentage Value Object - đảm bảo tính toàn vẹn của giá trị phần trăm (0-100%)
/// </summary>
public sealed class Percentage : ValueObject
{
    public decimal Value { get; private set; }

    private Percentage(decimal value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo Percentage từ decimal với validation
    /// </summary>
    public static Percentage Create(decimal value)
    {
        if (value < 0)
            throw new DomainException("Percentage cannot be negative");

        if (value > 100)
            throw new DomainException("Percentage cannot exceed 100%");

        // Làm tròn đến 2 chữ số thập phân
        value = Math.Round(value, 2);

        return new Percentage(value);
    }

    /// <summary>
    /// Tạo Percentage từ số thập phân (0.0 - 1.0)
    /// </summary>
    public static Percentage FromDecimal(decimal decimalValue)
    {
        return Create(decimalValue * 100);
    }

    /// <summary>
    /// Chuyển thành số thập phân (0.0 - 1.0)
    /// </summary>
    public decimal ToDecimal() => Value / 100;

    /// <summary>
    /// Chuyển thành fraction (0.0 - 1.0)
    /// </summary>
    public decimal ToFraction() => ToDecimal();

    /// <summary>
    /// Cộng hai Percentage
    /// </summary>
    public Percentage Add(Percentage other)
    {
        return Create(Value + other.Value);
    }

    /// <summary>
    /// Trừ hai Percentage
    /// </summary>
    public Percentage Subtract(Percentage other)
    {
        return Create(Value - other.Value);
    }

    /// <summary>
    /// Nhân với hệ số
    /// </summary>
    public Percentage Multiply(decimal factor)
    {
        return Create(Value * factor);
    }

    /// <summary>
    /// Kiểm tra có phải zero không
    /// </summary>
    public bool IsZero() => Value == 0;

    /// <summary>
    /// Kiểm tra có phải 100% không
    /// </summary>
    public bool IsFull() => Value == 100;

    /// <summary>
    /// Implicit conversion từ Percentage sang decimal
    /// </summary>
    public static implicit operator decimal(Percentage percentage) => percentage.Value;

    /// <summary>
    /// Explicit conversion từ decimal sang Percentage
    /// </summary>
    public static explicit operator Percentage(decimal value) => Create(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value}%";
}