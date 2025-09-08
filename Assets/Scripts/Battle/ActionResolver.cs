using System.Collections.Generic;
using UnityEngine;
using RogueLike2D.Characters;
using RogueLike2D.ScriptableObjects;

namespace RogueLike2D.Battle
{
    // Resolves actions/abilities. Placeholder implementations.
    public class ActionResolver
    {
        public void BasicAttack(CharacterRuntime source, CharacterRuntime target)
        {
            if (source == null || target == null || !target.IsAlive) return;
            int baseDmg = Mathf.Max(1, source.Stats.Attack);
            int mod = source.ConsumeNextAttackModifier();
            int raw = Mathf.Max(0, baseDmg + mod);
            DealDamage(source, target, raw);
            // TODO: integrate ability modifiers, crits, status application, etc.
        }

        public void ResolveAbility(AbilitySO ability, CharacterRuntime source, List<CharacterRuntime> allies, List<CharacterRuntime> enemies, CharacterRuntime chosenTarget)
        {
            if (ability == null || source == null || !source.IsAlive) return;

            switch (ability.Id)
            {
                case "warrior_whirlwind":
                    // AOE 1-2 to all enemies
                    foreach (var t in enemies)
                    {
                        if (t != null && t.IsAlive)
                        {
                            int dmg = Random.Range(1, 3); // 1-2 inclusive
                            DealDamage(source, t, dmg);
                        }
                    }
                    break;

                case "warrior_meditate":
                    // Block all incoming damage until next turn, heal 5 at next turn start
                    source.ApplyBlockAllUntilNextTurn(5);
                    break;

                case "warrior_mark":
                    if (chosenTarget != null && chosenTarget.IsAlive)
                    {
                        chosenTarget.MarkStacks += 1;
                    }
                    break;

                case "warrior_power_strike":
                    if (chosenTarget != null && chosenTarget.IsAlive)
                    {
                        int dmg;
                        if (chosenTarget.MarkStacks > 0)
                        {
                            chosenTarget.MarkStacks -= 1;
                            dmg = Random.Range(5, 11); // 5-10 inclusive
                        }
                        else
                        {
                            // No mark present: light hit
                            dmg = Random.Range(2, 4); // 2-3
                        }
                        DealDamage(source, chosenTarget, dmg);
                    }
                    break;

                default:
                    // Fallback: basic attack on chosen target if provided
                    if (chosenTarget != null)
                        BasicAttack(source, chosenTarget);
                    break;
            }

            // Apply cooldown
            if (!string.IsNullOrEmpty(ability.Id) && ability.CooldownTurns > 0)
            {
                source.ApplyCooldown(ability.Id, ability.CooldownTurns);
            }
        }

        public void DemonAct(CharacterRuntime demon, List<CharacterRuntime> demonTeam, List<CharacterRuntime> playerTeam)
        {
            if (demon == null || !demon.IsAlive) return;
            var alivePlayers = playerTeam.FindAll(p => p != null && p.IsAlive);
            if (alivePlayers.Count == 0) return;

            float roll = Random.value;
            if (roll < 0.10f)
            {
                // Suck: drain 5
                var target = alivePlayers[Random.Range(0, alivePlayers.Count)];
                int dmg = 5;
                int preHp = target.Stats.CurrentHP;
                DealDamage(demon, target, dmg);
                int dealt = preHp - target.Stats.CurrentHP;
                if (dealt > 0)
                {
                    demon.Heal(dealt);
                }
            }
            else if (roll < 0.60f)
            {
                // Guard 2 next turn and deal 2
                demon.ApplyGuard(2, 1);
                var target = alivePlayers[Random.Range(0, alivePlayers.Count)];
                DealDamage(demon, target, 2);
            }
            else
            {
                // Deal 2 and reduce that enemy's next attack by 1 (additive)
                var target = alivePlayers[Random.Range(0, alivePlayers.Count)];
                DealDamage(demon, target, 2);
                target.AddNextAttackModifier(-1);
            }
        }

        private void DealDamage(CharacterRuntime source, CharacterRuntime target, int amount)
        {
            if (source == null || target == null || !target.IsAlive) return;
            target.TakeDamage(Mathf.Max(0, amount));
        }

        // TODO: UseConsumable(ConsumableSO item, CharacterRuntime source, List<CharacterRuntime> targets)
        // TODO: SwapItems(CharacterRuntime actor, int itemIndexA, int itemIndexB)
    }
}
