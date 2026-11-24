using Meitou.Data;
using Meitou.Data.Fcs;

namespace AnimalBehaviors;

/// <summary>
/// Which buildings offer the framework's jobs (docs/depending-on-animal-behaviors.md "Hitching posts"): a BUILDING record names the behaviours it
/// hosts in its string field <see cref="Field"/>, a comma-separated list (today only <see cref="Hitch"/>), and a hitching post may cap its animals
/// in the int field <see cref="CapacityField"/>. The engine lets only the mod that claims a job type offer it, so every mod's posts share one offer
/// (one job id): a dependent marks its own records in its data phase, and the framework reads the marks when it initializes, after every data
/// phase.
/// </summary>
public static class Sites
{
    /// <summary>
    /// The BUILDING string field that names the behaviours.
    /// </summary>
    public const string Field = "animal behaviors";

    /// <summary>
    /// The BUILDING int field with the most animals a hitching post holds (missing or 0: no limit).
    /// </summary>
    public const string CapacityField = "animal behaviors hitch capacity";

    /// <summary>
    /// A hitching post (the Hitched job: pen and grazing).
    /// </summary>
    public const string Hitch = "hitch";

    /// <summary>
    /// Marks a building record as a hitching post holding at most <paramref name="capacity"/> animals (0: no limit), in a dependent's data phase.
    /// </summary>
    public static void MarkHitch(FcsRecord building, int capacity = 0)
    {
        Mark(building, Hitch);
        if (capacity > 0) building.Ints[CapacityField] = capacity;
    }

    /// <summary>
    /// Adds a behaviour to a building record's marks, keeping those it has.
    /// </summary>
    public static void Mark(FcsRecord building, string behavior)
    {
        var marks = Marks(building.Strings.GetValueOrDefault(Field));
        if (!marks.Contains(behavior)) marks.Add(behavior);
        building.Strings[Field] = string.Join(",", marks);
    }

    /// <summary>
    /// The string ids of the buildings marked with a behaviour, in record order.
    /// </summary>
    public static List<string> Marked(IEnumerable<GameRecord> buildings, string behavior) =>
        buildings
            .Where(b => Marks(b.GetString(Field)).Contains(behavior))
            .Select(b => b.StringId)
            .ToList();

    /// <summary>
    /// The capped hitching posts' capacities by record string id.
    /// </summary>
    public static Dictionary<string, int> Capacities(IEnumerable<GameRecord> buildings) =>
        buildings
            .Where(b => Marks(b.GetString(Field)).Contains(Hitch) && b.GetInt(CapacityField) > 0)
            .ToDictionary(b => b.StringId, b => b.GetInt(CapacityField));

    /// <summary>
    /// The behaviours of a field's value, trimmed, without case.
    /// </summary>
    static List<string> Marks(string? value) =>
        (value ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(m => m.ToLowerInvariant())
            .ToList();
}
