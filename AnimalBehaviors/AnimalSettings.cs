using Meitou.Modding;

namespace AnimalBehaviors;

/// <summary>
/// The player's settings (docs/animal-behaviors.md "Settings"), shown under the mod's row on the Settings screen. The system copies them at each
/// check.
/// </summary>
public sealed class AnimalSettings
{
    /// <summary>
    /// The pen's radius round a post, in game units (about 10 cm each). 100 is a circle the area of four large farms (a large farm's crop grid is
    /// about 90 x 90).
    /// </summary>
    [Setting("Pen radius", Hint = "How far hitched animals may roam from their post, in game units (100 = four large farms).",
        Min = 30, Max = 600, Step = 10)]
    public int PenRadius { get; set; } = 100;

    /// <summary>
    /// Whether hitched animals stroll about inside the pen while idle.
    /// </summary>
    [Setting("Grazing", Hint = "Idle hitched animals wander about inside their pen.")]
    public bool Grazing { get; set; } = true;

    /// <summary>
    /// A copy, for the simulation to read without the settings page changing it mid-check.
    /// </summary>
    public AnimalSettings Copy() => (AnimalSettings)MemberwiseClone();
}
