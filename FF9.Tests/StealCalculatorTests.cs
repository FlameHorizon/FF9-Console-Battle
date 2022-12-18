using FF9.Console;
using FF9.Console.Battle;
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
            .WithStealRates(new[] { 255 })
            .WithStealable(new List<Item>() { eqItem })
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