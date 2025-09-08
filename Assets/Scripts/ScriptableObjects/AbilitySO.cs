using System.Collections.Generic;
using UnityEngine;
using RogueLike2D.Battle;

namespace RogueLike2D.ScriptableObjects
{
    public enum AbilityTargeting
    {
        Self,
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        RandomEnemy
    }

    [CreateAssetMenu(menuName = "Roguelike/Ability", fileName = "NewAbility")]
    public class AbilitySO : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;

        [Header("Combat")]
        public AbilityTargeting Targeting = AbilityTargeting.SingleEnemy;
        public int BasePower = 10;
        public int CooldownTurns = 2;

        [Header("Status Effects (placeholders)")]
        public List<StatusType> AppliesStatuses = new List<StatusType>();
        public int StatusDuration = 2;

        // TODO: Add VFX/SFX references if needed.

        // Placeholder execute; in production this would be data-driven with effect graphs or similar.
        public virtual void ExecutePlaceholder()
        {
            // Intentional no-op in skeleton.
        }
    }
}
