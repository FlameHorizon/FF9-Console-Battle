// See https://aka.ms/new-console-template for more information

using FF9.ConsoleGame;
using FF9.ConsoleGame.Battle;
using FF9.ConsoleGame.Battle.Interfaces;

IEnumerable<Unit> CreateEnemyParty()
{
    int GetStealRateFromPercent(double value)
    {
        var result = (int)(byte.MaxValue * value);
        return result;
    }

    IEnumerable<Unit> units = new[]
    {
        new UnitBuilder()
            .AsEnemy()
            .WithName("Masked Man")
            .WithLv(1)
            .WithHp(100)
            .WithStr(9)
            .WithSpirit(10)
            .WithAgl(19)
            .WithStealable(new Item?[]
            {
                null,
                new UseableItem("Potion"),
                new EquipmentItem("Wrist"),
                new WeaponItem("Mage Masher", atk: 15)
            })
            .WithStealRates(new[]
            {
                GetStealRateFromPercent(1.0),
                GetStealRateFromPercent(1.0d),
                GetStealRateFromPercent(0.25d),
                GetStealRateFromPercent(0.0625d)
            })
            .Build()
    };
    return units;
}

IEnumerable<Unit> CreatePlayerParty()
{
    IEnumerable<Unit> enumerable = new[]
    {
        new UnitBuilder()
            .AsPlayer()
            .WithName("Zidane")
            .WithLv(1)
            .WithHp(105)
            .WithMp(36)
            .WithStr(21)
            .WithSpirit(23)
            .WithAgl(23)
            .Build(),

        new UnitBuilder()
            .AsPlayer()
            .WithName("Cinna")
            .WithLv(1)
            .WithHp(75)
            .WithMp(32)
            .WithStr(21)
            .WithSpirit(23)
            .WithAgl(23)
            .Build(),

        new UnitBuilder()
            .AsPlayer()
            .WithName("Marcus")
            .WithLv(1)
            .WithHp(90)
            .WithMp(22)
            .WithStr(21)
            .WithSpirit(23)
            .WithAgl(23)
            .Build()

    };
    return enumerable;
}

IEnumerable<Unit> playerParty = CreatePlayerParty();
IEnumerable<Unit> enemyParty = CreateEnemyParty();

BattleEngine btlEngine = new BattleEngineBuilder()
    .WithPlayerParty(playerParty)
    .WithEnemyParty(enemyParty)
    .Build();

var game = new Game(btlEngine);
game.Start();