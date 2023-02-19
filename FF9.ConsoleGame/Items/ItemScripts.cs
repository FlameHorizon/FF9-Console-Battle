using FF9.ConsoleGame.Battle;

namespace FF9.ConsoleGame.Items;

using System.Reflection;

// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local

public static class ItemScripts
{
    public class HiPotion : IUseable
    {
        public void Use(BattleContext ctx)
        {
            if (ctx.Target.IsDead)
                return;

            int healAmount = ctx.InCombat ? 450 : 300;

            if (ctx.Source.HasSupportAbility(SupportAbility.Chemist))
                healAmount *= 2;

            ctx.Target.TakeHeal(healAmount);
        }
    }

    public class Potion : IUseable
    {
        public void Use(BattleContext ctx)
        {
            if (ctx.Target.IsDead)
                return;

            int healAmount = ctx.Source.HasSupportAbility(SupportAbility.Chemist) 
                ? 150 
                : 100;
            
            ctx.Target.TakeHeal(healAmount);
        }
    }

    public class PhoenixDown : IUseable
    {
        public void Use(BattleContext ctx)
        {
            if (ctx.Target.Hp != 0)
                return;

            ctx.Target.Revive();
        }
    }

    public class PhoenixPinion : IUseable
    {
        private readonly IRandomProvider _randomProvider;

        public PhoenixPinion()
        {
            _randomProvider = new RandomProvider();
        }
        
        public PhoenixPinion(IRandomProvider randomProvider)
        {
            _randomProvider = randomProvider;
        }
        
        public void Use(BattleContext ctx)
        {
            if (ctx.Target.IsPlayer)
            {
                ctx.Target.Revive();
                return;
            }

            if (ctx.Target.IsEnemy && ctx.Target.IsType(UnitType.Undead))
            {
                int roll = _randomProvider.Next(1, 11);
                if (roll == 10)
                    ctx.Target.InstantDeath();
                else
                    ctx.Target.TakeHeal(roll);
            }
        }
    }

    public class Elixir : IUseable
    {
        public void Use(BattleContext ctx)
        {
            if (ctx.Target.IsDead)
                return;

            if (ctx.Target.IsType(UnitType.Undead))
            {
                ctx.Target.InstantDeath();
            }
            else
            {
                ctx.Target.HealFull();
                ctx.Target.ManaFull();
            }
        }
    }

    /// <summary>
    /// Annoyntments are used to remove Trouble status.
    /// </summary>
    private class Annoyntment : IUseable
    {
        public void Use(BattleContext ctx)
        {
            if (ctx.Target.IsDead)
                return;

            _ = ctx.Target.RemoveStatus(Status.Trouble);
        }
    }

    /// <summary>
    /// Antidotes are used to remove Poison and Venom statuses.
    /// </summary>
    private class Antidote : IUseable
    {
        public void Use(BattleContext ctx)
        {
            if (ctx.Target.IsDead)
                return;

            _ = ctx.Target.RemoveStatus(Status.Poison);
            _ = ctx.Target.RemoveStatus(Status.Venom);
        }
    }

    /// <summary>
    /// Softs are used to remove Petrify and Gradual Petrify statuses,
    /// but their more useful function is to defeat stone-type enemies instantly.
    /// </summary>
    private class Soft : IUseable
    {
        public void Use(BattleContext ctx)
        {
            if (ctx.Target.IsDead)
                return;

            if (ctx.Target.IsPlayer)
            {
                _ = ctx.Target.RemoveStatus(Status.Petrify);
                _ = ctx.Target.RemoveStatus(Status.GradualPetrify);
            }
            else if (ctx.Target.IsType(UnitType.Stone))
            {
                ctx.Target.InstantDeath();
            }
        }
    }

    /// <summary>
    /// Remedy is a status healing item used to remove Stop, Poison, Venom,
    /// Mini, Gradual Petrify, Silence, and Darkness statuses.
    /// </summary>
    private class Remedy : IUseable
    {
        public void Use(BattleContext ctx)
        {
            if (ctx.Target.IsDead)
                return;

            var statuses = new[]
            {
                Status.Stop, Status.Poison, Status.Venom,
                Status.Mini, Status.GradualPetrify, Status.Silence,
                Status.Darkness
            };

            if (!ctx.Target.HasAnyStatus(statuses))
                return;

            foreach (Status s in statuses)
                _ = ctx.Target.RemoveStatus(s);
        }
    }

    /// <summary>
    /// Ether are used to restore MP.
    /// </summary>
    private class Ether : IUseable
    {
        public void Use(BattleContext ctx)
        {
            int amount = ctx.InCombat ? 100 : 150;
            if (ctx.Source.HasSupportAbility(SupportAbility.Chemist))
            {
                amount *= 2;
            }

            ctx.Target.RestoreMp(amount);
        }
    }

    /// <summary>
    /// Restore 50% of maximum HP and MP in field.
    /// Can be used in combat to fully heal, with a 50% chance to
    /// inflict Poison, Darkness, and Silence on
    /// either an enemy or a party member.
    /// </summary>
    private class Tent : IUseable
    {
        public void Use(BattleContext ctx)
        {
            double effectiveness = ctx.InCombat ? 1 : 0.5;

            ctx.Target.TakeHeal((int)(ctx.Target.MaxHp * effectiveness));
            ctx.Target.RestoreMp((int)(ctx.Target.MaxMp * effectiveness));

            if (ctx.InCombat == false)
                return;

            int roll = Random.Shared.Next(0, 2);
            if (roll == 0)
                return;

            ctx.Target.AddStatus(Status.Poison);
            ctx.Target.AddStatus(Status.Darkness);
            ctx.Target.AddStatus(Status.Silence);
        }
    }
}

public class BattleEngine
{
    private readonly List<Item> _inventory = new();
    public Unit Source { get; init; } = null!;
    public Unit Target { get; init; } = null!;

    public void GameOver()
    {
        int cnt = _inventory.Single(i => i.Name == ItemName.PhoenixPinion).Count;
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
                                  && t.Namespace == "ThinkingAboutItems.Scripts"
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
}

//public record Item(string Name, int Count);

public static class RandomExtensions
{
    public static decimal NextDecimal(this Random rng, decimal minValue, decimal maxValue)
    {
        decimal nextDecimalSample = NextDecimalSample(rng);
        return maxValue * nextDecimalSample + minValue * (1 - nextDecimalSample);
    }

    /// <summary>
    /// Provides a random decimal value in the range
    /// [0.0000000000000000000000000000, 0.9999999999999999999999999999)
    /// with (theoretical) uniform and discrete distribution
    /// </summary>
    /// <param name="random"></param>
    /// <returns></returns>
    public static decimal NextDecimalSample(this Random random)
    {
        var sample = 1m;
        //After ~200 million tries this never took more than one attempt but
        //it is possible to generate combinations of a, b, and c
        //with the approach below resulting in a sample >= 1.
        while (sample >= 1)
        {
            int a = random.Next();
            int b = random.Next();
            //The high bits of 0.9999999999999999999999999999m are 542101086.
            int c = random.Next(542101087);
            sample = new decimal(a, b, c, false, 28);
        }

        return sample;
    }
}