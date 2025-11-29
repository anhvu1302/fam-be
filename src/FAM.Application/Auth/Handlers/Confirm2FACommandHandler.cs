using FAM.Application.Auth.Commands;
using FAM.Application.Auth.DTOs;
using FAM.Domain.Abstractions;
using MediatR;
using OtpNet;
using System.Security.Cryptography;

namespace FAM.Application.Auth.Handlers;

public sealed class Confirm2FACommandHandler : IRequestHandler<Confirm2FACommand, Confirm2FAResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private const int BackupCodeCount = 16; // 16 codes like GitHub
    private const int BackupCodeLength = 10; // 10 characters (5-5 format)

    public Confirm2FACommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Confirm2FAResponse> Handle(Confirm2FACommand request, CancellationToken cancellationToken)
    {
        // Get user by ID
        var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null) throw new UnauthorizedAccessException("User not found");

        // Verify the TOTP code with the provided secret
        var secretBytes = Base32Encoding.ToBytes(request.Secret);
        var totp = new Totp(secretBytes);

        // Verify code with time window tolerance (1 step = 30 seconds)
        // This allows for slight clock skew between server and client
        var verificationWindow = new VerificationWindow(1, 1);

        if (!totp.VerifyTotp(request.Code, out _, verificationWindow))
            throw new InvalidOperationException("Invalid verification code");

        // Generate backup codes
        var backupCodes = GenerateBackupCodes();

        // Hash backup codes before storing (for security)
        var hashedBackupCodes = backupCodes.Select(code => BCrypt.Net.BCrypt.HashPassword(code)).ToList();
        var backupCodesJson = System.Text.Json.JsonSerializer.Serialize(hashedBackupCodes);

        // Code is valid - enable 2FA and save the secret with backup codes
        user.EnableTwoFactor(request.Secret, backupCodesJson);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new Confirm2FAResponse
        {
            Success = true,
            BackupCodes = backupCodes,
            Message =
                "Two-factor authentication has been enabled successfully. Please save your backup codes in a secure location. Each code can only be used once for account recovery."
        };
    }

    /// <summary>
    /// Generate secure random backup codes
    /// </summary>
    private static List<string> GenerateBackupCodes()
    {
        var codes = new List<string>();

        for (var i = 0; i < BackupCodeCount; i++)
        {
            var code = GenerateSecureCode(BackupCodeLength);
            codes.Add(code);
        }

        return codes;
    }

    /// <summary>
    /// Generate a cryptographically secure random backup code
    /// Format: xxxxx-xxxxx (10 hex characters with dash, like GitHub)
    /// Example: 881eb-53018, cad57-19be7, c3481-cde7f
    /// </summary>
    private static string GenerateSecureCode(int length)
    {
        // Use lowercase hex characters only (like GitHub)
        const string chars = "0123456789abcdef";
        var result = new char[length];

        for (var i = 0; i < length; i++) result[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];

        // Format: xxxxx-xxxxx (5-5 format with dash)
        var code = new string(result);
        return $"{code.Substring(0, 5)}-{code.Substring(5, 5)}";
    }
}