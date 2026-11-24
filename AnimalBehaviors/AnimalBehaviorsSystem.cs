using System.Text;
using Meitou.Modding;
using Meitou.Simulation;
using Meitou.Simulation.Construction;

namespace AnimalBehaviors;

/// <summary>
/// Animal Behaviors in one world (docs/animal-behaviors.md): the pens kept and the posts' room enforced in serial steps, the hitches and set
/// sexes saved in the mod's sidecar and hashed.
/// </summary>
public sealed class AnimalBehaviorsSystem : ITickSystem, IStateHashed, IModSaveData, IAnimalState
{
    #region Ticking

    /// <summary>
    /// Ticks between pen checks of one animal (one game second at 30 ticks a second), staggered by slot.
    /// </summary>
    public const int PenEveryTicks = 30;

    /// <summary>
    /// The player's settings, copied at each pass.
    /// </summary>
    readonly AnimalSettings _settings;

    /// <summary>
    /// The posts' room.
    /// </summary>
    readonly HitchPosts _posts;

    /// <summary>
    /// The mod's random purpose (the pens' strolls).
    /// </summary>
    readonly RngPurpose _purpose;

    /// <summary>
    /// The latest view for the debug row (written in the tick, read on the host's thread).
    /// </summary>
    volatile string _view = "No hitched animals yet.";

    /// <summary>
    /// A system for one world.
    /// </summary>
    public AnimalBehaviorsSystem(AnimalSettings settings, HitchPosts posts, RngPurpose purpose)
    {
        _settings = settings;
        _posts = posts;
        _purpose = purpose;
        TradeWindowGender.Sexes = Sexes;
    }

    public Sexes Sexes { get; } = new();

    /// <summary>
    /// The debug row's view of the hitched animals.
    /// </summary>
    public string View => _view;

    public void SlowWorld(World world)
    {
        RestoreHitches(world);
        var settings = _settings.Copy();
        var hitched = _posts.Enforce(world, Hitches.All(world));
        foreach (var h in hitched)
            if ((h.Slot + world.Tick) % PenEveryTicks == 0) Pens.Keep(world, h, settings, _purpose);

        if (world.Tick % PenEveryTicks == 0) _view = Describe(hitched);
    }

    public int Capacity(BuildSite post) => _posts.Capacity(post);

    public bool TryHitch(World world, int slot, BuildSite post) => _posts.TryHitch(world, slot, post);

    public BuildSite? PostOf(World world, int slot) => Hitches.Animal(world, slot) is { } cold ? Hitches.Site(world, cold) : null;

    public IReadOnlyList<Hitched> HitchedTo(World world, BuildSite post) => Hitches.At(world, post);

    /// <summary>
    /// The debug view: each hitched animal with its post, race, sex, age and hunger.
    /// </summary>
    string Describe(List<Hitched> hitched)
    {
        if (hitched.Count == 0) return "No hitched animals.";

        var text = new StringBuilder();
        foreach (var h in hitched)
        {
            string sex = Sexes.Female(h.Cold) ? "female" : "male";
            text.AppendLine($"{h.Cold.Name} ({h.Cold.Race?.Name}, {sex}, age {h.Cold.Age:0.00}, {Hitches.Fed(Hitches.Hunger(h.Cold))}) at {h.Post.Id}");
        }

        return text.ToString();
    }

    #endregion

    #region Saving

    /// <summary>
    /// The sidecar layout's version.
    /// </summary>
    public const int SaveVersion = 1;

    /// <summary>
    /// Hitches read from a save or imported, waiting for the next tick to find their characters by portrait serial.
    /// </summary>
    List<(uint Serial, string Post)>? _pending;

    /// <summary>
    /// The world the last tick ran on, for the save's hitches.
    /// </summary>
    World? _world;

    public void ImportHitches(IEnumerable<(uint Serial, string Post)> hitches) => (_pending ??= []).AddRange(hitches);

    public void Write(Stream stream)
    {
        using var w = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        w.Write(SaveVersion);
        var hitches = SavedHitches();
        w.Write(hitches.Count);
        foreach (var (serial, post) in hitches)
        {
            w.Write(serial);
            w.Write(post);
        }

        w.Write(Sexes.Assigned.Count);
        foreach (var (serial, female) in Sexes.Assigned)
        {
            w.Write(serial);
            w.Write(female);
        }
    }

    public void Read(Stream stream)
    {
        using var r = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        int version = r.ReadInt32();
        if (version is < 1 or > SaveVersion) return;

        var pending = new List<(uint, string)>();
        int hitches = r.ReadInt32();
        for (int k = 0; k < hitches; k++) pending.Add((r.ReadUInt32(), r.ReadString()));
        ImportHitches(pending);

        int sexes = r.ReadInt32();
        for (int k = 0; k < sexes; k++) Sexes.Assigned[r.ReadUInt32()] = r.ReadBoolean();
    }

    /// <summary>
    /// Every hitch of the last world ticked, and those still waiting for their characters.
    /// </summary>
    List<(uint Serial, string Post)> SavedHitches()
    {
        var list = new List<(uint, string)>(_pending ?? []);
        if (_world is not { } world) return list;

        var table = world.Characters;
        for (int s = 0; s < table.HighWater; s++)
        {
            if (table.Next[s].Alive && table.Cold(s) is { PortraitSerial: { } serial } cold && Hitches.PostOf(cold) is { } post)
                list.Add((serial, post));
        }

        return list;
    }

    /// <summary>
    /// Puts the saved and imported hitches back on their characters at the next tick (the engine keeps no mod jobs in its save).
    /// </summary>
    void RestoreHitches(World world)
    {
        _world = world;
        if (_pending is not { } pending) return;

        _pending = null;
        var posts = pending.GroupBy(p => p.Serial).ToDictionary(g => g.Key, g => g.Last().Post);
        var table = world.Characters;
        for (int s = 0; s < table.HighWater; s++)
        {
            if (table.Next[s].Alive && table.Cold(s) is { IsPlayer: true, PortraitSerial: { } serial } cold && posts.TryGetValue(serial, out string? post))
                Hitches.Hitch(cold, post);
        }
    }

    #endregion

    #region Hashing

    public void Hash(ref StateHasher hasher)
    {
        hasher.Add(Sexes.Assigned.Count);
        foreach (var (serial, female) in Sexes.Assigned)
        {
            hasher.Add(serial);
            hasher.Add(female);
        }

        hasher.Add(_pending?.Count ?? -1);
        HashHitches(ref hasher);
    }

    /// <summary>
    /// The hitches on the characters (the engine hashes no mod jobs).
    /// </summary>
    void HashHitches(ref StateHasher hasher)
    {
        if (_world is not { } world) return;

        var table = world.Characters;
        for (int s = 0; s < table.HighWater; s++)
        {
            if (!table.Next[s].Alive || table.Cold(s) is not { } cold || Hitches.PostOf(cold) is not { } post) continue;

            hasher.Add(s);
            hasher.Add(Rng.StableHash(post));
        }
    }

    #endregion
}
