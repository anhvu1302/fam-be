using FAM.Application.Auth.Commands;
using FAM.Application.Auth.DTOs;
using FAM.Domain.Abstractions;
using MediatR;
using OtpNet;

namespace FAM.Application.Auth.Handlers;

public sealed class Enable2FACommandHandler : IRequestHandler<Enable2FACommand, Enable2FAResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public Enable2FACommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Enable2FAResponse> Handle(Enable2FACommand request, CancellationToken cancellationToken)
    {
        // Get user by ID
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) throw new UnauthorizedAccessException("User not found");

        // Verify password
        if (!user.Password.Verify(request.Password)) throw new UnauthorizedAccessException("Invalid password");

        // Generate new secret key (32 bytes = 256 bits for enhanced security)
        var secretKey = KeyGeneration.GenerateRandomKey(32);
        var base32Secret = Base32Encoding.ToString(secretKey);

        // Create QR code URI for authenticator apps
        // Format: otpauth://totp/{Issuer}:{AccountName}?secret={Secret}&issuer={Issuer}
        var issuer = "FAM"; // Fixed Asset Management
        var accountName = user.Email.Value;
        var qrCodeUri =
            $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountName)}?secret={base32Secret}&issuer={Uri.EscapeDataString(issuer)}";

        // Format manual entry key for better readability (groups of 4 characters)
        var manualEntryKey = FormatSecretKey(base32Secret);

        return new Enable2FAResponse
        {
            Secret = base32Secret,
            QrCodeUri = qrCodeUri,
            ManualEntryKey = manualEntryKey
        };
    }

    private static string FormatSecretKey(string secret)
    {
        // Format: XXXX XXXX XXXX XXXX ...
        var formatted = "";
        for (var i = 0; i < secret.Length; i += 4)
        {
            if (i > 0) formatted += " ";
            formatted += secret.Substring(i, Math.Min(4, secret.Length - i));
        }

        return formatted;
    }
}