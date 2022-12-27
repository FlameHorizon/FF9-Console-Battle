using FF9.ConsoleGame;
using FF9.ConsoleGame.Battle;
using FF9.ConsoleGame.Battle.Interfaces;
using FF9.ConsoleGame.Items;
using FluentAssertions;
using Moq;
using BattleEngine = FF9.ConsoleGame.Battle.BattleEngine;

namespace FF9.Tests;

public class BattleEngineTests
{
    [Fact]
    public void Init_Queue_ShouldOrderUnitsByAgility()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = InitialUnit.Warrior();

        BattleEngine engine = new BattleEngineBuilder()
            .WithPlayerUnit(thief)
            .WithPlayerUnit(warrior)
            .Build();

        engine.Queue.First().Should().Be(thief);
        engine.Queue.Skip(1).First().Should().Be(warrior);
    }

    [Fact]
    public void TurnAttack_DamageTaken_ShouldReduceHpOfTarget()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = InitialUnit.Warrior();

        int initialHp = warrior.Hp;

        BattleEngine engine = new BattleEngineBuilder()
            .WithPlayerUnit(thief)
            .WithPlayerUnit(warrior)
            .Build();

        engine.TurnAttack(source: thief, target: warrior);

        warrior.Hp.Should().Be(initialHp - engine.LastDamageValue);
    }

    [Fact]
    public void NextTurn_Queue_ShouldAdvanceToNextUnit()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = InitialUnit.Warrior();

        BattleEngine engine = new BattleEngineBuilder()
            .WithPlayerUnit(warrior)
            .WithPlayerUnit(thief)
            .Build();

        engine.Source.Should().Be(thief);
        engine.NextTurn();
        engine.Source.Should().Be(warrior);
    }

    [Fact]
    public void UnitsInBattleTest()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = InitialUnit.Warrior();

        BattleEngine engine = new BattleEngineBuilder()
            .WithPlayerUnit(thief)
            .WithPlayerUnit(warrior)
            .Build();

        engine.UnitsInBattle.Should().HaveCount(2);
        engine.UnitsInBattle.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void TurnAttack_Unit_ShouldRemoveDefenceStanceAfterItWasAttacked()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = InitialUnit.Warrior();

        BattleEngine engine = new BattleEngineBuilder()
            .WithPlayerUnit(thief)
            .WithPlayerUnit(warrior)
            .Build();

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
            .WithStealable(new Item?[] { null, null, null, new WeaponItem(ItemName.MageMasher) })
            .WithStealRates(new int[]
                { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue })
            .Build();

        IStealCalculator stealCalculator = GetAlwaysStealCalculator();

        BattleEngine engine = new BattleEngineBuilder()
            .WithPlayerUnit(thief)
            .WithEnemyUnit(warrior)
            .WithStealCalculator(stealCalculator)
            .Build();

        engine.TurnSteal(warrior);
        engine.LastStolenItem.Should().BeOfType(typeof(WeaponItem));
    }

    [Fact]
    public void LastStolenItem_Item_ShouldBeErasedAfterNextTurn()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = new UnitBuilder()
            .WithStealable(new Item?[] { null, null, null, new WeaponItem(ItemName.MageMasher) })
            .WithStealRates(new int[] { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue })
            .Build();

        IStealCalculator stealCalculator = GetAlwaysStealCalculator();

        BattleEngine engine = new BattleEngineBuilder()
            .WithPlayerUnit(thief)
            .WithEnemyUnit(warrior)
            .WithStealCalculator(stealCalculator)
            .Build();

        engine.TurnSteal(warrior);
        engine.NextTurn();

        engine.LastStolenItem.Should().BeNull();
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

    [Fact]
    public void LastDamage_Value_ShouldBeErasedAfterNextTurn()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = InitialUnit.Warrior();
        List<Unit> units = new() { thief, warrior };

        IPhysicalDamageCalculator alwaysHitTenCalculator =
            GetAlwaysHitTenCalculator();

        BattleEngine engine = new BattleEngineBuilder()
            .WithPlayerUnit(thief)
            .WithEnemyUnit(warrior)
            .WithPhysicalDamageCalculator(alwaysHitTenCalculator)
            .Build();

        engine.TurnAttack(units.Skip(1).First());
        engine.LastDamageValue.Should().Be(10);

        engine.NextTurn();
        engine.LastDamageValue.Should().Be(0);
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

        BattleEngine e = new BattleEngineBuilder()
            .WithPlayerUnit(player)
            .WithPlayerUnit(enemy)
            .Build();

        e.EnemyDefeated.Should().BeTrue();
    }

    [Fact]
    public void IsTurnAi_IsTrue_WhenAiIsNowTakingItsTurn()
    {
        BattleEngine be = new BattleEngineBuilder()
            .WithPlayerUnit(new UnitBuilder().AsPlayer().WithHp(1).Build())
            .WithEnemyUnit(new UnitBuilder().AsEnemy().WithHp(1).WithAgl(10).Build()
            ).Build();

        be.IsTurnAi.Should().BeTrue();
    }

    private static IEnumerable<Unit> GetAiUnits(int count)
    {
        return Enumerable
            .Range(0, count)
            .Select(_ => new UnitBuilder().AsEnemy().Build());
    }

    [Fact]
    public void AiAction_TakesMove_WhenItsTurn()
    {
        Unit playerUnit = new UnitBuilder().AsPlayer().WithHp(1).Build();
        Unit enemyUnit = new UnitBuilder().AsEnemy().WithHp(1).Build();

        BattleEngine be = new BattleEngineBuilder()
            .WithPlayerUnit(playerUnit)
            .WithEnemyUnit(enemyUnit)
            .Build();

        be.AiAction().Should().Be(BattleAction.Attack);
    }

    [Fact]
    public void Init_Accepts_UpToFourPartyMembers()
    {
        IEnumerable<Unit> playerParty = new[]
        {
            new UnitBuilder().AsPlayer().WithHp(1).WithAgl(10).Build(),
            new UnitBuilder().AsPlayer().WithHp(1).Build(),
            new UnitBuilder().AsPlayer().WithHp(1).Build(),
            new UnitBuilder().AsPlayer().WithHp(1).Build()
        };

        IEnumerable<Unit> enemyParty = new[]
        {
            new UnitBuilder().AsEnemy().WithHp(1).WithAgl(9).Build()
        };

        Unit highestAglPlayer = playerParty.MaxBy(p => p.Agl);
        Unit highestAglEnemy = enemyParty.MaxBy(e => e.Agl);

        IEnumerable<Unit> orderByAgl = new[]
        {
            highestAglPlayer,
            highestAglEnemy
        }.OrderByDescending(u => u.Agl);

        BattleEngine be = new BattleEngineBuilder()
            .WithPlayerParty(playerParty)
            .WithEnemyParty(enemyParty)
            .Build();

        be.UnitsInBattle.Should().HaveCount(5);
        be.Queue.First().Should().Be(orderByAgl.First());
        be.Queue.Skip(1).First().Should().Be(orderByAgl.Skip(1).First());
    }

    [Fact]
    public void PlayerDefeated_IsTrue_WhenPlayerPartyIsDead()
    {
        IEnumerable<Unit> playerParty = new[]
        {
            new UnitBuilder().AsPlayer().WithHp(0).Build(),
            new UnitBuilder().AsPlayer().WithHp(0).Build()
        };

        IEnumerable<Unit> enemyParty = GetAiUnits(1);

        BattleEngine e = new BattleEngineBuilder()
            .WithPlayerParty(playerParty)
            .WithEnemyParty(enemyParty)
            .Build();
        e.PlayerDefeated.Should().BeTrue();
    }

    [Fact]
    public void PlayerDefeated_IsFalse_WhenPlayerPartyIsNotDead()
    {
        IEnumerable<Unit> playerParty = new[]
        {
            new UnitBuilder().AsPlayer().WithHp(1).Build(),
            new UnitBuilder().AsPlayer().WithHp(1).Build()
        };

        IEnumerable<Unit> enemyParty = GetAiUnits(1);

        BattleEngine e = new BattleEngineBuilder()
            .WithPlayerParty(playerParty)
            .WithEnemyParty(enemyParty)
            .Build();
        e.PlayerDefeated.Should().BeFalse();
    }

    [Fact]
    public void Queue_RemovesUnit_WhenUnitDiesDuringCombat()
    {
        IEnumerable<Unit> playerParty = new[]
        {
            new UnitBuilder().AsPlayer().WithHp(1).Build(),
            new UnitBuilder().AsPlayer().WithHp(1).Build()
        };

        IEnumerable<Unit> enemyParty = new[]
        {
            new UnitBuilder().AsEnemy().WithHp(1).WithStr(2).Build()
        };

        BattleEngine e = new BattleEngineBuilder()
            .WithPlayerParty(playerParty)
            .WithEnemyParty(enemyParty)
            .WithPhysicalDamageCalculator(GetAlwaysHitTenCalculator())
            .Build();

        e.TurnAttack(enemyParty.First(), playerParty.First());
        e.NextTurn();

        e.Queue.Should().NotContain(playerParty.First());
    }

    [Fact]
    public void Queue_DoesNotQueueFirstUnit_WhenItIsDead()
    {
        IEnumerable<Unit> playerParty = new[]
        {
            new UnitBuilder().AsPlayer().WithName("a").WithHp(0).Build(),
            new UnitBuilder().AsPlayer().WithName("b").WithHp(1).Build()
        };

        IEnumerable<Unit> enemyParty = new[]
        {
            new UnitBuilder().AsEnemy().WithHp(0).Build()
        };

        BattleEngine e = new BattleEngineBuilder()
            .WithPlayerParty(playerParty)
            .WithEnemyParty(enemyParty)
            .Build();

        e.Queue.Should().NotContain(playerParty.First());
        e.Source.Should().Be(playerParty.Skip(1).First());
    }

    [Fact]
    public void Queue_DoesNotQueueFirstUnit_WhenItIsDead1()
    {
        IEnumerable<Unit> playerParty = new[]
        {
            new UnitBuilder().AsPlayer().WithName("a").WithHp(0).Build(),
            new UnitBuilder().AsPlayer().WithName("b").WithHp(1).Build()
        };

        IEnumerable<Unit> enemyParty = new[]
        {
            new UnitBuilder().AsEnemy().WithHp(0).Build()
        };

        BattleEngine e = new BattleEngineBuilder()
            .WithPlayerParty(playerParty)
            .WithEnemyParty(enemyParty)
            .Build();

        e.Queue.Should().NotContain(playerParty.First());
        e.Source.Should().Be(playerParty.Skip(1).First());
    }

    [Fact]
    public void Steal_AfterSuccess_MovesItemToOppsiteInventory()
    {
        Unit thief = InitialUnit.Thief();
        Unit warrior = new UnitBuilder()
            .WithStealable(new Item?[]
                { null, null, null, new UseableItem(ItemName.Potion, 1) })
            .WithStealRates(new int[]
                { byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue })
            .Build();

        IStealCalculator stealCalculator = GetAlwaysStealCalculator();

        BattleEngine engine = new BattleEngineBuilder()
            .WithPlayerUnit(thief)
            .WithEnemyUnit(warrior)
            .WithStealCalculator(stealCalculator)
            .Build();

        engine.TurnSteal(warrior);
        engine.NextTurn();

        engine.PlayerInventory.Should().Contain(i => i.Name == ItemName.Potion);
    }

    [Fact]
    public void TurnUseItem_Item_ShouldHaveAnEffectOnTarget()
    {
        Unit p = new UnitBuilder()
            .WithHp(100)
            .WithMaxHp(200)
            .Build();

        Unit e = new UnitBuilder().Build();

        BattleEngine engine = new BattleEngineBuilder()
            .WithPlayerUnit(p)
            .WithEnemyUnit(e)
            .WithPlayerInventory(new List<Item> { new UseableItem(ItemName.Potion, 1) })
            .Build();

        engine.SetItem(new UseableItem(ItemName.Potion, 1));
        engine.TurnUseItem(p);

        p.Hp.Should().Be(p.MaxHp);
    }

    [Fact]
    public void TurnUseItem_Item_ShouldRemoveItemFromInventory()
    {
        Unit p = new UnitBuilder()
            .WithHp(100)
            .WithMaxHp(200)
            .Build();

        Unit e = new UnitBuilder().Build();

        BattleEngine engine = new BattleEngineBuilder()
            .WithPlayerUnit(p)
            .WithPlayerInventory(new List<Item> { new UseableItem(ItemName.Potion, 1) })
            .WithEnemyUnit(e)
            .Build();

        engine.PlayerInventory.Should().ContainSingle();

        engine.SetItem(new UseableItem(ItemName.Potion, 1));
        engine.TurnUseItem(p);

        engine.PlayerInventory.Single(i => i.Name == ItemName.Potion).Count.Should().Be(0);
    }

    [Fact]
    public void TurnUseItem_PhoenixDown_ShouldPutUnitBackIntoTheQueue()
    {
        Unit deadUnit = new UnitBuilder()
            .WithHp(0)
            .WithMaxHp(100)
            .Build();

        Unit enemy = new UnitBuilder().Build();

        var pd = new UseableItem(ItemName.PhoenixDown, 1);

        BattleEngine e = new BattleEngineBuilder()
            .WithPlayerUnit(deadUnit)
            .WithEnemyUnit(enemy)
            .WithPlayerInventoryItem(pd)
            .Build();

        e.Queue.Should().NotContain(deadUnit);

        // Revive unit here by using Phoenix Down
        e.SetItem(pd);
        e.TurnUseItem(deadUnit);

        deadUnit.Hp.Should().BeGreaterThan(0);
        e.Queue.Should().Contain(deadUnit);
    }

    [Fact]
    public void TurnUseItem_PhoenixDown_ShouldNotChangeQueueWhenUsedOnLiving()
    {
        Unit aliveUnit = new UnitBuilder().Build();
        Unit enemy = new UnitBuilder().Build();

        var pd = new UseableItem(ItemName.PhoenixDown, 1);

        BattleEngine e = new BattleEngineBuilder()
            .WithPlayerUnit(aliveUnit)
            .WithEnemyUnit(enemy)
            .WithPlayerInventoryItem(pd)
            .Build();

        IEnumerable<Unit> before = e.Queue.ToList();

        e.SetItem(pd);
        e.TurnUseItem(aliveUnit);

        IEnumerable<Unit> after = e.Queue.ToList();
        before.Should().ContainInConsecutiveOrder(after);
    }
}