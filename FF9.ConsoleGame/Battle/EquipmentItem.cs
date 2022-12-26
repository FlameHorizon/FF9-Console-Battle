using FF9.ConsoleGame.Items;

namespace FF9.ConsoleGame.Battle;

public class EquipmentItem : Item
{
    public EquipmentItem(ItemName name) : base(name)
    { }

    public EquipmentItem(EquipmentType type, int armor) : base(ItemName.Sword)
    {
        Type = type;
        Armor = armor;
    }

    public EquipmentType Type { get; init; }
    public int Armor { get; init; }
}