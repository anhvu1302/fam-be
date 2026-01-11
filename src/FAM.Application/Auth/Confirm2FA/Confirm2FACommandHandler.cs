using System.Security.Cryptography;
using System.Text.Json;

using FAM.Application.Auth.Shared;
using FAM.Domain.Abstractions;
using FAM.Domain.Common.Base;
using FAM.Domain.Users;

using MediatR;

using OtpNet;

namespace FAM.Application.Auth.Confirm2FA;

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
        User? user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedException(ErrorCodes.USER_NOT_FOUND);
        }

        // Security: Verify the secret matches the pending secret (from Enable2FA)
        if (!user.IsPendingTwoFactorSecretValid(request.Secret))
        {
            throw new InvalidOperationException(
                "Invalid or expired 2FA secret. Please generate a new one by calling Enable2FA.");
        }

        // Verify the TOTP code with the provided secret
        byte[]? secretBytes = Base32Encoding.ToBytes(request.Secret);
        Totp totp = new(secretBytes);

        VerificationWindow verificationWindow = new(0, 0);

        if (!totp.VerifyTotp(request.Code, out _, verificationWindow))
        {
            throw new InvalidOperationException("Invalid verification code");
        }

        // Generate backup codes
        List<string> backupCodes = GenerateBackupCodes();

        // Hash backup codes before storing (for security)
        List<string> hashedBackupCodes = backupCodes.Select(code => BCrypt.Net.BCrypt.HashPassword(code)).ToList();
        string backupCodesJson = JsonSerializer.Serialize(hashedBackupCodes);

        // Code is valid - enable 2FA and save the secret with backup codes
        user.EnableTwoFactor(request.Secret, backupCodesJson);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new Confirm2FAResponse
        {
            BackupCodes = backupCodes
        };
    }

    /// <summary>
    /// Generate secure random backup codes
    /// </summary>
    private static List<string> GenerateBackupCodes()
    {
        List<string> codes = new();

        for (int i = 0; i < BackupCodeCount; i++)
        {
            string code = GenerateSecureCode(BackupCodeLength);
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
        char[] result = new char[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = chars[RandomNumberGenerator.GetInt32(chars.Length)];
        }

        // Format: xxxxx-xxxxx (5-5 format with dash)
        string code = new(result);
        return $"{code.Substring(0, 5)}-{code.Substring(5, 5)}";
    }
}
