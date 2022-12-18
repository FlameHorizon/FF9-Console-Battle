using System.Collections;
using FF9.ConsoleGame.Battle.Interfaces;

namespace FF9.ConsoleGame.Battle;

public class Unit
{
    public string Name { get; init; }

    public int Hp { get; private set; }
    public int Mp { get; private set; }

    // Base stats
    private int Str { get; init; }

    public int Agl { get; init; }
    // public int Int { get; private set; }
    // public int Sta { get; private set; }
    // public int Lck { get; private set; }

    // Derived stats
    public int Damage => (Str / 2) + Weapon.Atk;
    public int Defence { get; private set; }

    // Hidden stats
    // Let's pretend this is warrior.
    private const byte InitialAccValueAtLv1 = 18;
    private static byte _acc;

    public bool IsAlive => Hp > 0;
    public int Lv { get; private set; } = 1;
    private WeaponItem Weapon { get; set; } = new();

    private static readonly List<EquipmentItem> _equipment = new();
    public readonly IEnumerable<EquipmentItem> Equipment = _equipment;

    public byte PhysicalHitRate => (byte)(_acc + Weapon.HitRateBonus);
    public bool IsPlayer { get; private set; }
    public int Spirit { get; } = 0;
    public int[]? StealableItemsRates { get; init; }
    public Item?[] StealableItems { get; private set; }
    public List<Item> Inventory { get; } = new();
    public bool InDefenceStance { get; private set; }

    private IPhysicalDamageCalculator _physicalDamageCalculator;
    private int _atk;

    public Unit(string name, int hp, int mp, int str, int agl, int defence, int level, bool isPlayer, int spr,
        Item?[] stealableItems, int[]? rates)
    {
        Name = name;
        Hp = hp;
        Mp = mp;
        Str = str;
        Agl = agl;
        Defence = defence;

        Lv = level <= 0
            ? throw new ArgumentOutOfRangeException(nameof(level), "Level has to be positive value")
            : level;


        IsPlayer = isPlayer;
        _acc = (byte)(InitialAccValueAtLv1 + (3 * (Lv - 1)));
        Spirit = spr;
        if (rates != null) StealableItemsRates = rates;
        StealableItems = stealableItems;

        _physicalDamageCalculator = new PhysicalDamageCalculator(new RandomProvider());
        
        
    }

    public int StealableItemsCount => StealableItems.Count(i => i is not null);
    
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

    public void PutIntoInventory(Item item)
    {
        Inventory.Add(item);
    }

    public void RemoveDefenceStance()
    {
        InDefenceStance = false;
    }
}

public enum EquipmentType
{
    Weapon,
    Armor
}