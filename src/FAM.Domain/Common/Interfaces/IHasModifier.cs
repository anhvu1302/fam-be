namespace FAM.Domain.Common.Interfaces;

/// <summary>
/// Interface for entities that track who modified them
/// </summary>
public interface IHasModifier
{
    long? UpdatedById { get; set; }
}
