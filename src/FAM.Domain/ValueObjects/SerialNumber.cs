using FAM.Domain.Common.Base;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Serial Number Value Object - đảm bảo tính toàn vẹn của serial number
/// </summary>
public sealed class SerialNumber : ValueObject
{
    public string Value { get; private set; } = null!;

    // Constructor for EF Core
    private SerialNumber()
    {
    }

    private SerialNumber(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Tạo SerialNumber từ string với validation
    /// </summary>
    public static SerialNumber Create(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            throw new DomainException(ErrorCodes.VO_SERIAL_EMPTY);

        serialNumber = serialNumber.Trim();

        if (serialNumber.Length > 100)
            throw new DomainException(ErrorCodes.VO_SERIAL_INVALID);

        return new SerialNumber(serialNumber);
    }

    /// <summary>
    /// Implicit conversion từ SerialNumber sang string
    /// </summary>
    public static implicit operator string(SerialNumber serialNumber)
    {
        return serialNumber.Value;
    }

    /// <summary>
    /// Explicit conversion từ string sang SerialNumber
    /// </summary>
    public static explicit operator SerialNumber(string serialNumber)
    {
        return Create(serialNumber);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString()
    {
        return Value;
    }
}
