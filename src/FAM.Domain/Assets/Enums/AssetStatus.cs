namespace FAM.Domain.Assets.Enums;

/// <summary>
/// Trạng thái vòng đời của tài sản
/// </summary>
public enum AssetLifecycleStatus
{
    Draft, // Nháp
    PendingApproval, // Chờ duyệt
    Approved, // Đã duyệt
    Active, // Đang hoạt động
    AwaitingWriteoff, // Chờ xóa sổ
    WrittenOff, // Đã xóa sổ
    Rejected // Từ chối
}

/// <summary>
/// Trạng thái sử dụng
/// </summary>
public enum AssetUsageStatus
{
    Available, // Sẵn sàng
    InUse, // Đang sử dụng
    UnderRepair // Đang sửa chữa
}

/// <summary>
/// Phương pháp khấu hao
/// </summary>
public enum DepreciationMethod
{
    StraightLine, // Khấu hao đều
    DecliningBalance, // Khấu hao giảm dần
    UnitsOfProduction // Khấu hao theo sản lượng
}