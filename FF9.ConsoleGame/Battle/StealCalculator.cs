using FF9.ConsoleGame.Battle.Interfaces;
using FF9.ConsoleGame.Items;

namespace FF9.ConsoleGame.Battle;

public class StealCalculator : IStealCalculator
{
    private readonly IRandomProvider _randomProvider;

    public StealCalculator() : this(new RandomProvider()) { }
    
    public StealCalculator(IRandomProvider randomProvider)
    {
        _randomProvider = randomProvider;
    }
    
    public Item? Steal(Unit source, Unit target)
    {
        int hitRate = source.Lv + source.Spirit;
        int evade = target.Lv;

        int sourceRoll = _randomProvider.Next16() % hitRate;
        int targetRoll = _randomProvider.Next16() % evade;
        if (sourceRoll < targetRoll)
            return null; // Could not steal anything

        Item? itemStolen;    
        if (_randomProvider.Next8() < target.StealableItemsRates[3])
            itemStolen = target.Steal(3);
        else if (_randomProvider.Next8() < target.StealableItemsRates[2])
            itemStolen = target.Steal(2);
        else if (_randomProvider.Next8() < target.StealableItemsRates[1])
            itemStolen = target.Steal(1);
        else if (_randomProvider.Next8() < target.StealableItemsRates[0])
            itemStolen = target.Steal(0);
        else
            itemStolen = null;

        return itemStolen;
    }
}