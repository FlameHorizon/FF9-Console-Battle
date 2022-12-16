namespace FF9.Console.Battle;

public class Unit
{
    public string Name { get; init; }
    public int Health { get; private set; }
    public int Attack { get; init; }
    public int Defence { get; private set; }
    public int Agility { get; init; }
    public bool IsAlive => Health > 0;

    public Unit(string name, int health, int attack, int agility, int defence)
    {
        Name = name;
        Health = health;
        Attack = attack;
        Agility = agility;
        Defence = defence;
    }
    
    public int CalculateDamageTaken(int attack)
    {
        int damageTaken = attack - Defence;
        return damageTaken > 0 ? damageTaken : 0;
    }
    
    /// <summary>
    /// This method allows a unit to take damage and reduces the amount
    /// of damage taken based on the unit's defense.
    /// It also ensures that the unit's health cannot go below 0,
    /// which is typically the minimum value for health in a game.
    /// </summary>
    /// <param name="damage"></param>
    public virtual void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health < 0)
        {
            Health = 0;
        }
    }

    /// <summary>
    /// Allows a unit to increase its defense by
    /// 10% in preparation for an incoming attack.
    /// </summary>
    public void PerformDefence()
    {
        Defence = (int)(Defence * 1.1);
    }
}