using System.Collections;
using FF9.ConsoleGame.Battle.Interfaces;
using FF9.ConsoleGame.Items;

namespace FF9.ConsoleGame.Battle;

public interface IUnit
{
    string Name { get; init; }
    int Hp { get; set; }
    int Mp { get; }
    int Agl { get; init; }
    int Damage { get; }
    int Defence { get; }
    bool IsAlive { get; }
    int Lv { get; }
    byte PhysicalHitRate { get; }
    bool IsPlayer { get; }
    int Spirit { get; }
    int[]? StealableItemsRates { get; init; }
    Item?[] StealableItems { get; init; }
    List<Item> Inventory { get; }
    bool InDefenceStance { get; }
    List<SupportAbility> SupportAbilities { get; set; }
    int StealableItemsCount { get; }
    int MaxHp { get; }
    bool IsDead { get; }
    bool IsEnemy { get; }
    int MaxMp { get; }
    int CalculatePhysicalDamage(int damage, byte attackerHitRate);

    /// <summary>
    /// This method allows a unit to take damage and reduces the amount
    /// of damage taken based on the unit's defense.
    /// It also ensures that the unit's hp cannot go below 0,
    /// which is typically the minimum value for hp in a game.
    /// </summary>
    /// <param name="damage"></param>
    void TakeDamage(int damage);

    /// <summary>
    /// Allows a unit to increase its defense by
    /// 10% in preparation for an incoming attack.
    /// </summary>
    void PerformDefence();

    void Equip(EquipmentItem equipmentItem);
    Item? Steal(int slot);
    void PutIntoInventory(Item item);
    void RemoveDefenceStance();
    void TakeHeal(int heal);
    bool HasSupportAbility(SupportAbility sa);
    void Revive();
    bool IsType(UnitType type);
    void InstantDeath();
    void HealFull();
    void ManaFull();
    bool HasStatus(Status status);
    bool RemoveStatus(Status status);
    void AddStatus(Status status);
    void TryRemoveStatus(Status status, out bool result);
    bool HasAnyStatus(IEnumerable<Status> statuses);
    void RestoreMp(int amount);
}

public class Unit : IUnit
{
    public string Name { get; init; }

    public int Hp { get; set; }
    public int Mp { get; private set; }

    // Base stats
    private int Str { get; init; }

    public int Agl { get; init; }
    // public int Int { get; private set; }
    // public int Sta { get; private set; }
    // public int Lck { get; private set; }

    // Derived stats
    public int Damage => Str / 2 + Weapon.Atk;
    public int Defence { get; private set; }

    // Hidden stats
    // Let's pretend this is warrior.
    private const byte InitialAccValueAtLv1 = 18;
    private static byte _acc;

    public bool IsAlive => Hp > 0;
    public int Lv { get; private set; } = 1;
    private WeaponItem Weapon { get; set; } = new(ItemName.Sword);

    private static readonly List<EquipmentItem> _equipment = new();
    public readonly IEnumerable<EquipmentItem> Equipment = _equipment;

    public byte PhysicalHitRate => (byte)(_acc + Weapon.HitRateBonus);
    public bool IsPlayer { get; private set; }
    public int Spirit { get; } = 0;
    public int[]? StealableItemsRates { get; init; }
    public Item?[] StealableItems { get; init; }
    public List<Item> Inventory { get; } = new();
    public bool InDefenceStance { get; private set; }
    public List<SupportAbility> SupportAbilities { get; set; } = new();

    private IPhysicalDamageCalculator _physicalDamageCalculator;
    private int _atk;
    private List<UnitType> _type = new();

    public Unit(string name, int hp, int maxHp, int mp, int str, int agl, int defence, int level, bool isPlayer,
        int spr,
        Item?[] stealableItems, int[]? rates)
    {
        Name = name;
        Hp = hp;
        MaxHp = maxHp;
        Mp = mp;
        Str = str;
        Agl = agl;
        Defence = defence;

        Lv = level <= 0
            ? throw new ArgumentOutOfRangeException(nameof(level), "Level has to be positive value")
            : level;

        IsPlayer = isPlayer;
        _acc = (byte)(InitialAccValueAtLv1 + 3 * (Lv - 1));
        Spirit = spr;
        if (rates != null) StealableItemsRates = rates;
        StealableItems = stealableItems;

        _physicalDamageCalculator = new PhysicalDamageCalculator(new RandomProvider());
    }

    public int StealableItemsCount => StealableItems.Count(i => i is not null);
    public int MaxHp { get; }
    public bool IsDead => IsAlive == false;
    public bool IsEnemy => IsPlayer == false;

    public int CalculatePhysicalDamage(int damage, byte attackerHitRate)
    {
        return _physicalDamageCalculator.Calculate(damage, attackerHitRate, this);
    }

    /// <summary>
    /// This method allows a unit to take damage and reduces the amount
    /// of damage taken based on the unit's defense.
    /// It also ensures that the unit's hp cannot go below 0,
    /// which is typically the minimum value for hp in a game.
    /// </summary>
    /// <param name="damage"></param>
    public virtual void TakeDamage(int damage)
    {
        Hp -= damage;
        if (Hp < 0)
        {
            Hp = 0;
        }
    }

    /// <summary>
    /// Allows a unit to increase its defense by
    /// 10% in preparation for an incoming attack.
    /// </summary>
    public void PerformDefence()
    {
        InDefenceStance = true;
    }

    public void Equip(EquipmentItem equipmentItem)
    {
        _equipment.Add(equipmentItem);
    }

    public Item? Steal(int slot)
    {
        if (slot is <= -1 or >= 4) 
            throw new ArgumentOutOfRangeException(nameof(slot), "Value must be in range 0 and 3");

        if (StealableItemsCount == 0)
            return null;

        if (StealableItems[slot] is null)
            return null;
        
        Item? item = StealableItems[slot];
        StealableItems[slot] = null;

        return item;
    }

    public void PutIntoInventory(Item item) => Inventory.Add(item);

    public void RemoveDefenceStance() => InDefenceStance = false;

    public void TakeHeal(int heal)
    {
        Hp += heal;
        if (Hp > MaxHp)
        {
            Hp = MaxHp;
        }
    }

    public bool HasSupportAbility(SupportAbility sa) => 
        SupportAbilities.Contains(sa);
    
    public void Revive() => Hp = (int)(MaxHp * 0.1);
    
    public bool IsType(UnitType type) => _type.Any(t => t == type);
    
    public void InstantDeath() => Hp = 0;

    public void HealFull()
    {
        Hp = MaxHp;
        Console.WriteLine($"Player {1} healed for {MaxHp}");
    }

    public void ManaFull()
    {
        Mp = MaxMp;
        Console.WriteLine($"Player {1} Mp renewed for {MaxMp}");
    }

    public int MaxMp { get; }
    private HashSet<Status> _statuses = new();

    public bool HasStatus(Status status) => _statuses.Contains(status);

    public bool RemoveStatus(Status status) => _statuses.Remove(status);

    public void AddStatus(Status status) => _statuses.Add(status);

    public void TryRemoveStatus(Status status, out bool result)
    {
        result = RemoveStatus(status);
    }

    public bool HasAnyStatus(IEnumerable<Status> statuses)
    {
        return _statuses.Any(statuses.Contains);
    }

    public void RestoreMp(int amount)
    {
        Mp += amount;
        if (Mp > MaxMp) 
            Mp = MaxMp;

        Console.WriteLine($"{amount} mana restored.");
    }
}

public enum EquipmentType
{
    Weapon,
    Armor
}

public enum UnitType
{
    Human,
    Undead,
    Stone
}