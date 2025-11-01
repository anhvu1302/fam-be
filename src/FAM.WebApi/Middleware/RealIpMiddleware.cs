using System.Net;

namespace FAM.WebApi.Middleware;

/// <summary>
/// Middleware to extract and log the real client IP address
/// This helps with debugging and ensures proper IP detection across different deployment scenarios
/// </summary>
public class RealIpMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RealIpMiddleware> _logger;

    public RealIpMiddleware(RequestDelegate next, ILogger<RealIpMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        
        // Add the real IP to HttpContext.Items for easy access throughout the request pipeline
        context.Items["RealClientIp"] = clientIp;
        
        // Log IP for debugging (remove in production or use debug level)
        _logger.LogDebug("Request from IP: {IpAddress}, Path: {Path}", clientIp, context.Request.Path);
        
        await _next(context);
    }

    private string GetClientIpAddress(HttpContext context)
    {
        // Priority order for getting real client IP:
        // 1. CF-Connecting-IP (Cloudflare)
        // 2. True-Client-IP (Cloudflare Enterprise)
        // 3. X-Real-IP (Nginx proxy)
        // 4. X-Forwarded-For (Standard proxy header)
        // 5. X-Client-IP
        // 6. RemoteIpAddress (Direct connection)

        // Cloudflare header (most reliable when using Cloudflare)
        var cfConnectingIp = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cfConnectingIp) && IsValidIp(cfConnectingIp))
        {
            return cfConnectingIp.Trim();
        }

        // Cloudflare Enterprise header
        var trueClientIp = context.Request.Headers["True-Client-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(trueClientIp) && IsValidIp(trueClientIp))
        {
            return trueClientIp.Trim();
        }

        // X-Real-IP from Nginx or other reverse proxy
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp) && IsValidIp(realIp))
        {
            return realIp.Trim();
        }

        // X-Forwarded-For (can contain multiple IPs: client, proxy1, proxy2)
        // Take the first IP which is the original client
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var ip in ips)
            {
                if (IsValidIp(ip) && !IsPrivateIp(ip))
                {
                    return ip;
                }
            }
        }

        // X-Client-IP header
        var clientIp = context.Request.Headers["X-Client-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(clientIp) && IsValidIp(clientIp))
        {
            return clientIp.Trim();
        }

        // Fallback to RemoteIpAddress (direct connection, no proxy)
        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp != null)
        {
            // Handle IPv6 loopback (::1) and map it to IPv4
            if (remoteIp.IsIPv4MappedToIPv6)
            {
                remoteIp = remoteIp.MapToIPv4();
            }
            
            // Convert IPv6 loopback to IPv4
            if (remoteIp.ToString() == "::1")
            {
                return "127.0.0.1";
            }

            return remoteIp.ToString();
        }

        return "Unknown";
    }

    private bool IsValidIp(string ip)
    {
        return IPAddress.TryParse(ip, out _);
    }

    private bool IsPrivateIp(string ip)
    {
        if (!IPAddress.TryParse(ip, out var address))
            return false;

        // Check for private IP ranges
        var bytes = address.GetAddressBytes();
        
        // IPv4 private ranges:
        // 10.0.0.0 - 10.255.255.255
        // 172.16.0.0 - 172.31.255.255
        // 192.168.0.0 - 192.168.255.255
        // 127.0.0.0 - 127.255.255.255 (loopback)
        
        if (bytes.Length == 4)
        {
            return bytes[0] == 10 
                || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
                || (bytes[0] == 192 && bytes[1] == 168)
                || bytes[0] == 127;
        }

        // IPv6 loopback
        if (address.IsIPv6LinkLocal || address.ToString() == "::1")
            return true;

        return false;
    }
}
