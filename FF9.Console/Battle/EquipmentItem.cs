namespace FF9.Console.Battle;

public class EquipmentItem : Item
{
    public EquipmentItem()
    {
    }

    public EquipmentItem(EquipmentType type, int armor)
    {
        Type = type;
        Armor = armor;
    }

    public EquipmentType Type { get; init; }
    public int Armor { get; init; }
}