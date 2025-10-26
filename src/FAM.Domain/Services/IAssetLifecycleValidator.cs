using FAM.Domain.Assets;

namespace FAM.Domain.Services;

/// <summary>
/// Domain Service: Validation rules cho asset lifecycle transitions
/// </summary>
public interface IAssetLifecycleValidator
{
    bool CanTransitionTo(Asset asset, string newLifecycleCode);
    IEnumerable<string> GetAllowedTransitions(string currentLifecycleCode);
    string? ValidateTransition(Asset asset, string newLifecycleCode);
}

public class AssetLifecycleValidator : IAssetLifecycleValidator
{
    private static readonly Dictionary<string, string[]> _allowedTransitions = new()
    {
        ["draft"] = new[] { "pending_approval", "rejected" },
        ["pending_approval"] = new[] { "approved", "rejected", "draft" },
        ["approved"] = new[] { "active", "rejected" },
        ["active"] = new[] { "awaiting_writeoff", "under_repair" },
        ["awaiting_writeoff"] = new[] { "written_off", "active" },
        ["written_off"] = Array.Empty<string>(),
        ["rejected"] = new[] { "draft" }
    };

    public bool CanTransitionTo(Asset asset, string newLifecycleCode)
    {
        if (string.IsNullOrEmpty(asset.LifecycleCode))
            return newLifecycleCode == "draft";

        return _allowedTransitions.TryGetValue(asset.LifecycleCode, out var allowed) &&
               allowed.Contains(newLifecycleCode);
    }

    public IEnumerable<string> GetAllowedTransitions(string currentLifecycleCode)
    {
        return _allowedTransitions.TryGetValue(currentLifecycleCode, out var allowed)
            ? allowed
            : Array.Empty<string>();
    }

    public string? ValidateTransition(Asset asset, string newLifecycleCode)
    {
        if (asset.IsDeleted)
            return "Cannot change lifecycle of deleted asset";

        if (!CanTransitionTo(asset, newLifecycleCode))
            return $"Cannot transition from {asset.LifecycleCode} to {newLifecycleCode}";

        // Additional business rules
        if (newLifecycleCode == "active" && !asset.PurchaseDate.HasValue)
            return "Asset must have purchase date before activation";

        if (newLifecycleCode == "active" && !asset.LocationId.HasValue)
            return "Asset must have location before activation";

        return null; // Valid
    }
}
