# Frontend URL Configuration

## Overview
The FAM backend now uses database-driven email templates with configurable frontend and backend URLs. This allows customization of links in emails (reset password, verify email, etc.) and API callbacks without code changes.

**Smart Port Handling**: The configuration system automatically handles ports based on environment:
- **Development**: Includes port numbers (e.g., `:8000`, `:8001`)
- **Production**: Uses standard HTTPS port 443 (no port in URL)

## Configuration Files

### 1. Environment Variables (.env)
```bash
# Backend Configuration
BACKEND_URL=http://localhost:8000

# Frontend Configuration
FRONTEND_URL=http://localhost
FRONTEND_PORT=8001
FRONTEND_RESET_PASSWORD_PATH=/reset-password
```

### 2. appsettings.json
```json
{
  "Backend": {
    "BaseUrl": "http://localhost",
    "Port": 8000
  },
  "Frontend": {
    "BaseUrl": "http://localhost",
    "Port": 8001,
    "ResetPasswordPath": "/reset-password",
    "VerifyEmailPath": "/verify-email",
    "LoginPath": "/login"
  }
}
```

### 3. appsettings.Development.json
```json
{
  "Backend": {
    "BaseUrl": "http://localhost",
    "Port": 8000
  },
  "Frontend": {
    "BaseUrl": "http://localhost",
    "Port": 8001,
    "ResetPasswordPath": "/reset-password"
  }
}
```

### 4. appsettings.Production.json (example)
```json
{
  "Backend": {
    "BaseUrl": "https://api.fam.yourdomain.com"
    // No Port property - uses default 443
  },
  "Frontend": {
    "BaseUrl": "https://fam.yourdomain.com"
    // No Port property - uses default 443
  }
}
```

## Environment-Specific URLs

### Development
```bash
BACKEND_URL=http://localhost:8000
FRONTEND_URL=http://localhost
FRONTEND_PORT=8001
```

### Staging
```bash
BACKEND_URL=https://api-staging.fam.yourdomain.com
FRONTEND_URL=https://staging.fam.yourdomain.com
```

### Production
```bash
BACKEND_URL=https://api.fam.yourdomain.com
FRONTEND_URL=https://fam.yourdomain.com
```

## Configuration Options Classes

Location: `src/FAM.Application/Settings/FrontendOptions.cs`

### BackendOptions
```csharp
public class BackendOptions
{
    public string BaseUrl { get; set; }      // e.g., "http://localhost" or "https://api.fam.com"
    public int? Port { get; set; }            // Optional: 8000 for dev, null for production

    public string GetBaseUrl()                // Returns: "http://localhost:8000" or "https://api.fam.com"
    public string GetApiUrl(string path)      // Returns: "{BaseUrl}/path"
}
```

### FrontendOptions
```csharp
public class FrontendOptions
{
    public string BaseUrl { get; set; }             // e.g., "http://localhost" or "https://fam.com"
    public int? Port { get; set; }                  // Optional: 8001 for dev, null for production
    public string ResetPasswordPath { get; set; }   // Default: "/reset-password"
    public string VerifyEmailPath { get; set; }     // Default: "/verify-email"
    public string LoginPath { get; set; }           // Default: "/login"

    public string GetResetPasswordUrl()             // Returns: "{BaseUrl}/reset-password"
    public string GetVerifyEmailUrl()               // Returns: "{BaseUrl}/verify-email"
    public string GetLoginUrl()                     // Returns: "{BaseUrl}/login"
}
```

## Port Handling Logic

The configuration classes automatically handle port numbers:

### With Separate Port Property (Recommended for Development)
```json
{
  "Frontend": {
    "BaseUrl": "http://localhost",
    "Port": 8001
  }
}
```
Result: `http://localhost:8001`

### With Inline Port (Alternative)
```json
{
  "Frontend": {
    "BaseUrl": "http://localhost:8001"
  }
}
```
Result: `http://localhost:8001`

### Without Port (Production)
```json
{
  "Frontend": {
    "BaseUrl": "https://fam.yourdomain.com"
  }
}
```
Result: `https://fam.yourdomain.com` (uses default port 443)

### Smart Detection
The `GetBaseUrl()` method checks if:
1. Port property is set
2. URL doesn't already contain a port
3. If both conditions are true, it adds the port

This prevents double-port issues like `http://localhost:8001:8001`

## Environment Variable Overrides

FrontendOptions supports environment variable overrides in `Program.cs`:

```csharp
builder.Services.Configure<FrontendOptions>(options =>
{
    // Bind from appsettings first
    builder.Configuration.GetSection(FrontendOptions.SectionName).Bind(options);

    // Override with environment variables if present
    var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL");
    if (!string.IsNullOrEmpty(frontendUrl))
    {
        // Parse URL and extract port if present
        if (Uri.TryCreate(frontendUrl, UriKind.Absolute, out var uri))
        {
            options.BaseUrl = $"{uri.Scheme}://{uri.Host}";
            if (uri.Port != 80 && uri.Port != 443)
            {
                options.Port = uri.Port;
            }
            else
            {
                options.Port = null;
            }
        }
    }

    // Override port separately if specified
    var frontendPort = Environment.GetEnvironmentVariable("FRONTEND_PORT");
    if (!string.IsNullOrEmpty(frontendPort) && int.TryParse(frontendPort, out var port))
    {
        options.Port = port;
    }
});
```

## Usage in Code

### Before (Hardcoded)
```csharp
var resetUrl = "https://fam.yourdomain.com/reset-password";

await _emailService.SendPasswordResetEmailAsync(
    user.Email.Value,
    resetToken,
    user.Username.Value,
    resetUrl,
    cancellationToken);
```

### After (Configuration-based)
```csharp
public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResponse>
{
    private readonly FrontendOptions _frontendOptions;

    public ForgotPasswordCommandHandler(
        IOptions<FrontendOptions> frontendOptions)
    {
        _frontendOptions = frontendOptions.Value;
    }

    public async Task<ForgotPasswordResponse> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // Get reset URL from configuration
        var resetUrl = _frontendOptions.GetResetPasswordUrl();

        await _emailService.SendPasswordResetEmailAsync(
            user.Email.Value,
            resetToken,
            user.Username.Value,
            resetUrl,
            cancellationToken);
    }
}
```

## Email Links Generated

### Reset Password Email
- URL: `{BaseUrl}/reset-password?token={resetToken}&email={userEmail}`
- Example: `http://localhost:8001/reset-password?token=abc123&email=user@example.com`

### Verify Email (Future)
- URL: `{BaseUrl}/verify-email?token={verifyToken}`
- Example: `http://localhost:8001/verify-email?token=xyz789`

## Configuration Priority

1. **Environment Variables (.env)** - Highest priority
   - `FRONTEND_URL` and `FRONTEND_PORT` override everything
   - Used for sensitive/environment-specific values

2. **appsettings.{Environment}.json** - Medium priority
   - Environment-specific overrides (Development, Staging, Production)

3. **appsettings.json** - Lowest priority
   - Default/fallback values

## Testing

### 1. Check Configuration Loading
```bash
# Start the application
dotnet run --project src/FAM.WebApi

# Verify in logs that Frontend configuration is loaded
```

### 2. Test Password Reset Email
```bash
# Call forgot-password endpoint
curl -X POST http://localhost:8000/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email": "test@example.com"}'

# Check email contains correct frontend URL: http://localhost:8001/reset-password
```

### 3. Verify Different Environments
```bash
# Development (default)
ASPNETCORE_ENVIRONMENT=Development dotnet run
# Should use: http://localhost:8001

# Production
ASPNETCORE_ENVIRONMENT=Production dotnet run
# Should use: https://fam.yourdomain.com (if configured)
```

## Troubleshooting

### Issue: Email links point to localhost:3000 in production
**Solution:** Check `.env` file has correct `FRONTEND_URL` for production environment

### Issue: Email links have wrong path
**Solution:** Verify `FRONTEND_RESET_PASSWORD_PATH` in `.env` or `appsettings.json`

### Issue: Email links have wrong port
**Solution:** Check `FRONTEND_PORT` in `.env` or `Port` in `appsettings.json`

### Issue: Frontend URL not loading
**Solution:** Check `Program.cs` has `FrontendOptions` registered with environment variable overrides

## Future Enhancements

- [ ] Add `VerifyEmailPath` for email verification
- [ ] Add `ActivateAccountPath` for account activation
- [ ] Add `InviteUserPath` for user invitations
- [ ] Support multiple frontend URLs (mobile app, web app, admin portal)
- [ ] Add frontend URL validation in configuration

## Related Files

- `src/FAM.Application/Settings/FrontendOptions.cs` - Options classes
- `src/FAM.Application/Auth/ForgotPassword/ForgotPasswordCommandHandler.cs` - Usage example
- `src/FAM.WebApi/Program.cs` - Configuration registration with env overrides
- `.env` - Environment variables
- `appsettings.json` - Default configuration
- `appsettings.Development.json` - Development overrides</content>
<parameter name="filePath">/Users/VanAnh/WorkSpace/Personal/fixed-asset-management/fam-be/docs/FRONTEND_URL_CONFIGURATION.md