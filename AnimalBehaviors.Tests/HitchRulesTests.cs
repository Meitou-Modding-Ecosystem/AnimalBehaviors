namespace AnimalBehaviors.Tests;

/// <summary>
/// The dependents' terms for other factions' posts: free with no rule, the first refusal wins, a failing rule lets the animal stay, and a
/// failed dependent's rules go.
/// </summary>
public sealed class HitchRulesTests
{
    [Fact]
    public void No_rule_lets_the_animal_stay()
    {
        Assert.Null(new HitchRules().Ask(default, _ => { }));
    }

    [Fact]
    public void The_first_refusal_in_registration_order_wins()
    {
        var rules = new HitchRules();
        var asked = new List<string>();
        rules.Add("a", _ => { asked.Add("a"); return null; });
        rules.Add("b", _ => { asked.Add("b"); return "Pay 50 cats"; });
        rules.Add("c", _ => { asked.Add("c"); return "Never"; });

        Assert.Equal("Pay 50 cats", rules.Ask(default, _ => { }));
        Assert.Equal(["a", "b"], asked);
    }

    [Fact]
    public void A_rule_that_throws_is_logged_and_lets_the_animal_stay()
    {
        var rules = new HitchRules();
        var log = new List<string>();
        rules.Add("broken", _ => throw new InvalidOperationException("boom"));

        Assert.Null(rules.Ask(default, log.Add));
        Assert.Contains(log, line => line.Contains("broken") && line.Contains("boom"));
    }

    [Fact]
    public void A_failed_dependents_rules_are_dropped()
    {
        var rules = new HitchRules();
        rules.Add("gone", _ => "Refused");
        rules.Add("kept", _ => null);

        rules.Drop("gone");

        Assert.Equal(1, rules.Count);
        Assert.Null(rules.Ask(default, _ => { }));
    }
}
