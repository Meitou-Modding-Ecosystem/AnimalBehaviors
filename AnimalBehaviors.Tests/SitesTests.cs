using Meitou.Data;
using Meitou.Data.Fcs;

namespace AnimalBehaviors.Tests;

/// <summary>
/// The hitching-post marks dependents put on their BUILDING records (docs/depending-on-animal-behaviors.md "Hitching posts"): marking keeps the
/// marks a record has, and the framework finds every marked post and its capacity.
/// </summary>
public sealed class SitesTests
{
    /// <summary>
    /// A BUILDING record as a dependent's data phase makes it.
    /// </summary>
    static FcsRecord Building(string id) => new() { RecordType = FcsRecordType.Building, StringId = id, Name = id };

    [Fact]
    public void Marking_twice_or_beside_another_behaviour_keeps_one_mark_each()
    {
        var post = Building("1-a.mod");
        post.Strings[Sites.Field] = "Herd";

        Sites.MarkHitch(post, 6);
        Sites.MarkHitch(post, 6);

        Assert.Equal("herd,hitch", post.Strings[Sites.Field]);
        Assert.Equal(6, post.Ints[Sites.CapacityField]);
    }

    [Fact]
    public void Every_mods_posts_share_the_offer_and_keep_their_own_capacity()
    {
        var husbandry = Building("1-husbandry.mod");
        var other = Building("2-ranch.mod");
        var shed = Building("3-ranch.mod");
        Sites.MarkHitch(husbandry, 6);
        Sites.MarkHitch(other);
        var records = Records(husbandry, other, shed);

        Assert.Equal(["1-husbandry.mod", "2-ranch.mod"], Sites.Marked(records, Sites.Hitch));
        Assert.Equal(new Dictionary<string, int> { ["1-husbandry.mod"] = 6 }, Sites.Capacities(records));
    }

    [Fact]
    public void Animal_sexes_keep_the_roll_they_had_under_husbandry()
    {
        var purpose = (Meitou.Simulation.RngPurpose)(uint)(Meitou.Simulation.Rng.StableHash("husbandry") | 0x8000_0000UL);

        Assert.Equal(purpose, Sexes.Purpose);
    }

    /// <summary>
    /// The records as the game reads them.
    /// </summary>
    static List<GameRecord> Records(params FcsRecord[] records)
    {
        var file = new FcsFile();
        file.Records.AddRange(records);
        var game = new GameDatabase();
        game.Apply(file, "test.mod");
        return game.OfType(FcsRecordType.Building).ToList();
    }
}
