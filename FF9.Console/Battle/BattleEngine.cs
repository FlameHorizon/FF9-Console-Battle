using System.ComponentModel;

namespace FF9.Console.Battle;

public class BattleEngine
{
    public string ExecuteAction(PlayerAction? action, PlayerUnit source, OpponentUnit target)
    {
        string msg = action switch
        {
            PlayerAction.Attack => $"Player {source.Name} attacked {target.Name}",
            PlayerAction.Steal => $"Player {source.Name} stole {Steal(source, target).Name} from {target.Name}",
            PlayerAction.UseItem => "Player uses item",
            PlayerAction.Defend => "Player defends",
            PlayerAction.Change => "Player changes",
            null => string.Empty,
            _ => throw new InvalidEnumArgumentException()
        };

        return msg;
    }

    private static Item Steal(PlayerUnit source, OpponentUnit target)
    {
        return new Item() { Name = "Potion" };
    }
}

public class Item
{
    public string Name { get; init; }
}