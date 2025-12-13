using FAM.Application.Auth.Services;
using FAM.Application.Auth.Shared;
using FAM.Domain.Authorization;
using FAM.Domain.Common.Base;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

/// <summary>
/// Controller for JWKS (JSON Web Key Set) endpoints
/// Provides public keys for JWT verification and admin key management
/// </summary>
[ApiController]
public class JwksController : BaseApiController
{
    private readonly ISigningKeyService _signingKeyService;
    private readonly ILogger<JwksController> _logger;

    public JwksController(ISigningKeyService signingKeyService, ILogger<JwksController> logger)
    {
        _signingKeyService = signingKeyService;
        _logger = logger;
    }

    #region Public Endpoints

    /// <summary>
    /// Get the JSON Web Key Set (JWKS) containing public keys for JWT verification
    /// This is a standard endpoint that should be publicly accessible
    /// </summary>
    [HttpGet(".well-known/jwks.json")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JwksDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<JwksDto>> GetJwks(CancellationToken cancellationToken)
    {
        JwksDto jwks = await _signingKeyService.GetJwksAsync(cancellationToken);
        return OkResponse(jwks);
    }

    /// <summary>
    /// Alternative JWKS endpoint under /api path
    /// </summary>
    [HttpGet("api/auth/jwks")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(JwksDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<JwksDto>> GetJwksAlternative(CancellationToken cancellationToken)
    {
        return await GetJwks(cancellationToken);
    }

    #endregion

    #region Admin Endpoints - Key Management

    /// <summary>
    /// Get all signing keys (admin only)
    /// </summary>
    [HttpGet("api/admin/signing-keys")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(IEnumerable<SigningKeyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SigningKeyResponse>>> GetAllKeys(CancellationToken cancellationToken)
    {
        IReadOnlyList<SigningKeyResponse> keys = await _signingKeyService.GetAllKeysAsync(cancellationToken);
        return OkResponse(keys);
    }

    /// <summary>
    /// Get a specific signing key by ID (admin only)
    /// </summary>
    [HttpGet("api/admin/signing-keys/{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(SigningKeyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SigningKeyResponse>> GetKeyById(long id, CancellationToken cancellationToken)
    {
        SigningKeyResponse? key = await _signingKeyService.GetKeyByIdAsync(id, cancellationToken);
        if (key == null)
            throw new NotFoundException(ErrorCodes.KEY_NOT_FOUND, "SigningKey", id);
        return OkResponse(key);
    }

    /// <summary>
    /// Generate a new signing key (admin only)
    /// </summary>
    [HttpPost("api/admin/signing-keys")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(SigningKeyResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        SigningKeyResponse? response = await _signingKeyService.GetKeyByIdAsync(key.Id, cancellationToken);
        _logger.LogInformation("Generated new signing key {KeyId}", key.KeyId);

        return CreatedAtAction(nameof(GetKeyById), new { id = key.Id }, response);
    }

    /// <summary>
    /// Rotate signing keys - generate new active key and deactivate/revoke old (admin only)
    /// </summary>
    [HttpPost("api/admin/signing-keys/rotate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(SigningKeyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

        SigningKeyResponse? response = await _signingKeyService.GetKeyByIdAsync(key.Id, cancellationToken);
        _logger.LogInformation("Rotated signing keys. New key: {KeyId}", key.KeyId);

        return OkResponse(response);
    }

    /// <summary>
    /// Activate a specific signing key (admin only)
    /// </summary>
    [HttpPost("api/admin/signing-keys/{id:long}/activate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> ActivateKey(long id, CancellationToken cancellationToken)
    {
        await _signingKeyService.ActivateKeyAsync(id, cancellationToken);
        _logger.LogInformation("Activated signing key {KeyId}", id);
        return OkResponse(new { message = "Key activated successfully" });
    }

    /// <summary>
    /// Deactivate a specific signing key (admin only)
    /// </summary>
    [HttpPost("api/admin/signing-keys/{id:long}/deactivate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeactivateKey(long id, CancellationToken cancellationToken)
    {
        await _signingKeyService.DeactivateKeyAsync(id, cancellationToken);
        _logger.LogInformation("Deactivated signing key {KeyId}", id);
        return OkResponse(new { message = "Key deactivated successfully" });
    }

    /// <summary>
    /// Revoke a signing key (admin only)
    /// Revoked keys cannot be used for signing or verification
    /// </summary>
    [HttpPost("api/admin/signing-keys/{id:long}/revoke")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    [HttpDelete("api/admin/signing-keys/{id:long}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteKey(long id, CancellationToken cancellationToken)
    {
        await _signingKeyService.DeleteKeyAsync(id, cancellationToken);
        _logger.LogInformation("Deleted signing key {KeyId}", id);
        return OkResponse(new { message = "Key deleted successfully" });
    }

    /// <summary>
    /// Get keys that are expiring soon (admin only)
    /// </summary>
    [HttpGet("api/admin/signing-keys/expiring")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(typeof(IEnumerable<SigningKeyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<SigningKeyResponse>>> GetExpiringKeys(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<SigningKeyResponse> keys =
            await _signingKeyService.GetExpiringKeysAsync(TimeSpan.FromDays(days), cancellationToken);
        return OkResponse(keys);
    }

    #endregion
}
