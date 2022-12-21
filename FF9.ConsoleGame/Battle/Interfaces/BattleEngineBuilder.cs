namespace FF9.ConsoleGame.Battle.Interfaces;

public class BattleEngineBuilder
{
    private List<Unit> _playerUnits = new();
    private List<Unit> _enemyUnits = new();
    private IPhysicalDamageCalculator _physicalDamageCalculator =
        new PhysicalDamageCalculator(new RandomProvider());
    
    private IStealCalculator _stealCalculator = new StealCalculator();
    private List<Item> _inventory = new();

    public BattleEngineBuilder WithPlayerUnit(Unit unit)
    {
        _playerUnits.Add(unit);
        return this;
    }

    public BattleEngineBuilder WithPlayerParty(IEnumerable<Unit> units)
    {
        _playerUnits = units.ToList();
        return this;
    }

    public BattleEngineBuilder WithEnemyUnit(Unit unit)
    {
        _enemyUnits.Add( unit);
        return this;
    }

    public BattleEngineBuilder WithEnemyParty(IEnumerable<Unit> units)
    {
        _enemyUnits = units.ToList();
        return this;
    }

    public BattleEngineBuilder WithPhysicalDamageCalculator(
        IPhysicalDamageCalculator physicalDamageCalculator)
    {
        _physicalDamageCalculator = physicalDamageCalculator;
        return this;
    }

    public BattleEngineBuilder WithStealCalculator(IStealCalculator stealCalculator)
    {
        _stealCalculator = stealCalculator;
        return this;
    }

    public BattleEngineBuilder WithPlayerInventoryItem(Item item)
    {
        _inventory.Add(item);
        return this;
    }

    public BattleEngineBuilder WithPlayerInventory(IEnumerable<Item> items)
    {
        _inventory = items.ToList();
        return this;
    }

    public BattleEngine Build()
    {
        return new BattleEngine(
            _playerUnits,
            _enemyUnits,
            _physicalDamageCalculator,
            _stealCalculator,
            _inventory);
    }
}