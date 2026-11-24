using Meitou.Simulation;

namespace AnimalBehaviors;

/// <summary>
/// One animal at another faction's hitching post, as a rule sees it (docs/depending-on-animal-behaviors.md "Other factions' posts"):
/// the world (a serial step: a rule may change it), the hitch, the post's owner, and whether the animal has just been hitched there
/// (<paramref name="First"/>) or is staying into a new game day.
/// </summary>
public readonly record struct HitchUse(World World, Hitched Hitch, int Owner, bool First);

/// <summary>
/// A dependent's terms for other factions' hitching posts: null lets the animal stay (after charging, a relations change or a crime the rule
/// made itself), a text refuses it (the animal is unhitched and the text floats over it).
/// </summary>
public delegate string? HitchRule(HitchUse use);

/// <summary>
/// The dependents' hitch rules in registration order, each kept under its mod's id. Filled during the mods' <c>Initialize</c>, frozen with the
/// animal registry when the first world is built.
/// </summary>
public sealed class HitchRules
{
    /// <summary>
    /// The rules with their mods' ids, in registration order.
    /// </summary>
    readonly List<(string Owner, HitchRule Rule)> _rules = [];

    /// <summary>
    /// How many rules are registered.
    /// </summary>
    public int Count => _rules.Count;

    /// <summary>
    /// Registers a mod's rule.
    /// </summary>
    public void Add(string owner, HitchRule rule)
    {
        ArgumentNullException.ThrowIfNull(rule);
        _rules.Add((owner, rule));
    }

    /// <summary>
    /// Drops a mod's rules (its <c>Initialize</c> failed).
    /// </summary>
    public void Drop(string owner) => _rules.RemoveAll(r => r.Owner == owner);

    /// <summary>
    /// Asks the rules in order: the first refusal's text, or null when every rule lets the animal stay (none registered: free). A rule that
    /// throws is logged and lets it stay.
    /// </summary>
    public string? Ask(HitchUse use, Action<string> log)
    {
        foreach (var (owner, rule) in _rules)
        {
            string? refusal;
            try
            {
                refusal = rule(use);
            }
            catch (Exception e)
            {
                log($"animal-behaviors: {owner}'s hitch rule failed: {e.Message}");
                continue;
            }

            if (refusal is not null) return refusal;
        }

        return null;
    }
}
