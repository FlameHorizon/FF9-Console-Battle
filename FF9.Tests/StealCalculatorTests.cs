using FF9.ConsoleGame;
using FF9.ConsoleGame.Battle;
using FluentAssertions;
using Moq;

namespace FF9.Tests;

public class StealCalculatorTests
{
    [Fact]
    public void Steal_Success_IfHitRateIsHigherThanEvade()
    {
        EquipmentItem eqItem = new();
        Unit thief = new UnitBuilder()
            .WithLv(1)
            .WithSpirit(100)
            .Build();
        
        Unit warrior = new UnitBuilder()
            .WithLv(1)
            .WithStealable(new Item?[] { null, null, null, eqItem })
            .WithStealRates(new int[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue })
            .Build();

        Mock<IRandomProvider> provider = new();
        provider.SetupSequence(p => p.Next16())
            .Returns(1)
            .Returns(1);

        StealCalculator calc = new(provider.Object);
        Item? item = calc.Steal(thief, warrior);
        item.Should().Be(eqItem);
    }
}