using FF9.Console.Battle;

namespace FF9.Console;

public class PhysicalDamageCalculator : IPhysicalDamageCalculator
{
    private readonly IRandomProvider _randomProvider;

    public PhysicalDamageCalculator(IRandomProvider randomProvider)
    {
        _randomProvider = randomProvider;
    }
    
    public int Calculate(int damage, byte attackerHitRate, Unit target)
    {
        int sumOfArmor = target.Equipment
            .Where(e => e.Type == EquipmentType.Armor)
            .Sum(e => e.Armor);

        const int baseChanceToHit = 168;

        int targetEvadeRate = 48 + target.Agl;

        int chanceToHitBeforeEvade = Math.Min(baseChanceToHit + attackerHitRate, 255);
        int chanceToHit = chanceToHitBeforeEvade - targetEvadeRate;
        int rng = _randomProvider.Next(0, 200);

        if (rng > chanceToHit) 
            return 0;
        
        int rawDamage = _randomProvider.Next(damage, damage * 2) - sumOfArmor;
        if (target.InDefenceStance)
            rawDamage /= 2;
        
        return Math.Max(1, rawDamage);
    }
}