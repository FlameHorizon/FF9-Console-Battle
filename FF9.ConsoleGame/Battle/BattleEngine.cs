using FF9.ConsoleGame.Battle.Interfaces;
using FF9.ConsoleGame.Items;
using System.Reflection;

namespace FF9.ConsoleGame.Battle;

/// <summary>
/// Provides game logic for a battle.
/// </summary>
public class BattleEngine
{
    private Queue<Unit> _queue = new();
    private readonly IPhysicalDamageCalculator _physicalDamageCalc;
    private readonly IStealCalculator _stealCalculator;
    private readonly List<Unit> _unitsInBattle;
    private readonly List<Item> _playerInventory;
    private Unit? _target;

    /// <summary>
    /// Initializes a new instance of the <see cref="BattleEngine"/> class.
    /// </summary>
    /// <param name="playerUnits">The player units.</param>
    /// <param name="enemyUnits">The enemy units.</param>
    /// <param name="physicalDamageCalculator">The physical damage calculator.</param>
    /// <param name="stealCalculator">The steal calculator.</param>
    /// <param name="playerInventory">The player inventory.</param>
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

    /// <summary>
    /// Gets the queue of units in the battle.
    /// </summary>
    public IEnumerable<Unit> Queue => _queue;

    /// <summary>
    /// Gets the player units.
    /// </summary>
    public IEnumerable<Unit> PlayerUnits { get; }

    /// <summary>
    /// Gets the enemy units.
    /// </summary>
    public IEnumerable<Unit> EnemyUnits { get; }

    /// <summary>
    /// Gets the last stolen item.
    /// </summary>
    public Item? LastStolenItem { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the turn is controlled by AI.
    /// </summary>
    public bool IsTurnAi => Source.IsPlayer == false;

    /// <summary>
    /// Gets the player inventory.
    /// </summary>
    public IEnumerable<Item> PlayerInventory => _playerInventory;

    /// <summary>
    /// Gets the last damage value.
    /// </summary>
    public int LastDamageValue { get; private set; }

    /// <summary>
    /// Gets the unit that is taking its turn.
    /// </summary>
    public Unit Source => _queue.First();

    /// <summary>
    /// Gets or sets the unit that is the target of an action taken by the Source unit.
    /// </summary>
    public Unit? Target { get; set; }

    /// <summary>
    /// Gets a value indicating whether all the enemy units are defeated.
    /// </summary>
    public bool EnemyDefeated => EnemyUnits.All(u => u.IsAlive == false);

    /// <summary>
    /// Gets a value indicating whether all the enemy units are defeated.
    /// </summary>
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

    /// <summary>
    /// Steals an item from a target unit and adds it to the player's inventory.
    /// </summary>
    /// <param name="target">The unit to steal an item from.</param>
    /// <exception cref="ArgumentNullException">Thrown if the target is null.</exception>
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

    /// <summary>
    /// Returns an attack action for the AI.
    /// </summary>
    /// <returns>The attack action.</returns>
    public BattleAction AiAction() => BattleAction.Attack;

    /// <summary>
    /// Sets the unit that is the target of an action taken by the Source unit.
    /// </summary>
    /// <param name="newTarget">The new target unit.</param>
    public void SetTarget(Unit? newTarget)
    {
        if (newTarget is null)
            return;

        Target = newTarget;
    }

    /// <summary>
    /// Sets the item that the unit will use in the next turn.
    /// </summary>
    /// <param name="newItem">The new item.</param>
    public void SetItem(Item? newItem)
    {
        if (newItem is null)
            return;

        ItemForUse = newItem;
    }

    //// <summary>
    /// Uses the item set in the item for use property on a target unit.
    /// </summary>
    /// <param name="target">The target unit.</param>
    public void TurnUseItem(Unit target) => TurnUseItem(target, ItemForUse);

    /// <summary>
    /// Uses the specified item on the target unit.
    /// </summary>
    /// <param name="target">The target unit.</param>
    /// <param name="item">The item to use.</param>
    /// <exception cref="ArgumentNullException">Thrown if the target is null.</exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no item was set for this action, or if there are no items of that type in the player's inventory.
    /// </exception>
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

        bool wasAilve = target.IsAlive;
        UseItem(item.Name, Source, target);

        if (item.Name == ItemName.PhoenixDown && wasAilve == false)
            RecalculateQueue(target);

        _playerInventory.Single(i => i.Name == item.Name).Count--;
        LastUsedItem = item;
    }

    /// <summary>
    /// Use an item on the currently set target.
    /// </summary>
    /// <param name="name">The name of the item to use.</param>
    public void UseItem(ItemName name) => UseItem(name, Source, Target);

    /// <summary>
    /// Use an item with the given name on the given source and target units.
    /// </summary>
    /// <param name="name">The name of the item to use.</param>
    /// <param name="source">The unit that is using the item.</param>
    /// <param name="target">The unit that is the target of the item.</param>
    private static void UseItem(ItemName name, Unit source, Unit target)
    {
        IUseable itemScript = FindItemScript(name);


        // Create battle context and use method for the item
        var ctx = new BattleContext(source, target, InCombat: true);
        itemScript.Use(ctx);
    }

    /// <summary>
    /// Find the script for an item with the given name.
    /// </summary>
    /// <param name="name">The name of the item to find the script for.</param>
    /// <returns>The script for the item.</returns>
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

    /// <summary>
    /// Check if the party has a Phoenix Pinion and attempt to use it to revive the party.
    /// </summary>
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

    /// <summary>
    /// Recalculate the turn order queue when a unit is revived with a Phoenix Down.
    /// </summary>
    /// <param name="revivedUnit">The unit that was revived.</param>
    private void RecalculateQueue(Unit revivedUnit)
    {
        // Put revived unit to the end of the queue.
        _queue.Enqueue(revivedUnit);
    }
}