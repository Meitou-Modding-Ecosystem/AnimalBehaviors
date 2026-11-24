using System.Text.Json;
using Meitou.Content;
using Meitou.Modding;

namespace AnimalBehaviors.Tests;

/// <summary>
/// The manifest and the code agree: the loader refuses a mod whose entry is missing or whose id differs from what its code registers under.
/// </summary>
public sealed class ManifestTests
{
    [Fact]
    public void The_manifest_names_the_entry_and_its_id()
    {
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "meitou.json")));
        var root = manifest.RootElement;

        var entry = typeof(AnimalBehaviorsMod).Assembly.GetType(root.GetProperty("entry").GetString()!);

        Assert.NotNull(entry);
        Assert.True(typeof(IMod).IsAssignableFrom(entry));
        Assert.Equal(AnimalBehaviorsMod.Id, root.GetProperty("id").GetString());
    }

    /// <summary>
    /// The engine's own reader accepts the manifest; a malformed one refuses the mod in game.
    /// </summary>
    [Fact]
    public void The_engine_reads_the_manifest()
    {
        var manifest = CodeModManifest.Read(Path.Combine(AppContext.BaseDirectory, "meitou.json"));

        Assert.Equal(AnimalBehaviorsMod.Id, manifest.Id);
        Assert.Empty(manifest.DependsOn);
    }
}
