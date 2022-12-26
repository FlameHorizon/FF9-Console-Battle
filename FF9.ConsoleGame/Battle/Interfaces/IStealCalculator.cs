using FF9.ConsoleGame.Items;

namespace FF9.ConsoleGame.Battle.Interfaces;

public interface IStealCalculator
{
    Item? Steal(Unit source, Unit target);
}