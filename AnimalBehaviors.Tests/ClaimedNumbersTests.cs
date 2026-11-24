namespace AnimalBehaviors.Tests;

/// <summary>
/// Every task type Animal Behaviors claims is in the README's <c>## Numbers</c> list, which <see cref="NumbersTests"/> keeps inside the block.
/// </summary>
public sealed class ClaimedNumbersTests
{
    [Fact]
    public void Every_claimed_task_type_is_listed_in_the_readme() => Assert.Contains((int)Hitches.Type, NumbersTests.Listed());
}
