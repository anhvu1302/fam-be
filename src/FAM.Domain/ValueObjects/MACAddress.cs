using System.Text.RegularExpressions;
using FAM.Domain.Common.Base;

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
            throw new DomainException(ErrorCodes.VO_MAC_EMPTY);

        macAddress = macAddress.Trim().ToUpperInvariant();

        var format = DetermineFormat(macAddress);
        if (format == MACAddressFormat.Invalid)
            throw new DomainException(ErrorCodes.VO_MAC_INVALID);

        return new MACAddress(macAddress, format);
    }

    private static MACAddressFormat DetermineFormat(string mac)
    {
        // Remove separators for validation
        var cleanMac = mac.Replace(":", "").Replace("-", "").Replace(".", "");

        // Must be exactly 12 hexadecimal characters
        if (cleanMac.Length != 12 || !Regex.IsMatch(cleanMac, "^[0-9A-F]{12}$"))
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

    public bool IsMulticast()
    {
        return Value.Length >= 2 && (Value[1] == '1' || Value[1] == '3' || Value[1] == '5' || Value[1] == '7' ||
                                     Value[1] == '9' || Value[1] == 'B' || Value[1] == 'D' || Value[1] == 'F');
    }

    public bool IsUnicast()
    {
        return !IsMulticast();
    }

    public bool IsLocallyAdministered()
    {
        return Value.Length >= 2 && (Value[1] == '2' || Value[1] == '6' || Value[1] == 'A' || Value[1] == 'E');
    }

    public string GetNormalizedFormat()
    {
        var clean = Value.Replace(":", "").Replace("-", "").Replace(".", "");
        return string.Join(":", Enumerable.Range(0, 6).Select(i => clean.Substring(i * 2, 2)));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value.ToUpperInvariant();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(MACAddress macAddress)
    {
        return macAddress.Value;
    }
}

public enum MACAddressFormat
{
    Invalid,
    ColonSeparated, // 00:11:22:33:44:55
    HyphenSeparated, // 00-11-22-33-44-55
    DotSeparated, // 0011.2233.4455
    NoSeparators // 001122334455
}