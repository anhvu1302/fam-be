using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Value Object cho địa chỉ MAC
/// </summary>
public sealed class MACAddress : ValueObject
{
    public string Value { get; }
    public MACAddressFormat Format { get; }

    private MACAddress(string value, MACAddressFormat format)
    {
        Value = value;
        Format = format;
    }

    public static MACAddress Create(string macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
            throw new DomainException("MAC address is required");

        macAddress = macAddress.Trim().ToUpperInvariant();

        var format = DetermineFormat(macAddress);
        if (format == MACAddressFormat.Invalid)
            throw new DomainException($"Invalid MAC address format: {macAddress}");

        return new MACAddress(macAddress, format);
    }

    private static MACAddressFormat DetermineFormat(string mac)
    {
        // Remove separators for validation
        var cleanMac = mac.Replace(":", "").Replace("-", "").Replace(".", "");

        // Must be exactly 12 hexadecimal characters
        if (cleanMac.Length != 12 || !System.Text.RegularExpressions.Regex.IsMatch(cleanMac, "^[0-9A-F]{12}$"))
            return MACAddressFormat.Invalid;

        // Determine format based on separators
        if (mac.Contains(":") && mac.Split(':').Length == 6)
            return MACAddressFormat.ColonSeparated; // 00:11:22:33:44:55

        if (mac.Contains("-") && mac.Split('-').Length == 6)
            return MACAddressFormat.HyphenSeparated; // 00-11-22-33-44-55

        if (mac.Contains(".") && mac.Split('.').Length == 3)
            return MACAddressFormat.DotSeparated; // 0011.2233.4455

        if (!mac.Contains(":") && !mac.Contains("-") && !mac.Contains("."))
            return MACAddressFormat.NoSeparators; // 001122334455

        return MACAddressFormat.Invalid;
    }

    public bool IsMulticast() => Value.Length >= 2 && (Value[1] == '1' || Value[1] == '3' || Value[1] == '5' || Value[1] == '7' || Value[1] == '9' || Value[1] == 'B' || Value[1] == 'D' || Value[1] == 'F');

    public bool IsUnicast() => !IsMulticast();

    public bool IsLocallyAdministered() => Value.Length >= 2 && (Value[1] == '2' || Value[1] == '6' || Value[1] == 'A' || Value[1] == 'E');

    public string GetNormalizedFormat()
    {
        var clean = Value.Replace(":", "").Replace("-", "").Replace(".", "");
        return string.Join(":", Enumerable.Range(0, 6).Select(i => clean.Substring(i * 2, 2)));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString() => Value;

    public static implicit operator string(MACAddress macAddress) => macAddress.Value;
}

public enum MACAddressFormat
{
    Invalid,
    ColonSeparated,    // 00:11:22:33:44:55
    HyphenSeparated,   // 00-11-22-33-44-55
    DotSeparated,      // 0011.2233.4455
    NoSeparators       // 001122334455
}