namespace FF9.ConsoleGame.Battle;

public abstract class Item
{
    public string Name { get; protected set; } = string.Empty;
    public int Count { get; set; } = 0;

    public Item(string name) : this(name, 1)
    { }

    public Item(string name, int count)
    {
        Name = name;
        Count = count;
    }
}