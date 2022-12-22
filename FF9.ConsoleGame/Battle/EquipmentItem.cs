namespace FF9.ConsoleGame.Battle;

public class EquipmentItem : Item
{
    public EquipmentItem(string name) : base(name)
    { }

    public EquipmentItem(EquipmentType type, int armor) : base("Sword")
    {
        Type = type;
        Armor = armor;
    }

    public EquipmentType Type { get; init; }
    public int Armor { get; init; }
}