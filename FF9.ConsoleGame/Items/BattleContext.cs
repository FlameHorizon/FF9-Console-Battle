using FF9.ConsoleGame.Battle;

namespace FF9.ConsoleGame.Items;

public record BattleContext(IUnit Source, IUnit Target, bool InCombat)
{
    public BattleContext() : this(null, null, false)
    {
    }
}