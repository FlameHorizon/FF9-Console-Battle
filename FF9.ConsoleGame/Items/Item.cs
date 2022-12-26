namespace FF9.ConsoleGame.Items;

public abstract class Item
{
    public ItemName Name { get; protected set; }
    public int Count { get; set; } = 0;

    public Item(ItemName name) : this(name, 1)
    { }

    public Item(ItemName name, int count)
    {
        Name = name;
        Count = count;
    }
}