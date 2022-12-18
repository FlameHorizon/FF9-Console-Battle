namespace FF9.Console.Battle;

public class WeaponItem : Item
{
    public int Attack { get; private set; } = 0;
    public byte HitRateBonus { get; private set; } = 0;
}