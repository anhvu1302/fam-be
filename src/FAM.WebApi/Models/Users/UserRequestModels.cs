using System.ComponentModel.DataAnnotations;

namespace FAM.WebApi.Models.Users;

/// <summary>
/// Create user request model - Web API shape validation only
/// </summary>
public class CreateUserRequestModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$",
        ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    public string Password { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Full name must not exceed 200 characters")]
    public string? FullName { get; set; }
}

/// <summary>
/// Update user request model
/// </summary>
public class UpdateUserRequestModel
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$",
        ErrorMessage = "Username can only contain letters, numbers, dots, underscores, and hyphens")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Email must be a valid email address")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    public string? Password { get; set; }

    [StringLength(200, ErrorMessage = "Full name must not exceed 200 characters")]
    public string? FullName { get; set; }
}

/// <summary>
/// Request to update user avatar with upload session
/// </summary>
public class UpdateAvatarRequest
{
    [Required(ErrorMessage = "Upload ID is required")]
    public string UploadId { get; set; } = string.Empty;
}