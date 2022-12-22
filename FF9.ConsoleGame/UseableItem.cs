using FF9.ConsoleGame.Battle;

namespace FF9.ConsoleGame;

public class UseableItem : Item
{
    public UseableItem(string name) : base(name, 1)
    { }

    public UseableItem(string name, int count) :base (name, count)
    { }
}