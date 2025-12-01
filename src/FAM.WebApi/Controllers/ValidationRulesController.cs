using FAM.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace FAM.WebApi.Controllers;

/// <summary>
/// API endpoint to expose validation rules to Frontend
/// This ensures Frontend can sync with Backend validation constants
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ValidationRulesController : ControllerBase
{
    /// <summary>
    /// Get all validation rules for frontend synchronization
    /// Frontend can use these constants to implement client-side validation
    /// that matches backend validation exactly
    /// </summary>
    /// <returns>Validation rules organized by domain</returns>
    [HttpGet]
    public IActionResult GetValidationRules()
    {
        return Ok(new
        {
            Username = new
            {
                MinLength = DomainRules.Username.MinLength,
                MaxLength = DomainRules.Username.MaxLength,
                Pattern = DomainRules.Username.Pattern,
                AllowedCharacters = DomainRules.Username.AllowedCharacters
            },
            Password = new
            {
                MinLength = DomainRules.Password.MinLength,
                MaxLength = DomainRules.Password.MaxLength,
                SpecialCharacters = DomainRules.Password.SpecialCharacters,
                Patterns = new
                {
                    Uppercase = DomainRules.Password.UppercasePattern,
                    Lowercase = DomainRules.Password.LowercasePattern,
                    Digit = DomainRules.Password.DigitPattern,
                    Special = DomainRules.Password.SpecialCharPattern
                }
            },
            Email = new
            {
                MaxLength = DomainRules.Email.MaxLength,
                Description = DomainRules.Email.Description
            },
            PhoneNumber = new
            {
                MinLength = DomainRules.PhoneNumber.MinLength,
                MaxLength = DomainRules.PhoneNumber.MaxLength,
                DefaultCountryCode = DomainRules.PhoneNumber.DefaultCountryCode,
                Description = DomainRules.PhoneNumber.Description
            },
            PostalCode = new
            {
                MaxLength = DomainRules.PostalCode.MaxLength,
                Description = DomainRules.PostalCode.Description
            },
            Address = new
            {
                StreetMaxLength = DomainRules.Address.StreetMaxLength,
                CityMaxLength = DomainRules.Address.CityMaxLength,
                CountryCodeLength = DomainRules.Address.CountryCodeLength,
                Description = DomainRules.Address.Description
            },
            DomainName = new
            {
                MaxLength = DomainRules.DomainName.MaxLength,
                Pattern = DomainRules.DomainName.Pattern,
                Description = DomainRules.DomainName.Description
            },
            RoleCode = new
            {
                MaxLength = DomainRules.RoleCode.MaxLength,
                Pattern = DomainRules.RoleCode.Pattern,
                AllowedCharacters = DomainRules.RoleCode.AllowedCharacters,
                Description = DomainRules.RoleCode.Description
            },
            TaxCode = new
            {
                MinLength = DomainRules.TaxCode.MinLength,
                MaxLength = DomainRules.TaxCode.MaxLength,
                Pattern = DomainRules.TaxCode.Pattern,
                AllowedCharacters = DomainRules.TaxCode.AllowedCharacters,
                Description = DomainRules.TaxCode.Description
            },
            Rating = new
            {
                MinValue = DomainRules.Rating.MinValue,
                MaxValue = DomainRules.Rating.MaxValue,
                Description = DomainRules.Rating.Description
            },
            Percentage = new
            {
                MinValue = DomainRules.Percentage.MinValue,
                MaxValue = DomainRules.Percentage.MaxValue,
                Description = DomainRules.Percentage.Description
            }
        });
    }

    /// <summary>
    /// Get validation rules for a specific domain
    /// </summary>
    /// <param name="domain">Domain name (e.g., "username", "password")</param>
    /// <returns>Validation rules for the specified domain</returns>
    [HttpGet("{domain}")]
    public IActionResult GetValidationRulesByDomain(string domain)
    {
        return domain.ToLower() switch
        {
            "username" => Ok(new
            {
                MinLength = DomainRules.Username.MinLength,
                MaxLength = DomainRules.Username.MaxLength,
                Pattern = DomainRules.Username.Pattern,
                AllowedCharacters = DomainRules.Username.AllowedCharacters
            }),
            "password" => Ok(new
            {
                MinLength = DomainRules.Password.MinLength,
                MaxLength = DomainRules.Password.MaxLength,
                SpecialCharacters = DomainRules.Password.SpecialCharacters,
                Patterns = new
                {
                    Uppercase = DomainRules.Password.UppercasePattern,
                    Lowercase = DomainRules.Password.LowercasePattern,
                    Digit = DomainRules.Password.DigitPattern,
                    Special = DomainRules.Password.SpecialCharPattern
                }
            }),
            "email" => Ok(new
            {
                MaxLength = DomainRules.Email.MaxLength,
                Description = DomainRules.Email.Description
            }),
            "phonenumber" or "phone" => Ok(new
            {
                MinLength = DomainRules.PhoneNumber.MinLength,
                MaxLength = DomainRules.PhoneNumber.MaxLength
            }),
            _ => NotFound(new { error = $"Validation rules for domain '{domain}' not found" })
        };
    }
}
