using FAM.Domain.Assets;
using FAM.Domain.Abstractions;

namespace FAM.Domain.Services;

/// <summary>
/// Domain Service: Quản lý asset tag (mã tài sản)
/// </summary>
public interface IAssetTagGenerator
{
    Task<string> GenerateAssetTagAsync(int? companyId, int? assetTypeId, CancellationToken cancellationToken = default);
    bool ValidateAssetTag(string assetTag);
    string ParseAssetTagPrefix(string assetTag);
    int? ParseAssetTagSequence(string assetTag);
}

public class AssetTagGenerator : IAssetTagGenerator
{
    private readonly IAssetRepository _assetRepository;

    public AssetTagGenerator(IAssetRepository assetRepository)
    {
        _assetRepository = assetRepository;
    }

    public async Task<string> GenerateAssetTagAsync(
        int? companyId, 
        int? assetTypeId, 
        CancellationToken cancellationToken = default)
    {
        // Format: {CompanyCode}{TypeCode}{YYYYMM}{Sequence}
        // Example: C01-IT-202410-0001
        
        var companyCode = companyId.HasValue ? $"C{companyId:D2}" : "C00";
        var typeCode = "IT"; // Get from asset type
        var period = DateTime.UtcNow.ToString("yyyyMM");
        
        // Find next sequence number
        var sequence = await GetNextSequenceAsync(companyCode, typeCode, period, cancellationToken);
        
        return $"{companyCode}-{typeCode}-{period}-{sequence:D4}";
    }

    public bool ValidateAssetTag(string assetTag)
    {
        if (string.IsNullOrWhiteSpace(assetTag))
            return false;

        // Format: XXX-XX-YYYYMM-NNNN
        var parts = assetTag.Split('-');
        return parts.Length == 4 && parts[3].Length == 4 && int.TryParse(parts[3], out _);
    }

    public string ParseAssetTagPrefix(string assetTag)
    {
        var parts = assetTag.Split('-');
        return parts.Length >= 3 ? $"{parts[0]}-{parts[1]}-{parts[2]}" : string.Empty;
    }

    public int? ParseAssetTagSequence(string assetTag)
    {
        var parts = assetTag.Split('-');
        if (parts.Length == 4 && int.TryParse(parts[3], out var sequence))
            return sequence;
        return null;
    }

    private async Task<int> GetNextSequenceAsync(
        string companyCode, 
        string typeCode, 
        string period, 
        CancellationToken cancellationToken)
    {
        var prefix = $"{companyCode}-{typeCode}-{period}";
        var assets = await _assetRepository.GetAllAsync(cancellationToken);
        
        var maxSequence = assets
            .Where(a => a.AssetTag?.StartsWith(prefix) == true)
            .Select(a => ParseAssetTagSequence(a.AssetTag!))
            .Where(s => s.HasValue)
            .DefaultIfEmpty(0)
            .Max() ?? 0;

        return maxSequence + 1;
    }
}
