using Meitou.Data.Gameplay.Bodies;
using Meitou.Modding;
using Meitou.Simulation;
using Meitou.Simulation.Bodies;
using Meitou.Simulation.Construction;

namespace AnimalBehaviors;

/// <summary>
/// Animal Behaviors' public surface (docs/depending-on-animal-behaviors.md), implemented by the entry type: a dependent gets it with
/// <c>c.Dependency&lt;IAnimalBehaviorsApi&gt;("animal.behaviors")</c> in its own <c>Initialize</c>.
/// </summary>
public interface IAnimalBehaviorsApi
{
    /// <summary>
    /// The registrations of the calling mod, kept under its id (<c>c.Id</c>). Allowed only before the first world is built (during dependents'
    /// <c>Initialize</c>).
    /// </summary>
    IAnimalBehaviorsRegistry For(ModContext c);

    /// <summary>
    /// What animals of a race may do: a registration for the race (the last loaded wins), else the built-in list, else the default
    /// (docs/animal-behaviors.md "Supported animals").
    /// </summary>
    AnimalSupport SupportOf(RaceData race);

    /// <summary>
    /// Whether the character is an animal whose race supports all of <paramref name="what"/>.
    /// </summary>
    bool Supports(CharacterCold cold, AnimalSupport what);

    /// <summary>
    /// Every registered animal kind, the built-in ones first, then dependents' in load order.
    /// </summary>
    IReadOnlyList<AnimalKind> Kinds { get; }

    /// <summary>
    /// The player's pen settings, live: copy them (<see cref="AnimalSettings.Copy"/>) at each check, as for <see cref="Pens.Keep"/> round a
    /// dependent's own work post.
    /// </summary>
    AnimalSettings Settings { get; }

    /// <summary>
    /// The framework's state in the world being built or last built: a dependent's system factory runs after the framework's for the same world,
    /// so it reads that world's state here. Null before the first world.
    /// </summary>
    IAnimalState? Current { get; }

    /// <summary>
    /// The framework's state in a world; null when the world does not run it. Serial steps of that world only.
    /// </summary>
    IAnimalState? In(World world);
}

/// <summary>
/// One mod's registrations with Animal Behaviors (<see cref="IAnimalBehaviorsApi.For"/>).
/// </summary>
public interface IAnimalBehaviorsRegistry
{
    /// <summary>
    /// Says what animals of a race may do. A later registration for the same race (from this mod or one loaded after it) replaces this one.
    /// </summary>
    void AddAnimal(AnimalKind kind);

    /// <summary>
    /// Adds terms for hitching to another faction's post (docs/depending-on-animal-behaviors.md "Other factions' posts"): asked when an animal is
    /// hitched there and at each new game day it stays, in registration order; the first refusal unhitches it. With no rule such posts are free.
    /// </summary>
    void AddHitchRule(HitchRule rule);
}

/// <summary>
/// Animal Behaviors in one world (docs/depending-on-animal-behaviors.md "State"). Serial steps only.
/// </summary>
public interface IAnimalState
{
    /// <summary>
    /// The animals' sexes; a mod that makes an animal with a set sex writes it to <see cref="Sexes.Assigned"/> (saved by the framework).
    /// </summary>
    Sexes Sexes { get; }

    /// <summary>
    /// Hitches to put back at the next serial step, by portrait serial and post site id (a dependent's older save that kept them).
    /// </summary>
    void ImportHitches(IEnumerable<(uint Serial, string Post)> hitches);

    /// <summary>
    /// How many animals a hitching post holds (its record's <see cref="Sites.CapacityField"/>); 0 for no limit.
    /// </summary>
    int Capacity(BuildSite post);

    /// <summary>
    /// Hitches the animal in a slot to a post when the post has room (a newborn to its mother's post); false, leaving it unhitched with a warning
    /// over it, when full.
    /// </summary>
    bool TryHitch(World world, int slot, BuildSite post);

    /// <summary>
    /// The hitching post (any faction's) the animal in a slot is assigned to, or null (not hitched, dead, or its post is gone or unfinished).
    /// Reads only.
    /// </summary>
    BuildSite? PostOf(World world, int slot);

    /// <summary>
    /// The living animals assigned to a hitching post, in slot order. Reads only.
    /// </summary>
    IReadOnlyList<Hitched> HitchedTo(World world, BuildSite post);
}

/// <summary>
/// What an animal may do (docs/animal-behaviors.md "Supported animals"); each flag is read by the mod that gives the behaviour.
/// </summary>
[Flags]
public enum AnimalSupport
{
    /// <summary>
    /// Nothing (it can still be hitched, so it keeps to a pen).
    /// </summary>
    None = 0,

    /// <summary>
    /// Hitched adult pairs of the race breed at a Hitching Post.
    /// </summary>
    Breeding = 1,

    /// <summary>
    /// It learns skills at the Training Post.
    /// </summary>
    Training = 2,

    /// <summary>
    /// A wild one, knocked out, can be caught in a Beast Capsule (summoning it makes it the player's).
    /// </summary>
    Taming = 4,

    /// <summary>
    /// One of the player's own can be put in a Beast Capsule and summoned again.
    /// </summary>
    Summoning = 8,

    /// <summary>
    /// Everything.
    /// </summary>
    All = Breeding | Training | Taming | Summoning,
}

/// <summary>
/// One registered animal race: the RACE record's string id or its name (matched without case, the id first), what it supports, the mod that
/// registered it, and a note for the list (docs/animal-behaviors.md "Supported animals").
/// </summary>
public sealed record AnimalKind(string Race, AnimalSupport Support, string Note = "")
{
    /// <summary>
    /// The id of the mod that registered it (set by the registry).
    /// </summary>
    public string Owner { get; init; } = "";
}
