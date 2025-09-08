using System.Collections.Generic;
using UnityEngine;
using RogueLike2D.Characters;

namespace RogueLike2D.ScriptableObjects
{
    [CreateAssetMenu(menuName = "Roguelike/Character", fileName = "NewCharacter")]
    public class CharacterDefinitionSO : ScriptableObject
    {
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;

        [Header("Base Stats")]
        public CharacterStats BaseStats = new CharacterStats();

        [Header("Abilities")]
        // 7 total abilities to choose from (player equips 4 per stage)
        public List<AbilitySO> Abilities = new List<AbilitySO>(7);

        [Header("Passives")]
        public List<AbilitySO> Passives = new List<AbilitySO>(2);

        [Header("Permanent Items")]
        public List<PermanentItemSO> PermanentItems = new List<PermanentItemSO>(3);
    }
}
