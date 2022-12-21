using FF9.ConsoleGame.Battle.Interfaces;

namespace FF9.ConsoleGame.Battle;

public class BattleEngine
{
    private Queue<Unit> _queue = new();
    
    // Here is an opportunity to allow clients to see which units are player
    // controlled and which are AI.
    private readonly IEnumerable<Unit> _playerUnits;
    private readonly IEnumerable<Unit> _enemyUnits;
    private readonly IPhysicalDamageCalculator _physicalDamageCalc;
    private readonly IStealCalculator _stealCalculator;
    private readonly List<Unit> _unitsInBattle;
    private Unit? _target;
    private List<Item> _playerInventory;

    public int LastDamageValue { get; private set; }

    public IEnumerable<Unit> UnitsInBattle
    {
        get => _unitsInBattle;
        private init => _unitsInBattle = value.ToList();
    }

    public BattleEngine(
        IEnumerable<Unit> playerUnits,
        IEnumerable<Unit> enemyUnits,
        IPhysicalDamageCalculator physicalDamageCalculator,
        IStealCalculator stealCalculator,
        IEnumerable<Item> playerInventory)
    {
        _playerUnits = playerUnits;
        _enemyUnits = enemyUnits;

        _unitsInBattle = new List<Unit>();
        _unitsInBattle.AddRange(_playerUnits);
        _unitsInBattle.AddRange(_enemyUnits);

        _physicalDamageCalc = physicalDamageCalculator;
        _stealCalculator = stealCalculator;
        _playerInventory = playerInventory.ToList();

        InitializeQueue();
    }

    /// <summary>
    /// Puts units into the queue accordingly to their agility value.
    /// </summary>
    private void InitializeQueue()
    {
        UnitsInBattle
            .Where(u => u.IsAlive)
            .OrderByDescending(u => u.Agl)
            .ToList()
            .ForEach(u => _queue.Enqueue(u));
    }

    public IEnumerable<Unit> Queue => _queue;

    /// <summary>
    /// Returns Unit which is now taking it's turn.
    /// </summary>
    public Unit Source => _queue.First();

    /// <summary>
    /// Returns Unit which is a target of an action taken by Source.
    /// </summary>
    /// It would be good to implement targeting mechanism next.
    public Unit Target { get; private set; }

    public bool EnemyDefeated
    {
        get
        {
            return UnitsInBattle
                .Where(u => u.IsPlayer == false)
                .All(u => u.IsAlive == false);
        }
    }

    public Item? LastStolenItem { get; private set; }
    public bool IsTurnAi => Source.IsPlayer == false;

    public bool PlayerDefeated
    {
        get
        {
            return UnitsInBattle
                .Where(u => u.IsPlayer)
                .All(u => u.IsAlive == false);
        }
    }

    public IEnumerable<Item> PlayerInventory => _playerInventory;

    public void TurnAttack() => TurnAttack(Source, Target);
    public void TurnAttack(Unit target) => TurnAttack(Source, target);

    public void TurnAttack(Unit source, Unit target)
    {
        if (!ReferenceEquals(_target, target))
        {
            _target = target;
        }

        int damageTaken = _physicalDamageCalc.Calculate(source.Damage, source.PhysicalHitRate, target);

        LastDamageValue = damageTaken;
        target.TakeDamage(damageTaken);
        target.RemoveDefenceStance();
    }

    public void TurnDefence() => TurnDefence(Source);

    public void TurnDefence(Unit source) => source.PerformDefence();

    /// <summary>
    /// This method allows the turn order to be advanced by removing
    /// the next unit from the queue and adding it back to the end,
    /// effectively shifting all the other units in the queue forward by one position.
    /// </summary>
    public void NextTurn()
    {
        // Remove unit which is dead from the queue.
        if (_target?.IsAlive == false)
        {
            _queue = new Queue<Unit>(_queue.Where(u => u != _target));
        }

        Unit u = _queue.Dequeue();
        _queue.Enqueue(u);

        LastStolenItem = null;
        LastDamageValue = 0;
    }

    /// <summary>
    /// This method allows unit to steal item from Target unit. If successfully, item is transferred to
    /// the inventory of stealer. Otherwise, turn passes.
    /// </summary>
    public void TurnSteal() => TurnSteal(Target);

    public void TurnSteal(Unit target)
    {
        Item? stolenItem = _stealCalculator.Steal(Source, target);

        // Means, nothing got stolen.
        if (stolenItem is null)
            return; //CouldNotStealAnything

        _playerInventory.Add(stolenItem);
        LastStolenItem = stolenItem;
    }

    public BattleAction AiAction()
    {
        return BattleAction.Attack;
    }

    public void SetTarget(Unit? newTarget)
    {
        if (newTarget is null)
            return;

        Target = newTarget;
    }
}