using FF9.ConsoleGame;
using FF9.ConsoleGame.Battle;
using FF9.ConsoleGame.Battle.Interfaces;
using FluentAssertions;
using Moq;

namespace FF9.Tests;

public class BattleEngineTests
{
    [Fact]
    public void Init_Queue_ShouldOrderUnitsByAgility()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = InitialUnit.Warrior();

        var engine = new BattleEngine(thief, warrior);

        engine.Queue.First().Should().Be(thief);
        engine.Queue.Skip(1).First().Should().Be(warrior);
    }

    [Fact]
    public void TurnAttack_DamageTaken_ShouldReduceHpOfTarget()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = InitialUnit.Warrior();

        int initialHp = warrior.Hp;

        var engine = new BattleEngine(warrior, thief);
        engine.TurnAttack(source: thief, target: warrior);

        warrior.Hp.Should().Be(initialHp - engine.LastDamageValue);
    }

    [Fact]
    public void NextTurn_Queue_ShouldAdvanceToNextUnit()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = InitialUnit.Warrior();

        var engine = new BattleEngine(warrior, thief);

        engine.Source.Should().Be(thief);

        engine.NextTurn();

        engine.Source.Should().Be(warrior);
    }

    [Fact]
    public void UnitsInBattleTest()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = InitialUnit.Warrior();

        var engine = new BattleEngine(warrior, thief);

        engine.UnitsInBattle.Should().HaveCount(2);
        engine.UnitsInBattle.Should().OnlyHaveUniqueItems();
    }
    
    [Fact]
    public void TurnAttack_Unit_ShouldRemoveDefenceStanceAfterItWasAttacked()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = InitialUnit.Warrior();

        var engine = new BattleEngine(warrior, thief);
        engine.TurnDefence();
        engine.NextTurn();
        engine.TurnAttack(thief);

        thief.InDefenceStance.Should().BeFalse();
    }

    [Fact]
    public void LastStolenItem_Item_ShouldBeSetAfterSuccessfulSteal()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = new UnitBuilder()
            .WithStealable(new List<Item>( new [] { new WeaponItem() }))
            .WithStealRates(new int[] { byte.MaxValue })
            .Build();

        IStealCalculator stealCalculator = GetAlwaysStealCalculator();

        var engine = new BattleEngine(thief, warrior, stealCalculator);
        engine.TurnSteal();
        
        engine.LastStolenItem.Should().BeOfType(typeof(WeaponItem));
    }

    [Fact]
    public void LastStolenItem_Item_ShouldBeErasedAfterNextTurn()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = new UnitBuilder()
            .WithStealable(new List<Item>( new [] { new WeaponItem() }))
            .WithStealRates(new int[] { byte.MaxValue })
            .Build();

        IStealCalculator stealCalculator = GetAlwaysStealCalculator();

        var engine = new BattleEngine(thief, warrior, stealCalculator);
        engine.TurnSteal();
        engine.NextTurn();

        engine.LastStolenItem.Should().BeNull();
    }

    [Fact]
    public void LastDamage_Value_ShouldBeErasedAfterNextTurn()
    {
        var units = new List<Unit> { InitialUnit.Thief(), InitialUnit.Warrior() };
        var engine = new BattleEngine(units, GetAlwaysHitTenCalculator());
        
        engine.TurnAttack(units.Skip(1).First());
        engine.LastDamageValue.Should().Be(10);
        
        engine.NextTurn();
        engine.LastDamageValue.Should().Be(0);
    }
    
    private static IStealCalculator GetAlwaysStealCalculator()
    {
        Mock<IRandomProvider> randomProvider = new();
        randomProvider
            .SetupSequence(p => p.Next16())
            .Returns(1)
            .Returns(1);

        IStealCalculator calculator = new StealCalculator(randomProvider.Object);
        return calculator;
    }

    private static IPhysicalDamageCalculator GetAlwaysHitTenCalculator()
    {
        var randomProvider = new Mock<IRandomProvider>();
        randomProvider
            .SetupSequence(p => p.Next(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(1) // This roll controls if hit connects
            .Returns(10); // This roll control damage.

        return new PhysicalDamageCalculator(randomProvider.Object);
    }
    
    [Fact]
    public void EnemyDefeated_IsTrue_WhenAllEnemiesAreDead()
    {
        Unit player = new UnitBuilder().WithHp(10).AsPlayer().Build();
        Unit enemy = new UnitBuilder().WithHp(0).AsEnemy().Build();

        var e = new BattleEngine(player, enemy);
        e.EnemyDefeated.Should().BeTrue();
    }
}