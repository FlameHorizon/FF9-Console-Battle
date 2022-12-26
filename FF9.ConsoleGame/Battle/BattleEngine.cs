using System.Reflection;
using FF9.ConsoleGame.Battle.Interfaces;
using FF9.ConsoleGame.Items;

namespace FF9.ConsoleGame.Battle;

public class BattleEngine
{
    // Engine should be able to recalculate queue once units are revived.

    private Queue<Unit> _queue = new();
    private readonly IPhysicalDamageCalculator _physicalDamageCalc;
    private readonly IStealCalculator _stealCalculator;
    private readonly List<Unit>_unitsInBattle;
    private readonly List<Item> _playerInventory;
    private Unit? _target;

    public BattleEngine(
        IEnumerable<Unit> playerUnits,
        IEnumerable<Unit> enemyUnits,
        IPhysicalDamageCalculator physicalDamageCalculator,
        IStealCalculator stealCalculator,
        IEnumerable<Item> playerInventory)
    {
        PlayerUnits = playerUnits;
        EnemyUnits = enemyUnits;

        _unitsInBattle = new List<Unit>();
        _unitsInBattle.AddRange(PlayerUnits);
        _unitsInBattle.AddRange(EnemyUnits);

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
    
    public IEnumerable<Unit> PlayerUnits { get; }
    public IEnumerable<Unit> EnemyUnits { get; }
    public Item? LastStolenItem { get; private set; }
    public bool IsTurnAi => Source.IsPlayer == false;
    public IEnumerable<Item> PlayerInventory => _playerInventory;
    public int LastDamageValue { get; private set; }

    /// <summary>
    /// Returns Unit which is now taking it's turn.
    /// </summary>
    public Unit Source => _queue.First();

    /// <summary>
    /// Returns Unit which is a target of an action taken by Source.
    /// </summary>
    public Unit? Target { get; private set; }

    public bool EnemyDefeated => EnemyUnits.All(u => u.IsAlive == false);

    public bool PlayerDefeated => PlayerUnits.All(u => u.IsAlive == false);

    public IEnumerable<Unit> UnitsInBattle => _unitsInBattle;
    
    public Item? ItemForUse { get; private set; }
    public Item? LastUsedItem { get; private set; }

    public void TurnAttack() => TurnAttack(Source, Target);
    public void TurnAttack(Unit target) => TurnAttack(Source, target);

    public void TurnAttack(Unit source, Unit? target)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        
        if (!ReferenceEquals(_target, target))
            _target = target;

        int damageTaken = _physicalDamageCalc.Calculate(
            source.Damage, 
            source.PhysicalHitRate, 
            target);

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
            _queue = new Queue<Unit>(_queue.Where(u => u != _target));

        Unit u = _queue.Dequeue();
        _queue.Enqueue(u);

        LastStolenItem = null;
        LastDamageValue = 0;
        LastUsedItem = null;
        ItemForUse = null;
    }

    /// <summary>
    /// This method allows unit to steal item from Target unit.
    /// If successfully, item is transferred to
    /// the inventory of stealer. Otherwise, turn passes.
    /// </summary>
    public void TurnSteal() => TurnSteal(Target);

    public void TurnSteal(Unit? target)
    {
        if (target == null) throw new ArgumentNullException(nameof(target));
        
        Item? stolenItem = _stealCalculator.Steal(Source, target);

        // Means, nothing got stolen.
        if (stolenItem is null)
            return; //CouldNotStealAnything

        _playerInventory.Add(stolenItem);
        LastStolenItem = stolenItem;
    }

    public BattleAction AiAction() => BattleAction.Attack;

    public void SetTarget(Unit? newTarget)
    {
        if (newTarget is null)
            return;

        Target = newTarget;
    }

    public void SetItem(Item? newItem)
    {
        if (newItem is null)
            return;

        ItemForUse = newItem;
    }

    public void TurnUseItem(Unit target) => TurnUseItem(target, ItemForUse);
    
    public void TurnUseItem(Unit target, Item? item)
    {
        if (target == null) 
            throw new ArgumentNullException(nameof(target));
        
        if (!ReferenceEquals(_target, target))
            _target = target;

        if (item is null)
            throw new InvalidOperationException("No item was set for this action");
        
        int count = _playerInventory.Single(i => i.Name == item.Name).Count;

        if (count <= 0)
            throw new InvalidOperationException(
                $"Can't use item {item.Name} because there is non in inventory.");

        UseItem(item.Name, Source, target);
        

        
        _playerInventory.Single(i => i.Name == item.Name).Count--;
        LastUsedItem = item;
    }
    
    public void UseItem(ItemName name) => UseItem(name, Source, Target);
    
    private static void UseItem(ItemName name, Unit source, Unit target)
    {
        IUseable itemScript = FindItemScript(name);

        // Create battle context and use method for the item
        var ctx = new BattleContext(source, target, InCombat: true);
        itemScript.Use(ctx);
    }
    
    private static IUseable FindItemScript(ItemName name)
    {
        // Get class by name.
        Type? type = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .SingleOrDefault(t => t.IsClass
                                  && t.GetInterfaces().Any(i => i.Name == nameof(IUseable))
                                  && t.Namespace == "FF9.ConsoleGame.Items"
                                  && t.Name == name.ToString());

        if (type is null)
        {
            var msg = $"Script for item {name.ToString()} can't be found.";
            throw new InvalidOperationException(msg);
        }

        // Initialize it via constructor.
        ConstructorInfo? ctor = type.GetConstructor(Type.EmptyTypes);

        if (ctor is null)
        {
            string msg = $"Script for item {name.ToString()} " +
                         "does not have parameterless constructor.";
            throw new InvalidOperationException(msg);
        }

        if (ctor.Invoke(Array.Empty<object>()) is not IUseable itemScript)
        {
            var msg = $"Object for an item wasn't created. Type {type.Name}.";
            throw new InvalidOperationException(msg);
        }

        return itemScript;
    }
    
    public void GameOver()
    {
        int cnt = PlayerInventory.Single(i => i.Name == ItemName.PhoenixPinion).Count;
        var phoenixAppearChance = (decimal)(cnt / 256.0d);

        decimal roll = Random.Shared.NextDecimalSample();

        if (phoenixAppearChance <= roll)
        {
            // Show phoenix and revive party.
        }
        else
        {
            // Game over, really.
        }
    }
}