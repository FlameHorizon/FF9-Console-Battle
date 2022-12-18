namespace FF9.Console.Battle;

public interface IPhysicalDamageCalculator
{
    int Calculate(int damage, byte attackerHitRate, Unit target);
}