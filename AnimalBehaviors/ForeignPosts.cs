using System.Numerics;
using Meitou.Simulation;
using Meitou.Simulation.Player;

namespace AnimalBehaviors;

/// <summary>
/// The animals at other factions' hitching posts (docs/animal-behaviors.md "Other factions' posts"): the rules are asked when an animal is first
/// seen at such a post and again at each new game day while it stays; a refusal unhitches it with the rule's text over it.
/// </summary>
public sealed class ForeignPosts(HitchRules rules, Action<string> log)
{
    /// <summary>
    /// The foreign post each animal was last let stay at, by portrait serial (the animals without one by their slot, as ~slot).
    /// </summary>
    public Dictionary<uint, string> Seen { get; } = [];

    /// <summary>
    /// The game day the rules were last asked for the staying animals; -1 before the first check.
    /// </summary>
    public long Day { get; set; } = -1;

    /// <summary>
    /// Asks the rules for every animal at another faction's post and returns the hitches left.
    /// </summary>
    public List<Hitched> Check(World world, List<Hitched> hitched)
    {
        long day = (long)Math.Floor(world.GameHours / 24);
        bool newDay = Day >= 0 && day != Day;
        Day = day;

        var left = new List<Hitched>(hitched.Count);
        var seen = new HashSet<uint>();
        foreach (var h in hitched)
        {
            if (h.Post.Owner == world.Player.Faction || Stays(world, h, newDay, seen)) left.Add(h);
        }

        foreach (uint key in Seen.Keys.Where(k => !seen.Contains(k)).ToList()) Seen.Remove(key);
        return left;
    }

    /// <summary>
    /// Whether an animal at another faction's post may stay: asked when it is new there or a new day began, else it stays.
    /// </summary>
    bool Stays(World world, Hitched h, bool newDay, HashSet<uint> seen)
    {
        uint key = Key(h);
        bool first = !Seen.TryGetValue(key, out string? post) || post != h.Post.Id;
        if (!first && !newDay)
        {
            seen.Add(key);
            return true;
        }

        if (rules.Ask(new HitchUse(world, h, h.Post.Owner, first), log) is { } refusal)
        {
            Refuse(world, h, refusal);
            return false;
        }

        Seen[key] = h.Post.Id;
        seen.Add(key);
        return true;
    }

    /// <summary>
    /// Unhitches a refused animal and floats the rule's text over it.
    /// </summary>
    static void Refuse(World world, Hitched h, string refusal)
    {
        Hitches.Unhitch(h.Cold);
        var at = world.Characters.Next[h.Slot].Position + new Vector3(0, 20, 0);
        world.Floaters?.Enqueue(Floater.Label(at, refusal));
    }

    /// <summary>
    /// An animal's key: its portrait serial, else its slot complemented.
    /// </summary>
    static uint Key(Hitched h) => h.Cold.PortraitSerial ?? ~(uint)h.Slot;
}
