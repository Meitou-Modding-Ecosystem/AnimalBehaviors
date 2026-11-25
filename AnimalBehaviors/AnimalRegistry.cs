using Meitou.Data.Gameplay.Ai;
using Meitou.Data.Gameplay.Bodies;
using Meitou.Simulation;
using Meitou.Simulation.Bodies;
using Meitou.Simulation.Factions;

namespace AnimalBehaviors;

/// <summary>
/// The animal kinds Animal Behaviors and its dependents registered (docs/animal-behaviors.md "Supported animals"),
/// and what each animal may do. Filled during the mods' <c>Initialize</c>, frozen when the first world is built, read by the systems after.
/// </summary>
public sealed class AnimalRegistry
{
    #region Kinds

    /// <summary>
    /// What a robot's race supports unless a registration says otherwise: training only (it neither breeds nor fits a capsule).
    /// </summary>
    public const AnimalSupport RobotSupport = AnimalSupport.Training;

    /// <summary>
    /// What any other animal's race supports unless a registration says otherwise.
    /// </summary>
    public const AnimalSupport DefaultSupport = AnimalSupport.All;

    /// <summary>
    /// The built-in list (docs/animal-behaviors.md "Supported animals").
    /// </summary>
    public static readonly AnimalKind[] BuiltIn =
    [
        new("Leviathan", AnimalSupport.Breeding | AnimalSupport.Training, "too big for a capsule") { Owner = AnimalBehaviorsMod.Id },
    ];

    /// <summary>
    /// The registered kinds in registration order.
    /// </summary>
    readonly List<AnimalKind> _kinds = [.. BuiltIn];

    /// <summary>
    /// The registered kinds in registration order.
    /// </summary>
    public IReadOnlyList<AnimalKind> Kinds => _kinds;

    /// <summary>
    /// Registers an animal kind for a mod.
    /// </summary>
    public void AddKind(string owner, AnimalKind kind)
    {
        CheckOpen();
        if (string.IsNullOrWhiteSpace(kind.Race)) throw new ArgumentException("An animal kind needs a race (a RACE string id or name).", nameof(kind));

        _kinds.Add(kind with { Owner = owner, Race = kind.Race.Trim() });
    }

    /// <summary>
    /// What animals of a race may do (<see cref="SupportOf(string, string, bool)"/>).
    /// </summary>
    public AnimalSupport SupportOf(RaceData race) => SupportOf(race.StringId, race.Name, race.IsRobot);

    /// <summary>
    /// What animals of a race may do: the last registration naming its string id, else the last naming its name, else
    /// <see cref="RobotSupport"/> for a robot and <see cref="DefaultSupport"/> for the rest.
    /// </summary>
    public AnimalSupport SupportOf(string stringId, string name, bool robot)
    {
        if (Last(stringId) is { } byId) return byId.Support;
        if (Last(name) is { } byName) return byName.Support;

        return robot ? RobotSupport : DefaultSupport;
    }

    /// <summary>
    /// Whether a character is an animal whose race supports everything in <paramref name="what"/>.
    /// </summary>
    public bool Supports(CharacterCold cold, AnimalSupport what) =>
        SquadRegistry.IsAnimal(cold) && cold.Race is { } race && (SupportOf(race) & what) == what;

    /// <summary>
    /// The list of what each race may do, a line per race ("Garru: breeding, training, taming, summoning"), sorted by name.
    /// </summary>
    public List<string> Describe(IEnumerable<(string StringId, string Name, bool Robot)> races) =>
        races
            .DistinctBy(r => r.StringId)
            .OrderBy(r => r.Name, StringComparer.Ordinal)
            .Select(r => $"{r.Name}: {Words(SupportOf(r.StringId, r.Name, r.Robot))}")
            .ToList();

    /// <summary>
    /// The words for a support ("breeding, training", "nothing").
    /// </summary>
    public static string Words(AnimalSupport support)
    {
        var words = Enum.GetValues<AnimalSupport>()
            .Where(s => s is not (AnimalSupport.None or AnimalSupport.All) && (support & s) == s)
            .Select(s => s.ToString().ToLowerInvariant())
            .ToList();
        return words.Count == 0 ? "nothing" : string.Join(", ", words);
    }

    /// <summary>
    /// The last registration naming a race, or null.
    /// </summary>
    AnimalKind? Last(string race) =>
        race.Length == 0 ? null : _kinds.LastOrDefault(k => string.Equals(k.Race, race, StringComparison.OrdinalIgnoreCase));

    #endregion

    #region Lifetime

    /// <summary>
    /// Set when the first world is built; registrations after it throw.
    /// </summary>
    public bool Frozen { get; private set; }

    /// <summary>
    /// Ends registration (the first world is being built).
    /// </summary>
    public void Freeze() => Frozen = true;

    /// <summary>
    /// Drops everything a mod registered (its <c>Initialize</c> failed).
    /// </summary>
    public void Drop(string owner)
    {
        _kinds.RemoveAll(k => k.Owner == owner);
    }

    /// <summary>
    /// Throws once registration has ended.
    /// </summary>
    public void CheckOpen()
    {
        if (Frozen) throw new InvalidOperationException("Animal Behaviors takes registrations only in a mod's Initialize, before the first world.");
    }

    #endregion
}
