using FAM.Domain.Common;

namespace FAM.Domain.ValueObjects;

/// <summary>
/// Value Object cho địa chỉ IP
/// </summary>
public sealed class IPAddress : ValueObject
{
    public string Value { get; }
    public IPAddressType Type { get; }

    private IPAddress(string value, IPAddressType type)
    {
        Value = value;
        Type = type;
    }

    public static IPAddress Create(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            throw new DomainException("IP address is required");

        ipAddress = ipAddress.Trim();

        // Handle "Unknown" IP (e.g., in testing or when IP detection fails)
        if (ipAddress.Equals("Unknown", StringComparison.OrdinalIgnoreCase))
        {
            // Default to localhost for unknown IPs
            ipAddress = "127.0.0.1";
        }

        var type = DetermineType(ipAddress);
        if (type == IPAddressType.Invalid)
            throw new DomainException($"Invalid IP address format: {ipAddress}");

        return new IPAddress(ipAddress, type);
    }

    private static IPAddressType DetermineType(string ip)
    {
        if (System.Net.IPAddress.TryParse(ip, out var parsed))
        {
            return parsed.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork
                ? IPAddressType.IPv4
                : IPAddressType.IPv6;
        }
        return IPAddressType.Invalid;
    }

    public bool IsPrivate()
    {
        if (Type != IPAddressType.IPv4)
            return false;

        var parts = Value.Split('.');
        if (parts.Length != 4)
            return false;

        // 10.0.0.0 - 10.255.255.255
        if (parts[0] == "10")
            return true;

        // 172.16.0.0 - 172.31.255.255
        if (parts[0] == "172" && int.TryParse(parts[1], out var second) && second >= 16 && second <= 31)
            return true;

        // 192.168.0.0 - 192.168.255.255
        if (parts[0] == "192" && parts[1] == "168")
            return true;

        return false;
    }

    public bool IsLoopback() => Value == "127.0.0.1" || Value == "::1";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
        yield return Type;
    }

    public override string ToString() => Value;

    public static implicit operator string(IPAddress ipAddress) => ipAddress.Value;
}

public enum IPAddressType
{
    Invalid,
    IPv4,
    IPv6
}
