using System;
using System.Collections.Generic;
using UnityEngine;
using RogueLike2D.Stage;
using RogueLike2D.ScriptableObjects;

namespace RogueLike2D.Systems
{
    // Generates procedural stage layouts and enemy squads based on seed.
    public class StageGenerator : MonoBehaviour
    {
        public List<StageData> GenerateSeasonStages(int seed)
        {
            // For this skeleton, generate 3 stages + 1 final stage placeholder.
            var stages = new List<StageData>();
            var rng = new System.Random(seed);

            for (int i = 0; i < 3; i++)
            {
                var stage = new StageData(i);
                // 4 normal battles + 1 mini-boss
                for (int e = 0; e < 4; e++)
                    stage.Encounters.Add(new EncounterData(BattleType.Normal, $"Stage {i + 1} - Battle {e + 1}"));

                stage.Encounters.Add(new EncounterData(BattleType.MiniBoss, $"Stage {i + 1} - Mini Boss"));
                stages.Add(stage);
            }

            // Final boss as stage index 3 (single encounter)
            var finalStage = new StageData(3);
            finalStage.Encounters.Add(new EncounterData(BattleType.FinalBoss, "Final Boss"));
            stages.Add(finalStage);

            return stages;
        }

        public List<CharacterDefinitionSO> GenerateEnemiesForEncounter(BattleType type, int seed, int stageIndex, int encounterIndex)
        {
            // Placeholder enemy generation: return 4 copies of a dummy CharacterDefinitionSO if found.
            // In production, you would pull from a database of enemy definitions filtered by stage and difficulty.
            var list = new List<CharacterDefinitionSO>();
            // TODO: Replace with real enemy SOs. For now return empty list; BattleManager will handle nulls.
            return list;
        }
    }
}
