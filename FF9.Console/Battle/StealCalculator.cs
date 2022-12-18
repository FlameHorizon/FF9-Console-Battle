namespace FF9.Console.Battle;

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
        
        if (_randomProvider.Next16() % hitRate < _randomProvider.Next16() % evade)
            return null;
        
        byte stealRoll = _randomProvider.Next8();
        return stealRoll < target.StealableItemsRates[0] ? target.Steal(0) : null;
    }
}