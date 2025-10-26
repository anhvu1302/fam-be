using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Rating Value Object - đảm bảo tính toàn vẹn của rating (1-5 sao)
/// </summary>
public sealed class Rating : ValueObject
{
    public int Value { get; private set; }

    private Rating(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo Rating từ int với validation
    /// </summary>
    public static Rating Create(int value)
    {
        if (value < 1 || value > 5)
            throw new DomainException("Rating must be between 1 and 5");

        return new Rating(value);
    }

    /// <summary>
    /// Tạo Rating từ decimal (làm tròn)
    /// </summary>
    public static Rating Create(decimal value)
    {
        var roundedValue = (int)Math.Round(value);
        return Create(roundedValue);
    }

    /// <summary>
    /// Rating tối đa (5 sao)
    /// </summary>
    public static Rating Max => new Rating(5);

    /// <summary>
    /// Rating tối thiểu (1 sao)
    /// </summary>
    public static Rating Min => new Rating(1);

    /// <summary>
    /// Rating trung bình (3 sao)
    /// </summary>
    public static Rating Average => new Rating(3);

    /// <summary>
    /// Kiểm tra có phải rating cao không (>= 4)
    /// </summary>
    public bool IsHighRating() => Value >= 4;

    /// <summary>
    /// Kiểm tra có phải rating thấp không (<= 2)
    /// </summary>
    public bool IsLowRating() => Value <= 2;

    /// <summary>
    /// Kiểm tra có phải rating trung bình không (= 3)
    /// </summary>
    public bool IsAverageRating() => Value == 3;

    /// <summary>
    /// Lấy mô tả text của rating
    /// </summary>
    public string GetDescription()
    {
        return Value switch
        {
            1 => "Very Poor",
            2 => "Poor",
            3 => "Average",
            4 => "Good",
            5 => "Excellent",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Lấy số sao dưới dạng string
    /// </summary>
    public string GetStars() => new string('★', Value) + new string('☆', 5 - Value);

    /// <summary>
    /// So sánh với rating khác
    /// </summary>
    public int CompareTo(Rating other) => Value.CompareTo(other.Value);

    /// <summary>
    /// Cộng hai rating (lấy trung bình)
    /// </summary>
    public Rating AverageWith(Rating other)
    {
        var average = (Value + other.Value) / 2.0m;
        return Create(average);
    }

    /// <summary>
    /// Implicit conversion từ Rating sang int
    /// </summary>
    public static implicit operator int(Rating rating) => rating.Value;

    /// <summary>
    /// Explicit conversion từ int sang Rating
    /// </summary>
    public static explicit operator Rating(int value) => Create(value);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value}/5 stars";
}