namespace FAM.WebApi.Attributes;

/// <summary>
/// Attribute to mark endpoints that require device_id validation
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequireDeviceIdAttribute : Attribute
{
}
