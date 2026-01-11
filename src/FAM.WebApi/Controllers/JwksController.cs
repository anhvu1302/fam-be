using System.Security.Cryptography;

using FAM.Domain.Abstractions;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;
using FAM.WebApi.Contracts.Auth;
using FAM.WebApi.Contracts.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Controller for JWKS (JSON Web Key Set) endpoints
/// Provides public keys for JWT verification and admin key management
/// </summary>
[ApiController]
public class JwksController : BaseApiController
{
    private readonly ISigningKeyRepository _signingKeyRepository;
    private readonly ISigningKeyService _signingKeyService;
    private readonly ILogger<JwksController> _logger;

    public JwksController(
        ISigningKeyRepository signingKeyRepository,
        ISigningKeyService signingKeyService,
        ILogger<JwksController> logger)
    {
        _signingKeyRepository = signingKeyRepository;
        _signingKeyService = signingKeyService;
        _logger = logger;
    }

    #region Public Endpoints

    /// <summary>
    /// Get the JSON Web Key Set (JWKS) containing public keys for JWT verification
    /// This is a standard endpoint that should be publicly accessible
    /// </summary>
    /// <remarks>
    /// Returns the JWKS containing all active (non-revoked, non-expired) public keys.
    /// Clients use these keys to verify JWT tokens issued by this server.
    /// </remarks>
    /// <response code="200">Success - Returns {success: true, result: JwksDto}</response>
    [HttpGet(".well-known/jwks.json")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<List<JwkDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<JwkDto>>> GetJwks(CancellationToken cancellationToken)
    {
        IEnumerable<SigningKey> keys = await _signingKeyRepository.GetAllAsync(cancellationToken);
        List<JwkDto> jwks = new();

        foreach (SigningKey key in keys)
        {
            if (key.IsRevoked || key.IsExpired())
            {
                continue;
            }

            RSA rsa = RSA.Create();
            rsa.ImportFromPem(key.PublicKey.AsSpan());
            RSAParameters rsaParams = rsa.ExportParameters(false);

            jwks.Add(new JwkDto
            {
                Kid = key.KeyId,
                Alg = key.Algorithm,
                N = Base64UrlEncoder.Encode(rsaParams.Modulus!),
                E = Base64UrlEncoder.Encode(rsaParams.Exponent!)
            });
        }

        return OkResponse(jwks);
    }

    /// <summary>
    /// Alternative JWKS endpoint under /api path
    /// </summary>
    /// <remarks>
    /// Same as /.well-known/jwks.json but under /api path for convenience.
    /// </remarks>
    /// <response code="200">Success - Returns {success: true, result: JwksDto}</response>
    [HttpGet("api/auth/jwks")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiSuccessResponse<List<JwkDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<JwkDto>>> GetJwksAlternative(CancellationToken cancellationToken)
    {
        return await GetJwks(cancellationToken);
    }

    #endregion

    #region Admin Endpoints - Key Management

    /// <summary>
    /// Get all signing keys (admin only)
    /// </summary>
    /// <remarks>
    /// Returns a list of all signing keys including their metadata.
    /// Admin and SuperAdmin roles only.
    /// </remarks>
    /// <response code="200">Success - Returns {success: true, result: [SigningKeyResponse, ...]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: string, code: string}]}</response>
    [HttpGet("api/admin/signing-keys")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<SigningKeyResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SigningKeyResponse>>> GetAllKeys(CancellationToken cancellationToken)
    {
        IEnumerable<SigningKey> keys = await _signingKeyRepository.GetAllAsync(cancellationToken);
        IEnumerable<SigningKeyResponse> responses = keys.ToSigningKeyResponses();
        return OkResponse(responses);
    }

    /// <summary>
    /// Get a specific signing key by ID (admin only)
    /// </summary>
    /// <remarks>
    /// Retrieves a single signing key with all its details and metadata.
    /// Admin and SuperAdmin roles only.
    /// </remarks>
    /// <param name="id">Signing key ID</param>
    /// <response code="200">Success - Returns {success: true, result: SigningKeyResponse}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="404">Not found - Returns {success: false, errors: [{message: "Key not found", code: "KEY_NOT_FOUND"}]}</response>
    [HttpGet("api/admin/signing-keys/{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<SigningKeyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SigningKeyResponse>> GetKeyById(long id, CancellationToken cancellationToken)
    {
        SigningKey? key = await _signingKeyRepository.GetByIdAsync(id, cancellationToken);
        if (key == null)
        {
            throw new NotFoundException(ErrorCodes.KEY_NOT_FOUND, "SigningKey", id);
        }

        return OkResponse(key.ToSigningKeyResponse());
    }

    /// <summary>
    /// Generate a new signing key (admin only)
    /// </summary>
    /// <remarks>
    /// Creates a new signing key with specified algorithm and key size.
    /// Can be immediately activated or held for later activation.
    /// Admin and SuperAdmin roles only.
    /// </remarks>
    /// <param name="request">GenerateSigningKeyRequest with algorithm, keySize, expiresAt, description</param>
    /// <response code="201">Created - Returns {success: true, result: SigningKeyResponse}</response>
    /// <response code="400">Bad request - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: string, code: string}]}</response>
    [HttpPost("api/admin/signing-keys")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<SigningKeyResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SigningKeyResponse>> GenerateKey(
        [FromBody] GenerateSigningKeyRequest request,
        CancellationToken cancellationToken)
    {
        SigningKey key = await _signingKeyService.GenerateKeyAsync(
            request.Algorithm,
            request.KeySize,
            request.ExpiresAt,
            request.Description,
            request.ActivateImmediately,
            cancellationToken);

        SigningKeyResponse response = key.ToSigningKeyResponse();

        return CreatedAtAction(nameof(GetKeyById), new { id = key.Id }, response);
    }

    /// <summary>
    /// Rotate signing keys - generate new active key and deactivate/revoke old (admin only)
    /// </summary>
    /// <remarks>
    /// Performs key rotation: generates a new signing key and optionally revokes the old one.
    /// This is critical for maintaining security when keys need to be updated.
    /// Admin and SuperAdmin roles only.
    /// </remarks>
    /// <param name="request">RotateKeyRequest with algorithm, keySize, revokeOldKey flag</param>
    /// <response code="200">Success - Returns {success: true, result: SigningKeyResponse}</response>
    /// <response code="400">Bad request - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: string, code: string}]}</response>
    [HttpPost("api/admin/signing-keys/rotate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<SigningKeyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SigningKeyResponse>> RotateKey(
        [FromBody] RotateKeyRequest request,
        CancellationToken cancellationToken)
    {
        SigningKey key = await _signingKeyService.RotateKeyAsync(
            request.Algorithm,
            request.KeySize,
            request.ExpiresAt,
            request.Description,
            request.RevokeOldKey,
            request.RevocationReason,
            cancellationToken);

        SigningKeyResponse response = key.ToSigningKeyResponse();

        return OkResponse(response);
    }

    /// <summary>
    /// Activate a specific signing key (admin only)
    /// </summary>
    /// <remarks>
    /// Activates an inactive signing key, making it available for token signing.
    /// Admin and SuperAdmin roles only.
    /// </remarks>
    /// <param name="id">Signing key ID to activate</param>
    /// <response code="200">Success - Returns {success: true, result: {message: "Key activated successfully"}}</response>
    /// <response code="400">Bad request - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="404">Not found - Returns {success: false, errors: [{message: "Key not found", code: "KEY_NOT_FOUND"}]}</response>
    [HttpPost("api/admin/signing-keys/{id:long}/activate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<dynamic>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ActivateKey(long id, CancellationToken cancellationToken)
    {
        await _signingKeyService.ActivateKeyAsync(id, cancellationToken);
        return OkResponse(new { message = "Key activated successfully" });
    }

    /// <summary>
    /// Deactivate a specific signing key (admin only)
    /// </summary>
    /// <remarks>
    /// Deactivates an active signing key, preventing it from being used for new token signing.
    /// Existing tokens signed with this key remain valid.
    /// Admin and SuperAdmin roles only.
    /// </remarks>
    /// <param name="id">Signing key ID to deactivate</param>
    /// <response code="200">Success - Returns {success: true, result: {message: "Key deactivated successfully"}}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="404">Not found - Returns {success: false, errors: [{message: "Key not found", code: "KEY_NOT_FOUND"}]}</response>
    [HttpPost("api/admin/signing-keys/{id:long}/deactivate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<dynamic>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeactivateKey(long id, CancellationToken cancellationToken)
    {
        await _signingKeyService.DeactivateKeyAsync(id, cancellationToken);
        return OkResponse(new { message = "Key deactivated successfully" });
    }

    /// <summary>
    /// Revoke a signing key (admin only)
    /// Revoked keys cannot be used for signing or verification
    /// </summary>
    /// <remarks>
    /// Permanently revokes a signing key with a reason recorded.
    /// Revoked keys cannot be used for token signing or verification.
    /// Admin and SuperAdmin roles only.
    /// </remarks>
    /// <param name="id">Signing key ID to revoke</param>
    /// <param name="request">RevokeSigningKeyRequest with reason</param>
    /// <response code="200">Success - Returns {success: true, result: {message: "Key revoked successfully"}}</response>
    /// <response code="400">Bad request - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="404">Not found - Returns {success: false, errors: [{message: "Key not found", code: "KEY_NOT_FOUND"}]}</response>
    [HttpPost("api/admin/signing-keys/{id:long}/revoke")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<dynamic>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RevokeKey(
        long id,
        [FromBody] RevokeSigningKeyRequest request,
        CancellationToken cancellationToken)
    {
        await _signingKeyService.RevokeKeyAsync(id, request.Reason, cancellationToken);
        _logger.LogWarning("Revoked signing key {KeyId}. Reason: {Reason}", id, request.Reason ?? "Not specified");
        return OkResponse(new { message = "Key revoked successfully" });
    }

    /// <summary>
    /// Delete a revoked signing key permanently (admin only)
    /// </summary>
    /// <remarks>
    /// Permanently deletes a revoked signing key from the system.
    /// Only revoked keys can be deleted.
    /// Admin and SuperAdmin roles only.
    /// </remarks>
    /// <param name="id">Signing key ID to delete</param>
    /// <response code="200">Success - Returns {success: true, result: {message: "Key deleted successfully"}}</response>
    /// <response code="400">Bad request - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="404">Not found - Returns {success: false, errors: [{message: "Key not found", code: "KEY_NOT_FOUND"}]}</response>
    [HttpDelete("api/admin/signing-keys/{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<dynamic>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteKey(long id, CancellationToken cancellationToken)
    {
        await _signingKeyService.DeleteKeyAsync(id, cancellationToken);
        return OkResponse(new { message = "Key deleted successfully" });
    }

    /// <summary>
    /// Get keys that are expiring soon (admin only)
    /// </summary>
    /// <remarks>
    /// Returns all signing keys that will expire within the specified number of days.
    /// Helps identify keys that need to be rotated soon.
    /// Admin and SuperAdmin roles only.
    /// </remarks>
    /// <param name="days">Number of days to look ahead (default: 7)</param>
    /// <response code="200">Success - Returns {success: true, result: [SigningKeyResponse, ...]}</response>
    /// <response code="401">Unauthorized - Returns {success: false, errors: [{message: string, code: string}]}</response>
    /// <response code="403">Forbidden - Returns {success: false, errors: [{message: string, code: string}]}</response>
    [HttpGet("api/admin/signing-keys/expiring")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(ApiSuccessResponse<IEnumerable<SigningKeyResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SigningKeyResponse>>> GetExpiringKeys(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SigningKey> keys =
            await _signingKeyRepository.GetKeysExpiringWithinAsync(TimeSpan.FromDays(days), cancellationToken);
        IEnumerable<SigningKeyResponse> responses = keys.ToSigningKeyResponses();
        return OkResponse(responses);
    }

    #endregion
}
