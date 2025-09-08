using System.Collections.Generic;
using UnityEngine;
using RogueLike2D.Stage;
using RogueLike2D.ScriptableObjects;

namespace RogueLike2D.Core
{
    // Holds current run progression and player squad.
    [System.Serializable]
    public class RunData
    {
        public int SeasonId { get; private set; }
        public int Seed { get; private set; }

        public List<StageData> Stages { get; private set; }

        // 0..2 stages with mini-boss; final boss after stage 3
        public int StageIndex { get; private set; } = 0;
        // 0..4 encounters per stage (4 normal + 1 mini-boss)
        public int EncounterIndex { get; private set; } = 0;

        public List<CharacterDefinitionSO> Squad { get; private set; }

        public bool IsRunComplete { get; private set; } = false;

        public RunData(int seasonId, int seed, List<StageData> stages, List<CharacterDefinitionSO> squad)
        {
            SeasonId = seasonId;
            Seed = seed;
            Stages = stages;
            Squad = new List<CharacterDefinitionSO>(squad);
        }

        public BattleType GetNextEncounterType()
        {
            if (IsRunComplete) return BattleType.None;

            if (StageIndex < 3)
            {
                if (EncounterIndex < 4) return BattleType.Normal;
                if (EncounterIndex == 4) return BattleType.MiniBoss;
            }
            else if (StageIndex == 3)
            {
                // Final boss only
                if (EncounterIndex == 0) return BattleType.FinalBoss;
            }

            return BattleType.None;
        }

        public void AdvanceEncounter()
        {
            if (StageIndex < 3)
            {
                EncounterIndex++;
                if (EncounterIndex > 4)
                {
                    StageIndex++;
                    EncounterIndex = 0;
                }
            }
            else
            {
                // Final stage has only final boss
                EncounterIndex++;
                if (EncounterIndex > 0)
                {
                    IsRunComplete = true;
                }
            }
        }
    }
}
