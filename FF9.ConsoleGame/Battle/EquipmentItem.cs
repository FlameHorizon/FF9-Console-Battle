namespace FF9.ConsoleGame.Battle;

public class EquipmentItem : Item
{
    public EquipmentItem()
    { }

    public EquipmentItem(string name)
    {
        Name = name;
    }

    public EquipmentItem(EquipmentType type, int armor)
    {
        Type = type;
        Armor = armor;
    }

    public EquipmentType Type { get; init; }
    public int Armor { get; init; }
}