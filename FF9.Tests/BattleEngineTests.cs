using FF9.Console;
using FF9.Console.Battle;
using FluentAssertions;


namespace FF9.Tests;

public class BattleEngineTests
{
    [Fact]
    public void InitTest()
    {
        var s = new Unit(string.Empty, 2, 0, 0, 0);
        var t = new Unit(string.Empty, 1, 0, 0, 0);

        var engine = new BattleEngine(s, t);

        engine.Queue.First().Should().Be(s);
        engine.Queue.Skip(1).First().Should().Be(t);
    }

    [Fact]
    public void CurrentPlayerTurnTest()
    {
        var s = new Unit(string.Empty, 2, 0, 1, 0);
        var t = new Unit(string.Empty, 1, 0, 0, 0);

        var engine = new BattleEngine(s, t);

        engine.CurrentUnitTurn.Should().Be(s);
    }

    [Fact]
    public void TurnAttackTest()
    {
        var s = new Unit(string.Empty, 2, 1, 1, 0);
        var t = new Unit(string.Empty, 1, 0, 0, 0);
        
        var engine = new BattleEngine(s, t);
        engine.TurnAttack(t);

        t.Health.Should().Be(0);
        t.IsAlive.Should().BeFalse();
    }

    [Fact]
    public void TurnDefenceTest()
    {
        var s = new Unit(string.Empty, 2, 1, 0, 10);
        var t = new Unit(string.Empty, 1, 0, 0, 0);

        var engine = new BattleEngine(s, t);
        engine.TurnDefence();

        s.Defence.Should().Be(11);
    }

    [Fact]
    public void TurnAttack_TakingIntoAccountDefence_Test()
    {
        var s = new Unit(string.Empty, 10, 5, 0, 2);
        var t = new Unit(string.Empty, 10, 2, 0, 2);
        
        var engine = new BattleEngine(s, t);
        engine.TurnAttack(t);

        t.Health.Should().Be(7);
    }

    [Fact]
    public void NextTurnTest()
    {
        var s = new Unit(string.Empty, 2, 0, 0, 0);
        var t = new Unit(string.Empty, 1, 0, 0, 0);

        var engine = new BattleEngine(s, t);

        engine.CurrentUnitTurn.Should().Be(s);
        
        engine.NextTurn();

        engine.CurrentUnitTurn.Should().Be(t);
    }

    [Fact]
    public void UnitsInBattleTest()
    {
        var s = new Unit(string.Empty, 2, 0, 0, 0);
        var t = new Unit(string.Empty, 1, 0, 0, 0);

        var engine = new BattleEngine(s, t);

        engine.UnitsInBattle.Should().HaveCount(2);
        engine.UnitsInBattle.Should().OnlyHaveUniqueItems();
    }
}



































