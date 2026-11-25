using Meitou.Data.Fcs;
using Meitou.Data.Gameplay.Bodies;
using Meitou.Modding;
using Meitou.Simulation;
using Meitou.Simulation.Bodies;

namespace AnimalBehaviors;

/// <summary>
/// Animal Behaviors' entry (docs/animal-behaviors.md): the Hitched job on every marked hitching post (<see cref="Sites"/>), the system that keeps
/// pens and saves sexes and hitches, the sex patches, the settings and the debug rows. Also the framework's surface for dependents
/// (<see cref="IAnimalBehaviorsApi"/>, docs/depending-on-animal-behaviors.md). It adds no records: the posts are its dependents'.
/// </summary>
public sealed class AnimalBehaviorsMod : IMod, IAnimalBehaviorsApi, IModDependents
{
    /// <summary>
    /// The mod's manifest id.
    /// </summary>
    public const string Id = "animal.behaviors";

    /// <summary>
    /// The player's settings.
    /// </summary>
    public AnimalSettings Settings { get; } = new();

    /// <summary>
    /// The system of the latest world built.
    /// </summary>
    AnimalBehaviorsSystem? _latest;

    /// <summary>
    /// The animal kinds, built-in and dependents'.
    /// </summary>
    readonly AnimalRegistry _registry = new();

    /// <summary>
    /// The dependents' terms for other factions' hitching posts.
    /// </summary>
    readonly HitchRules _rules = new();

    /// <summary>
    /// The capped hitching posts' capacities by record id, read from the marks.
    /// </summary>
    Dictionary<string, int> _capacities = [];

    /// <summary>
    /// Every RACE of the loaded data, for the list of supported animals.
    /// </summary>
    List<(string StringId, string Name, bool Robot)> _races = [];

    public void Initialize(ModContext c)
    {
        c.AddSettings(Settings);
        c.Harmony.PatchAll(typeof(AnimalBehaviorsMod).Assembly);
        AddHitching(c);
        c.AddSystem("animal-behaviors", ctx => _latest = Build(c, ctx), SystemAnchor.End);
        c.AddDebugCommand("Animal Behaviors: hitched animals", _ => Told(c, _latest?.View ?? NoWorld));
        c.AddDebugCommand("Animal Behaviors: supported animals", _ => Told(c, string.Join(Environment.NewLine, _registry.Describe(_races))));
        _races = Races(c);
    }

    /// <summary>
    /// The Hitched job, offered on every building a mod marked as a hitching post; none marked, the job is claimed but offered nowhere.
    /// </summary>
    void AddHitching(ModContext c)
    {
        var buildings = c.Records((int)FcsRecordType.Building).ToList();
        var posts = Sites.Marked(buildings, Sites.Hitch);
        _capacities = Sites.Capacities(buildings);
        c.AddJobType(Hitches.Type, AnimalText.Get(Hitches.Name));
        if (posts.Count > 0) c.AddJobOffer(Hitches.Offer(posts));
        c.Log($"animal-behaviors: hitching posts: {(posts.Count == 0 ? "none" : string.Join(", ", posts))}");
        if (!Hitches.OffersForeign) c.Log("animal-behaviors: this engine offers jobs on the player's buildings only, so other factions' posts are off");
    }

    /// <summary>
    /// A world's system; the first ends registration and logs the list of supported animals.
    /// </summary>
    AnimalBehaviorsSystem Build(ModContext c, SystemBuildContext context)
    {
        if (!_registry.Frozen)
        {
            _registry.Freeze();
            c.Log("animal-behaviors: supported animals: " + string.Join("; ", _registry.Describe(_races)));
        }

        return new AnimalBehaviorsSystem(Settings, new HitchPosts(_capacities), new ForeignPosts(_rules, c.Log), c.RngPurpose);
    }

    /// <summary>
    /// Every RACE of the loaded data (mods' too): its string id, name and whether it is a robot.
    /// </summary>
    static List<(string StringId, string Name, bool Robot)> Races(ModContext c) =>
        c.Records((int)FcsRecordType.Race)
            .Select(r => (r.StringId, r.Name, r.GetBool("is robot")))
            .ToList();

    #region Framework

    public IAnimalBehaviorsRegistry For(ModContext c) => new Registration(_registry, _rules, c.Id);

    public AnimalSupport SupportOf(RaceData race) => _registry.SupportOf(race);

    public bool Supports(CharacterCold cold, AnimalSupport what) => _registry.Supports(cold, what);

    public IReadOnlyList<AnimalKind> Kinds => _registry.Kinds;

    public IAnimalState? Current => _latest;

    public IAnimalState? In(World world) => world.Systems.OfType<AnimalBehaviorsSystem>().FirstOrDefault();

    public void DependentFailed(string modId)
    {
        _registry.Drop(modId);
        _rules.Drop(modId);
    }

    /// <summary>
    /// One mod's registrations, kept under its id.
    /// </summary>
    sealed class Registration(AnimalRegistry registry, HitchRules rules, string owner) : IAnimalBehaviorsRegistry
    {
        public void AddAnimal(AnimalKind kind) => registry.AddKind(owner, kind);

        public void AddHitchRule(HitchRule rule)
        {
            registry.CheckOpen();
            rules.Add(owner, rule);
        }
    }

    #endregion

    /// <summary>
    /// Logs a debug answer too (the status line shows only its start).
    /// </summary>
    static string Told(ModContext c, string answer)
    {
        c.Log(answer);
        return answer;
    }

    /// <summary>
    /// The debug answer before the first world.
    /// </summary>
    const string NoWorld = "no world yet";
}
