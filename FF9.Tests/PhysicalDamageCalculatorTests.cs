using FF9.Console;
using FF9.Console.Battle;
using FluentAssertions;
using Moq;

namespace FF9.Tests;

public class PhysicalDamageCalculatorTests
{
    [Fact]
    public void TurnDefence_Damage_ShouldReduceByHalf()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = InitialUnit.Warrior();

        var randomProvider = new Mock<IRandomProvider>();
        randomProvider
            .SetupSequence(p => p.Next(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(1) // This roll controls if hit connects
            .Returns(warrior.Damage); // This roll control damage.

        var damageCalculator = new PhysicalDamageCalculator(randomProvider.Object);
        
        thief.PerformDefence();
        int actual = damageCalculator.Calculate(warrior.Damage, warrior.PhysicalHitRate, thief);

        actual.Should().Be(warrior.Damage / 2);
    }
}