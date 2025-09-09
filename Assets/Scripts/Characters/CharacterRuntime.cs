using System.Collections.Generic;
using UnityEngine;
using RogueLike2D.ScriptableObjects;

namespace RogueLike2D.Characters
{
    // Runtime instance of a character participating in battle.
    public class CharacterRuntime
    {
        public CharacterDefinitionSO Definition { get; private set; }
        public CharacterStats Stats { get; private set; }

        // 7 total abilities in definition; 4 equipped for this stage/run.
        public List<AbilitySO> EquippedAbilities { get; private set; } = new List<AbilitySO>(4);

        // Passive selection: 1 of 2
        public AbilitySO SelectedPassive { get; private set; }

        // Permanent items: 3 assigned, 2 active at a time (swap allowed)
        public List<PermanentItemSO> AssignedItems { get; private set; } = new List<PermanentItemSO>(3);
        public List<PermanentItemSO> ActiveItems { get; private set; } = new List<PermanentItemSO>(2);

        // Cooldowns for abilities/items (placeholder)
        private Dictionary<string, int> cooldowns = new Dictionary<string, int>();

        // Simple runtime combat flags/effects
        public int GuardAmount { get; private set; } = 0;              // flat reduction to incoming damage
        public int GuardTurnsRemaining { get; private set; } = 0;      // turns remaining for guard
        public bool BlockAllDamage { get; private set; } = false;      // blocks all incoming damage
        public int PendingHealNextTurn { get; private set; } = 0;      // heal at next BeginTurn
        public int NextAttackDamageModifier { get; private set; } = 0; // additive to next outgoing attack
        public int MarkStacks { get; set; } = 0;                        // simple mark for targeted synergies

        public bool IsAlive => Stats.CurrentHP > 0;

        public CharacterRuntime(CharacterDefinitionSO def)
        {
            Definition = def;
            Stats = def.BaseStats.Clone();

            // Default equipment setup
            for (int i = 0; i < def.Abilities.Count && EquippedAbilities.Count < 4; i++)
                EquippedAbilities.Add(def.Abilities[i]);

            if (def.Passives.Count > 0)
                SelectedPassive = def.Passives[0];

            for (int i = 0; i < def.PermanentItems.Count && AssignedItems.Count < 3; i++)
                AssignedItems.Add(def.PermanentItems[i]);

            // Activate up to 2 items
            for (int i = 0; i < AssignedItems.Count && ActiveItems.Count < 2; i++)
                ActiveItems.Add(AssignedItems[i]);

            // TODO: Apply passive and item modifiers to stats.
        }

        public void BeginTurn()
        {
            // Apply any pending start-of-turn effects
            if (PendingHealNextTurn > 0)
            {
                Heal(PendingHealNextTurn);
                PendingHealNextTurn = 0;
            }

            // Passive: regenerate at start of turn (e.g., Warrior passive)
            if (SelectedPassive != null && SelectedPassive.Id == "warrior_regen")
            {
                int regen = SelectedPassive.BasePower > 0 ? SelectedPassive.BasePower : 1;
                Heal(regen);
            }

            // Meditation-style full block ends at start of the user's next turn
            if (BlockAllDamage)
            {
                BlockAllDamage = false;
            }

            // Guard typically lasts through opponents' actions until end of our next turn;
            // we leave decrement to EndTurn to cover that window.
        }

        public void EndTurn()
        {
            // Reduce all cooldowns by 1 (min 0)
            var keys = new List<string>(cooldowns.Keys);
            foreach (var key in keys)
                cooldowns[key] = Mathf.Max(0, cooldowns[key] - 1);

            // Decay guard
            if (GuardTurnsRemaining > 0)
            {
                GuardTurnsRemaining--;
                if (GuardTurnsRemaining <= 0)
                {
                    GuardAmount = 0;
                }
            }

            // TODO: Tick status effects that act at turn end.
        }

        public void TakeDamage(int amount)
        {
            if (!IsAlive) return;

            if (BlockAllDamage)
                return;

            int dmg = Mathf.Max(0, amount - Stats.Defense);

            if (GuardAmount > 0 && GuardTurnsRemaining > 0)
                dmg = Mathf.Max(0, dmg - GuardAmount);

            Stats.CurrentHP = Mathf.Max(0, Stats.CurrentHP - dmg);
        }

        public void Heal(int amount)
        {
            Stats.CurrentHP = Mathf.Min(Stats.MaxHP, Stats.CurrentHP + Mathf.Max(0, amount));
        }

        public void ApplyCooldown(string id, int turns)
        {
            cooldowns[id] = Mathf.Max(cooldowns.TryGetValue(id, out int cur) ? cur : 0, turns);
        }

        public bool IsOnCooldown(string id)
        {
            return cooldowns.TryGetValue(id, out int turns) && turns > 0;
        }

        public void ApplyGuard(int amount, int turns)
        {
            GuardAmount = Mathf.Max(GuardAmount, amount);
            GuardTurnsRemaining = Mathf.Max(GuardTurnsRemaining, turns);
        }

        public void ApplyBlockAllUntilNextTurn(int healNextTurnAmount)
        {
            BlockAllDamage = true;
            PendingHealNextTurn = Mathf.Max(PendingHealNextTurn, healNextTurnAmount);
        }

        public void AddNextAttackModifier(int delta)
        {
            NextAttackDamageModifier += delta;
        }

        public int ConsumeNextAttackModifier()
        {
            int v = NextAttackDamageModifier;
            NextAttackDamageModifier = 0;
            return v;
        }

        public static List<CharacterRuntime> BuildRuntimeSquad(List<CharacterDefinitionSO> defs)
        {
            var list = new List<CharacterRuntime>();
            if (defs == null) return list;
            for (int i = 0; i < defs.Count && i < 4; i++)
            {
                if (defs[i] != null)
                    list.Add(new CharacterRuntime(defs[i]));
            }
            return list;
        }
    }
}
