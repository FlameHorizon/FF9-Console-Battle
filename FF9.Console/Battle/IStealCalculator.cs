namespace FF9.Console.Battle;

public interface IStealCalculator
{
    Item? Steal(Unit source, Unit target);
}