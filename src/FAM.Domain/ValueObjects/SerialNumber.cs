using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Serial Number Value Object - đảm bảo tính toàn vẹn của serial number
/// </summary>
public sealed class SerialNumber : ValueObject
{
    public string Value { get; private set; }

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
            throw new DomainException("Serial number cannot be empty");

        serialNumber = serialNumber.Trim();

        if (serialNumber.Length > 100)
            throw new DomainException("Serial number cannot exceed 100 characters");

        return new SerialNumber(serialNumber);
    }

    /// <summary>
    /// Implicit conversion từ SerialNumber sang string
    /// </summary>
    public static implicit operator string(SerialNumber serialNumber) => serialNumber.Value;

    /// <summary>
    /// Explicit conversion từ string sang SerialNumber
    /// </summary>
    public static explicit operator SerialNumber(string serialNumber) => Create(serialNumber);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}