namespace FF9.ConsoleGame;

public class RandomProvider : IRandomProvider
{
    public int Next() => Random.Shared.Next();

    public int Next(int maxValue) => Random.Shared.Next(maxValue);

    public int Next(int minValue, int maxValue)
    {
        return Random.Shared.Next(minValue, maxValue);
    }
    
    public short Next16() => (short)Random.Shared.Next(0, 65536);

    public byte Next8() => (byte)Random.Shared.Next(0, 256);
}