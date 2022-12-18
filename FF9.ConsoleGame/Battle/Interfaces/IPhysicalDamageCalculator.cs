namespace FF9.ConsoleGame.Battle.Interfaces;

public interface IPhysicalDamageCalculator
{
    int Calculate(int damage, byte attackerHitRate, Unit target);
}