using Meitou.Simulation.Production;

namespace AnimalBehaviors.Tests;

/// <summary>
/// The Hitched offer takes any faction's posts on an engine with <c>ModJobOffer.Owners</c> (a MeitouClient fork option), set by name so the mod still loads
/// on a release without it.
/// </summary>
public sealed class HitchOfferTests
{
    [Fact]
    public void The_offer_takes_any_factions_posts_where_the_engine_can()
    {
        var offer = Hitches.Offer(["post"]);
        var owners = typeof(ModJobOffer).GetProperty("Owners");

        Assert.Null(offer.Refusal());
        Assert.Equal(owners is not null, Hitches.OffersForeign);
        if (owners is not null) Assert.Equal(1, Convert.ToInt32(owners.GetValue(offer)));   // the fork's option: any faction's posts
    }
}
