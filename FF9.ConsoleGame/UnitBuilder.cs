using FF9.ConsoleGame.Battle;

namespace FF9.ConsoleGame;

public class UnitBuilder
{
    private int _lv = 1;
    private int _spr;
    private List<Item>? _items;
    private List<Item>? _stealable;
    private int[]? _rates;
    private int _hp;
    private bool _isPlayer;

    public UnitBuilder WithLv(int value)
    {
        _lv = value;
        return this;
    }

    public UnitBuilder WithSpirit(int value)
    {
        _spr = value;
        return this;
    }

    public UnitBuilder WithInventory(List<Item>? items)
    {
        _items = items;
        return this;
    }

    public Unit Build()
    {
        Unit u = new(string.Empty, _hp, 0, 0, 0, _lv, _isPlayer, _spr, _stealable, _rates);
        _items?.ForEach(i => u.PutIntoInventory(i));
        return u;
    }

    public UnitBuilder WithStealable(List<Item>? items)
    {
        _stealable = items;
        return this;
    }

    public UnitBuilder WithStealRates(int[] rates)
    {
        _rates = rates;
        return this;
    }

    public UnitBuilder WithHp(int value)
    {
        _hp = value;
        return this;
    }

    public UnitBuilder AsEnemy()
    {
        _isPlayer = false;
        return this;
    }

    public UnitBuilder AsPlayer()
    {
        _isPlayer = true;
        return this;
    }
}