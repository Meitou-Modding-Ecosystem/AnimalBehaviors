using System.Text.Json;
using System.Text.RegularExpressions;
using Meitou.Localisation;

namespace AnimalBehaviors.Tests;

/// <summary>
/// Every catalog translates the same texts and keeps their placeholders.
/// </summary>
public sealed class LocalizationTests
{
    [Theory]
    [InlineData("fr_FR")]
    [InlineData("de_DE")]
    [InlineData("es_ES")]
    [InlineData("it_IT")]
    [InlineData("pt_BR")]
    [InlineData("ru_RU")]
    [InlineData("ja_JP")]
    [InlineData("ko_KR")]
    [InlineData("zh_CN")]
    public void Catalog_covers_the_same_messages_and_keeps_their_format_arguments(string locale)
    {
        var assembly = typeof(AnimalBehaviorsMod).Assembly;
        Dictionary<string, string> Read(string code)
        {
            using var stream = assembly.GetManifestResourceStream($"AnimalBehaviors.Locale.{code}.json");
            Assert.NotNull(stream);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(stream)!;
        }

        var catalog = Read(locale);
        Assert.Equal(Read("fr_FR").Keys.Order(), catalog.Keys.Order());
        foreach (var (source, translation) in catalog)
        {
            Assert.False(string.IsNullOrWhiteSpace(translation));
            Assert.Equal(Arguments(source), Arguments(translation));
            Assert.Equal(translation, AnimalText.Lookup(locale, source));
        }

        string[] required = [Hitches.Name, "Hitch here", HitchPosts.Full];
        Assert.All(required, source => Assert.True(catalog.ContainsKey(source), source));
        Assert.NotEqual("Hitch here", AnimalText.Lookup(locale, "Hitch here"));
        Assert.Contains("6", Loc.Format(AnimalText.Lookup(locale, HitchPosts.Full), 6));
        Assert.Equal("Hitch here", AnimalText.Lookup("en_GB", "Hitch here"));
    }

    static string[] Arguments(string text) =>
        Regex.Matches(text, @"\{\d+\}").Select(match => match.Value).Order().ToArray();
}
