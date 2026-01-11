using System.Net.Mail;
using System.Text.RegularExpressions;

namespace FAM.Domain.Common.Base;

/// <summary>
/// Centralized domain validation rules.
/// Single source of truth for all business rules in the domain.
/// Other layers (Application, API) can reference these rules to avoid duplication.
/// </summary>
public static class DomainRules
{
    #region Username Rules

    public static class Username
    {
        public const int MinLength = 3;
        public const int MaxLength = 50;
        public const string Pattern = @"^[a-zA-Z0-9._-]+$";
        public const string Description = "Username can only contain letters, numbers, dots, underscores, and hyphens";
        public const string AllowedCharacters = "letters, numbers, dots (.), underscores (_), and hyphens (-)";

        /// <summary>
        /// Validates if username matches the allowed pattern
        /// </summary>
        public static bool IsValidFormat(string username)
        {
            return !string.IsNullOrWhiteSpace(username)
                   && Regex.IsMatch(username, Pattern);
        }

        /// <summary>
        /// Validates username length and format
        /// </summary>
        public static bool IsValid(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return false;
            }

            username = username.Trim();
            if (username.Length < MinLength || username.Length > MaxLength)
            {
                return false;
            }

            return IsValidFormat(username);
        }
    }

    #endregion

    #region Password Rules

    public static class Password
    {
        public const int MinLength = 8;
        public const int MaxLength = 128;

        // Regex patterns for validation
        public const string UppercasePattern = @"[A-Z]";
        public const string LowercasePattern = @"[a-z]";
        public const string DigitPattern = @"[0-9]";
        public const string SpecialCharPattern = @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]";

        // Display strings for user-friendly messages
        public const string SpecialCharacters = "!@#$%^&*()_+-=[]{}; ':\"\\|,.<>/?";
        public const string Description = "Password must contain uppercase, lowercase, digit, and special character";

        /// <summary>
        /// Validates if password contains at least one uppercase letter
        /// </summary>
        public static bool HasUppercase(string password)
        {
            return Regex.IsMatch(password, UppercasePattern);
        }

        /// <summary>
        /// Validates if password contains at least one lowercase letter
        /// </summary>
        public static bool HasLowercase(string password)
        {
            return Regex.IsMatch(password, LowercasePattern);
        }

        /// <summary>
        /// Validates if password contains at least one digit
        /// </summary>
        public static bool HasDigit(string password)
        {
            return Regex.IsMatch(password, DigitPattern);
        }

        /// <summary>
        /// Validates if password contains at least one special character
        /// </summary>
        public static bool HasSpecialChar(string password)
        {
            return Regex.IsMatch(password, SpecialCharPattern);
        }

        /// <summary>
        /// Validates password strength (all requirements)
        /// </summary>
        public static bool IsValid(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (password.Length < MinLength || password.Length > MaxLength)
            {
                return false;
            }

            return HasUppercase(password)
                   && HasLowercase(password)
                   && HasDigit(password)
                   && HasSpecialChar(password);
        }
    }

    #endregion

    #region Email Rules

    public static class Email
    {
        public const int MaxLength = 256;
        public const string Description = "Valid email address format";

        /// <summary>
        /// Validates email format using System.Net.Mail.MailAddress
        /// </summary>
        public static bool IsValidFormat(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                MailAddress addr = new(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    #endregion

    #region PhoneNumber Rules

    public static class PhoneNumber
    {
        public const int MinLength = 9;
        public const int MaxLength = 15;
        public const string DefaultCountryCode = "+84";
        public const string Description = "Phone number with 9-15 digits";

        /// <summary>
        /// Validates phone number length (digits only)
        /// </summary>
        public static bool IsValidLength(string phoneDigits)
        {
            if (string.IsNullOrWhiteSpace(phoneDigits))
            {
                return false;
            }

            int length = phoneDigits.Length;
            return length >= MinLength && length <= MaxLength;
        }
    }

    #endregion

    #region PostalCode Rules

    public static class PostalCode
    {
        public const int MaxLength = 10;
        public const string Description = "Valid postal code format";
    }

    #endregion

    #region Address Rules

    public static class Address
    {
        public const int StreetMaxLength = 200;
        public const int CityMaxLength = 100;
        public const int CountryCodeLength = 2;
        public const string Description = "Complete address information";
    }

    #endregion

    #region Domain Name Rules

    public static class DomainName
    {
        public const int MaxLength = 253;

        public const string Pattern =
            @"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)*[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?$";

        public const string Description = "Valid domain name format";

        /// <summary>
        /// Validates domain name format
        /// </summary>
        public static bool IsValidFormat(string domain)
        {
            return !string.IsNullOrWhiteSpace(domain)
                   && domain.Length <= MaxLength
                   && Regex.IsMatch(domain, Pattern);
        }
    }

    #endregion

    #region Role Code Rules

    public static class RoleCode
    {
        public const int MaxLength = 20;
        public const string Pattern = @"^[A-Z0-9_]+$";
        public const string Description = "Uppercase letters, numbers, and underscores only";
        public const string AllowedCharacters = "uppercase letters (A-Z), numbers (0-9), and underscores (_)";

        /// <summary>
        /// Validates role code format
        /// </summary>
        public static bool IsValidFormat(string code)
        {
            return !string.IsNullOrWhiteSpace(code)
                   && Regex.IsMatch(code, Pattern);
        }
    }

    #endregion

    #region Cost Center Rules

    public static class CostCenter
    {
        public const int MaxLength = 30;
        public const string Description = "Cost center code";
    }

    #endregion

    #region Asset Tag Rules

    public static class AssetTag
    {
        public const int MaxLength = 50;
        public const string Description = "Asset identification tag";
    }

    #endregion

    #region Serial Number Rules

    public static class SerialNumber
    {
        public const int MaxLength = 100;
        public const string Description = "Product serial number";
    }

    #endregion

    #region Tax Code Rules

    public static class TaxCode
    {
        public const int MinLength = 8;
        public const int MaxLength = 15;
        public const string Pattern = @"^[0-9\-]+$";
        public const string Description = "Tax identification number";
        public const string AllowedCharacters = "numbers (0-9) and hyphens (-)";

        /// <summary>
        /// Validates tax code format
        /// </summary>
        public static bool IsValidFormat(string taxCode)
        {
            return !string.IsNullOrWhiteSpace(taxCode)
                   && Regex.IsMatch(taxCode, Pattern);
        }
    }

    #endregion

    #region Resource Type Rules

    public static class ResourceType
    {
        public const int MaxLength = 50;
        public const string Description = "Resource type identifier";
    }

    #endregion

    #region Resource Action Rules

    public static class ResourceAction
    {
        public const int MaxLength = 50;
        public const string Description = "Resource action identifier";
    }

    #endregion

    #region Rating Rules

    public static class Rating
    {
        public const int MinValue = 1;
        public const int MaxValue = 5;
        public const string Description = "Rating from 1 to 5 stars";
    }

    #endregion

    #region Percentage Rules

    public static class Percentage
    {
        public const decimal MinValue = 0;
        public const decimal MaxValue = 100;
        public const string Description = "Percentage value from 0 to 100";
    }

    #endregion

    #region Money Rules

    public static class Money
    {
        public const decimal MinAmount = 0;
        public const string DefaultCurrency = "VND";
        public const string Description = "Monetary value with currency";
    }

    #endregion

    #region Dimensions Rules

    public static class Dimensions
    {
        public const decimal MinValue = 0;
        public static readonly string[] ValidUnits = { "cm", "m", "mm", "in", "ft" };
        public const string DefaultUnit = "cm";
        public const string Description = "Physical dimensions with unit";
    }

    #endregion

    #region Organization Rules

    public static class Organization
    {
        public const int NameMaxLength = 200;
        public const int CodeMaxLength = 50;
        public const string Description = "Organization information";
    }

    #endregion

    #region Department Rules

    public static class Department
    {
        public const int NameMaxLength = 200;
        public const int CodeMaxLength = 50;
        public const int MinHeadcount = 0;
        public const int MinBudgetYear = 1900;
        public const string Description = "Department information";
    }

    #endregion

    #region Asset Rules

    public static class Asset
    {
        public const int NameMaxLength = 200;
        public const int CodeMaxLength = 50;
        public const int DescriptionMaxLength = 2000;
        public const int MinMaintenanceIntervalDays = 1;
        public const string Description = "Fixed asset information";
    }

    #endregion

    #region Model Rules

    public static class Model
    {
        public const int NameMaxLength = 200;
        public const int ModelNumberMaxLength = 100;
        public const int DescriptionMaxLength = 2000;
        public const decimal MinWeight = 0;
        public const string Description = "Asset model information";
    }

    #endregion

    #region Category Rules

    public static class Category
    {
        public const int NameMaxLength = 200;
        public const int CodeMaxLength = 50;
        public const int DescriptionMaxLength = 1000;
        public const string Description = "Asset category information";
    }

    #endregion

    #region Manufacturer Rules

    public static class Manufacturer
    {
        public const int NameMaxLength = 200;
        public const int CodeMaxLength = 50;
        public const string Description = "Manufacturer information";
    }

    #endregion

    #region Supplier Rules

    public static class Supplier
    {
        public const int NameMaxLength = 200;
        public const int CodeMaxLength = 50;
        public const string Description = "Supplier information";
    }

    #endregion

    #region Location Rules

    public static class Location
    {
        public const int NameMaxLength = 200;
        public const int CodeMaxLength = 50;
        public const string Description = "Storage location information";
    }

    #endregion

    #region Menu Rules

    public static class Menu
    {
        public const int NameMaxLength = 100;
        public const int CodeMaxLength = 50;
        public const int UrlMaxLength = 500;
        public const int MaxDepth = 3;
        public const string Description = "Menu item information";
    }

    #endregion

    #region Setting Rules

    public static class Setting
    {
        public const int KeyMaxLength = 100;
        public const int ValueMaxLength = 2000;
        public const string Description = "System setting";
    }

    #endregion

    #region SigningKey Rules

    public static class SigningKey
    {
        public static readonly string[] ValidAlgorithms = { "RS256", "RS384", "RS512" };
        public static readonly int[] ValidKeySizes = { 2048, 3072, 4096 };
        public const int DefaultKeySize = 2048;
        public const string DefaultAlgorithm = "RS256";
        public const string Description = "JWT signing key";
    }

    #endregion

    #region Device Trust Rules

    public static class DeviceTrust
    {
        /// <summary>
        /// Minimum number of days a device must be trusted before allowing sensitive operations like deleting other sessions
        /// This prevents newly compromised devices from being used to delete other sessions
        /// </summary>
        public const int MinimumTrustDaysForSensitiveOperations = 3;
    }

    #endregion
}
