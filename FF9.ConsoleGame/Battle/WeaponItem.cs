namespace FF9.ConsoleGame.Battle;

public class WeaponItem : Item
{
    public WeaponItem()
    { }

    public WeaponItem(string name, int atk = 0)
    
    {
        Name = name;
        Atk = atk;
    }

    public int Atk { get; private set; } = 0;
    public byte HitRateBonus { get; private set; } = 0;
}