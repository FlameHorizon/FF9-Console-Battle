using FF9.ConsoleGame.Battle;

namespace FF9.ConsoleGame;

public static class InitialUnit
{
    public static Unit Warrior(string name = "", bool isPlayer = false) =>
        new(name, 35, 0, 10, 8, 0, 1, isPlayer, 0, new Item[3], null);

    public static Unit Thief(string name = "", bool isPlayer = false) =>
        new(name, 30, 0, 5, 15, 0, 1, isPlayer, 0, new Item[3], null);
}