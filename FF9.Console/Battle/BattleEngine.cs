using System.ComponentModel;

namespace FF9.Console.Battle;

public class BattleEngine
{
    private readonly Unit _unit1;
    private readonly Unit _unit2;
    private readonly Queue<Unit> _queue = new();

    public int LastDamageValue { get; private set; }

    public IEnumerable<Unit> UnitsInBattle { get; init; }
    
    public BattleEngine(Unit unit1, Unit unit2)
    {
        _unit1 = unit1;
        _unit2 = unit2;
        InitializeQueue();
        CurrentUnitTurn = _queue.First();
        UnitsInBattle = new List<Unit> { _unit1, _unit2 };
    }

    private void InitializeQueue()
    {
        var units = new List<Unit> { _unit1, _unit2 };
        List<Unit> orderByAgility = units.OrderByDescending(u => u.Agility).ToList();
        foreach (Unit u in orderByAgility)
        {
            _queue.Enqueue(u);
        }
    }

    public IEnumerable<Unit> Queue => _queue;
    public Unit CurrentUnitTurn { get; private set; }

    public void TurnAttack(Unit target) => TurnAttack(CurrentUnitTurn, target);

    private void TurnAttack(Unit source, Unit target)
    {
        int damageTaken = target.CalculateDamageTaken(source.Attack);
        LastDamageValue = damageTaken;
        target.TakeDamage(damageTaken);
    }

    public void TurnDefence() => TurnDefence(CurrentUnitTurn);

    private static void TurnDefence(Unit source) => source.PerformDefence();

    /// <summary>
    /// This method allows the turn order to be advanced by removing
    /// the next unit from the queue and adding it back to the end,
    /// effectively shifting all the other units in the queue forward by one position.
    /// </summary>
    public void NextTurn()
    {
        Unit u = _queue.Dequeue();
        CurrentUnitTurn = _queue.First();
        _queue.Enqueue(u);
    }
}