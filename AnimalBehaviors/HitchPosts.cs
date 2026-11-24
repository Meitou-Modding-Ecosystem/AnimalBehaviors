using System.Numerics;
using Meitou.Simulation;
using Meitou.Simulation.Bodies;
using Meitou.Simulation.Construction;
using Meitou.Simulation.Player;

namespace AnimalBehaviors;

/// <summary>
/// Each hitching post's room (docs/animal-behaviors.md "Hitching"): a post whose record caps its animals (<see cref="Sites.CapacityField"/>) keeps
/// the animals it held at the last check and takes the newest off again with a warning over them. Serial steps only.
/// </summary>
public sealed class HitchPosts(IReadOnlyDictionary<string, int> capacities)
{
    /// <summary>
    /// The portrait serials hitched at the last check, which a full post keeps before newcomers.
    /// </summary>
    HashSet<uint> _kept = [];

    /// <summary>
    /// The warning over an animal a full post turned away.
    /// </summary>
    public const string Full = "This post is full ({1} animals)";

    /// <summary>
    /// How many animals the post holds; 0 for no limit.
    /// </summary>
    public int Capacity(BuildSite post) => capacities.GetValueOrDefault(post.Record);

    /// <summary>
    /// Takes the newcomers off every post holding more than its capacity, and returns the hitches left.
    /// </summary>
    public List<Hitched> Enforce(World world, List<Hitched> hitched)
    {
        var left = new List<Hitched>(hitched.Count);
        foreach (var post in hitched.GroupBy(h => h.Post.Id))
        {
            int capacity = Capacity(post.First().Post);
            var ordered = post.OrderBy(h => Kept(h) ? 0 : 1).ThenBy(h => h.Slot).ToList();
            int keep = capacity > 0 ? Math.Min(capacity, ordered.Count) : ordered.Count;
            left.AddRange(ordered.Take(keep));
            foreach (var h in ordered.Skip(keep)) TurnAway(world, h.Slot, h.Cold, capacity);
        }

        _kept = left.Where(h => h.Cold.PortraitSerial is not null).Select(h => h.Cold.PortraitSerial!.Value).ToHashSet();
        return left.OrderBy(h => h.Slot).ToList();
    }

    /// <summary>
    /// Hitches an animal to a post when it has room; false, with the warning over the animal, when full.
    /// </summary>
    public bool TryHitch(World world, int slot, BuildSite post)
    {
        int capacity = Capacity(post);
        if (world.Characters.Cold(slot) is not { } cold) return false;

        if (capacity > 0 && Holding(world, post.Id) >= capacity)
        {
            Say(world, slot, capacity);
            return false;
        }

        Hitches.Hitch(cold, post.Id);
        if (cold.PortraitSerial is { } serial) _kept.Add(serial);
        return true;
    }

    /// <summary>
    /// How many living animals of the player's are hitched to the post now.
    /// </summary>
    static int Holding(World world, string post)
    {
        int count = 0;
        var table = world.Characters;
        for (int s = 0; s < table.HighWater; s++)
        {
            if (table.Next[s].Alive && table.Cold(s) is { IsPlayer: true, DiedTick: < 0 } cold && Hitches.PostOf(cold) == post) count++;
        }

        return count;
    }

    /// <summary>
    /// Whether the animal was hitched at the last check.
    /// </summary>
    bool Kept(Hitched h) => h.Cold.PortraitSerial is { } serial && _kept.Contains(serial);

    /// <summary>
    /// Unhitches an animal from a full post and says so over it.
    /// </summary>
    static void TurnAway(World world, int slot, CharacterCold cold, int capacity)
    {
        Hitches.Unhitch(cold);
        Say(world, slot, capacity);
    }

    /// <summary>
    /// Floats the full-post warning over a character.
    /// </summary>
    static void Say(World world, int slot, int capacity)
    {
        var at = world.Characters.Next[slot].Position + new Vector3(0, 20, 0);
        world.Floaters?.Enqueue(Floater.Label(at, AnimalText.Format(Full, capacity)));
    }
}
