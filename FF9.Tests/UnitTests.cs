using FF9.ConsoleGame;
using FF9.ConsoleGame.Battle;
using FluentAssertions;

namespace FF9.Tests;

public class UnitTests
{
    [Fact]
    public void Steal_Null_WhenNoStealableItemsAvailable()
    {
        Unit u = new UnitBuilder().WithStealable(new Item[4]).Build();

        u.Steal(0).Should().BeNull();
    }

    [Fact]
    public void Steal_Item_WhenStealableItemsAreAvailable()
    {
        Unit u = new UnitBuilder()
            .WithStealable(new Item?[4] { new WeaponItem("Mage Masher"), null, null, null })
            .Build();

        u.Steal(0)!.Name.Should().Be("Mage Masher");
        u.StealableItemsCount.Should().Be(0);
    }

    [Fact]
    public void Steal_ArgumentOutOfRangeException_WhenSlotIsLowerThanZero()
    {
        Unit u = new UnitBuilder().Build();

        u.Invoking(y => y.Steal(-1))
            .Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithMessage("Value must be in range 0 and 3 (Parameter 'slot')");
    }

    [Fact]
    public void Steal_ArgumentOutOfRangeException_WhenSlotIsHigherThanThree()
    {
        Unit u = new UnitBuilder().Build();

        u.Invoking(y => y.Steal(4))
            .Should()
            .Throw<ArgumentOutOfRangeException>()
            .WithMessage("Value must be in range 0 and 3 (Parameter 'slot')");
    }

    [Fact]
    public void Steal_Null_WhenAllItemsWereAlreadyStolen()
    {
        Unit u = new UnitBuilder()
            .WithStealable(new Item?[1] { new WeaponItem("Mage Masher") })
            .Build();

        u.Steal(0);

        u.Steal(0).Should().BeNull();
    }
}