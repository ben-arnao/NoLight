using UnityEngine;
using RogueLike2D.Characters;

namespace RogueLike2D.Battle
{
    // Resolves actions/abilities. Placeholder implementations.
    public class ActionResolver
    {
        public void BasicAttack(CharacterRuntime source, CharacterRuntime target)
        {
            if (source == null || target == null || !target.IsAlive) return;
            int raw = Mathf.Max(1, source.Stats.Attack);
            target.TakeDamage(raw);
            // TODO: integrate ability modifiers, crits, status application, etc.
        }

        // TODO: ResolveAbility(AbilitySO ability, CharacterRuntime source, List<CharacterRuntime> targets)
        // TODO: UseConsumable(ConsumableSO item, CharacterRuntime source, List<CharacterRuntime> targets)
        // TODO: SwapItems(CharacterRuntime actor, int itemIndexA, int itemIndexB)
    }
}
