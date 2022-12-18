namespace FF9.ConsoleGame;

public interface IRandomProvider
{
    int Next();
    int Next(int maxValue);
    int Next(int minValue, int maxValue);
    byte Next8();
    short Next16();

}