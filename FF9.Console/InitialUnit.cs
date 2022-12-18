using FF9.Console.Battle;

namespace FF9.Console;

public static class InitialUnit
{
    public static Unit Warrior(string name = "", bool isPlayer = false) =>
        new(name, 35, 10, 8, 0, 1, isPlayer, 0, new List<Item>(), null);

    public static Unit Thief(string name = "", bool isPlayer = false) =>
        new(name, 30, 5, 15, 0, 1, isPlayer, 0, new List<Item>(), null);
}