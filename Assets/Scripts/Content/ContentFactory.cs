using System.Collections.Generic;
using UnityEngine;
using RogueLike2D.Characters;
using RogueLike2D.ScriptableObjects;

namespace RogueLike2D.Content
{
    // Creates runtime ScriptableObjects for prototype content (warrior and demon).
    public static class ContentFactory
    {
        public static CharacterDefinitionSO CreateWarriorDefinition()
        {
            var def = ScriptableObject.CreateInstance<CharacterDefinitionSO>();
            def.DisplayName = "Warrior";
            def.BaseStats = new CharacterStats
            {
                MaxHP = 30,
                CurrentHP = 30,
                Attack = 3,
                Defense = 0,
                Speed = 5
            };

            def.Abilities = new List<AbilitySO>
            {
                CreateAbility("warrior_whirlwind", "Whirlwind", "AOE attack for 1-2 to all enemies.", AbilityTargeting.AllEnemies, basePower: 0, cooldown: 1),
                CreateAbility("warrior_meditate", "Meditate", "Block all damage and heal 5 next turn.", AbilityTargeting.Self, basePower: 0, cooldown: 1),
                CreateAbility("warrior_mark", "Marked Strike", "Mark an enemy.", AbilityTargeting.SingleEnemy, basePower: 0, cooldown: 1),
                CreateAbility("warrior_power_strike", "Power Strike", "Consume a Mark (if present) to deal 5-10 damage; otherwise light hit.", AbilityTargeting.SingleEnemy, basePower: 0, cooldown: 1),
            };

            def.Passives = new List<AbilitySO>();
            def.PermanentItems = new List<PermanentItemSO>();
            return def;
        }

        public static CharacterDefinitionSO CreateDemonDefinition()
        {
            var def = ScriptableObject.CreateInstance<CharacterDefinitionSO>();
            def.DisplayName = "Demon";
            def.BaseStats = new CharacterStats
            {
                MaxHP = 15,
                CurrentHP = 15,
                Attack = 2,
                Defense = 0,
                Speed = 2
            };
            def.Abilities = new List<AbilitySO>();
            def.Passives = new List<AbilitySO>();
            def.PermanentItems = new List<PermanentItemSO>();
            return def;
        }

        private static AbilitySO CreateAbility(string id, string name, string desc, AbilityTargeting targeting, int basePower, int cooldown)
        {
            var a = ScriptableObject.CreateInstance<AbilitySO>();
            a.Id = id;
            a.DisplayName = name;
            a.Description = desc;
            a.Targeting = targeting;
            a.BasePower = basePower;
            a.CooldownTurns = cooldown;
            return a;
        }
    }
}
