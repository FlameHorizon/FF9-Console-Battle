using FF9.ConsoleGame.Items;

namespace FF9.ConsoleGame.Battle;

public class WeaponItem : Item
{
    public WeaponItem(ItemName name, int atk = 0) : base (name)
    {
        Atk = atk;
    }

    public int Atk { get; private set; } = 0;
    public byte HitRateBonus { get; private set; } = 0;
}