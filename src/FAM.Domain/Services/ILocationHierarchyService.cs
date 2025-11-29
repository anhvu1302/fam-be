using FAM.Domain.Locations;

namespace FAM.Domain.Services;

/// <summary>
/// Domain Service: Quản lý location hierarchy (cây phân cấp)
/// </summary>
public interface ILocationHierarchyService
{
    string BuildFullPath(Location location, IEnumerable<Location> allLocations);
    string BuildPathIds(Location location, IEnumerable<Location> allLocations);
    IEnumerable<Location> GetAncestors(Location location, IEnumerable<Location> allLocations);
    IEnumerable<Location> GetDescendants(Location location, IEnumerable<Location> allLocations);
    int GetDepth(Location location, IEnumerable<Location> allLocations);
    bool IsAncestorOf(Location potentialAncestor, Location location, IEnumerable<Location> allLocations);
    bool WouldCreateCycle(int locationId, int newParentId, IEnumerable<Location> allLocations);
}

public class LocationHierarchyService : ILocationHierarchyService
{
    public string BuildFullPath(Location location, IEnumerable<Location> allLocations)
    {
        var ancestors = GetAncestors(location, allLocations).Reverse().ToList();
        ancestors.Add(location);
        return string.Join(" > ", ancestors.Select(l => l.Name));
    }

    public string BuildPathIds(Location location, IEnumerable<Location> allLocations)
    {
        var ancestors = GetAncestors(location, allLocations).Reverse().ToList();
        ancestors.Add(location);
        return string.Join("/", ancestors.Select(l => l.Id));
    }

    public IEnumerable<Location> GetAncestors(Location location, IEnumerable<Location> allLocations)
    {
        var ancestors = new List<Location>();
        var current = location;

        while (current.ParentId.HasValue)
        {
            var parent = allLocations.FirstOrDefault(l => l.Id == current.ParentId.Value);
            if (parent == null)
                break;

            ancestors.Add(parent);
            current = parent;
        }

        return ancestors;
    }

    public IEnumerable<Location> GetDescendants(Location location, IEnumerable<Location> allLocations)
    {
        var descendants = new List<Location>();
        var queue = new Queue<Location>();
        queue.Enqueue(location);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var children = allLocations.Where(l => l.ParentId == current.Id);

            foreach (var child in children)
            {
                descendants.Add(child);
                queue.Enqueue(child);
            }
        }

        return descendants;
    }

    public int GetDepth(Location location, IEnumerable<Location> allLocations)
    {
        return GetAncestors(location, allLocations).Count();
    }

    public bool IsAncestorOf(Location potentialAncestor, Location location, IEnumerable<Location> allLocations)
    {
        var ancestors = GetAncestors(location, allLocations);
        return ancestors.Any(a => a.Id == potentialAncestor.Id);
    }

    public bool WouldCreateCycle(int locationId, int newParentId, IEnumerable<Location> allLocations)
    {
        if (locationId == newParentId)
            return true;

        var location = allLocations.FirstOrDefault(l => l.Id == locationId);
        var newParent = allLocations.FirstOrDefault(l => l.Id == newParentId);

        if (location == null || newParent == null)
            return false;

        return IsAncestorOf(location, newParent, allLocations);
    }
}