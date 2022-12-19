// See https://aka.ms/new-console-template for more information

using FF9.ConsoleGame;
using FF9.ConsoleGame.Battle;
using FF9.ConsoleGame.UI;

IEnumerable<Unit> playerParty = new[]
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
        .Build(),
    
    new UnitBuilder()
        .AsPlayer()
        .WithName("Blank")
        .WithLv(1)
        .WithHp(105)
        .WithMp(24)
        .WithStr(21)
        .WithSpirit(23)
        .WithAgl(23)
        .Build()
};

IEnumerable<Unit> enemyParty = new[]
{
    new UnitBuilder()
        .AsEnemy()
        .WithName("Masked Man")
        .WithLv(1)
        .WithHp(188)
        .WithStr(9)
        .WithSpirit(10)
        .WithAgl(19)
        .WithStealable(new Item?[4]
        {
            null,
            new UseableItem("Potion"),
            new EquipmentItem("Wrist"),
            new WeaponItem("Mage Masher", atk: 15),
        })
        .WithStealRates(new int[]
        {
            GetStealRateFromPercent(1.0),
            GetStealRateFromPercent(1.0d),
            GetStealRateFromPercent(0.25d),
            GetStealRateFromPercent(0.0625d),
        })
        .Build()
};

int GetStealRateFromPercent(double value)
{
    var result = (int)((byte.MaxValue) * value);
    return result;
}

var btlEngine = new BattleEngine(playerParty, enemyParty);

var game = new Game(btlEngine);
game.Start();

public class UseableItem : Item
{
    public UseableItem(string name)
    {
        Name = name;
    }
}