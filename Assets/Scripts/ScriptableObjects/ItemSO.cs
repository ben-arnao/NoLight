using UnityEngine;

namespace RogueLike2D.ScriptableObjects
{
    public abstract class ItemSO : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
    }

    [CreateAssetMenu(menuName = "Roguelike/Item/Permanent", fileName = "NewPermanentItem")]
    public class PermanentItemSO : ItemSO
    {
        [Header("Stat Modifiers (placeholders)")]
        public int BonusHP;
        public int BonusAttack;
        public int BonusDefense;
        public int BonusSpeed;

        [Header("Active Effect (optional)")]
        public bool HasActive;
        public int ActiveCooldown = 3;
        // TODO: Reference to an AbilitySO to reuse execution path.
    }

    [CreateAssetMenu(menuName = "Roguelike/Item/Consumable", fileName = "NewConsumable")]
    public class ConsumableSO : ItemSO
    {
        public int MaxStack = 9;
        public bool Targeted = true;
        // TODO: Effect definition.
    }
}
