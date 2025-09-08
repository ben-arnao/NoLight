using System.Collections.Generic;

namespace RogueLike2D.Stage
{
    public enum BattleType
    {
        None = 0,
        Normal = 1,
        MiniBoss = 2,
        FinalBoss = 3
    }

    // Data for a single encounter on a stage.
    [System.Serializable]
    public class EncounterData
    {
        public BattleType Type;
        public string Label;

        public EncounterData(BattleType type, string label)
        {
            Type = type;
            Label = label;
        }
    }

    // Data for one stage (a list of encounters).
    [System.Serializable]
    public class StageData
    {
        public int StageIndex;
        public List<EncounterData> Encounters = new List<EncounterData>();

        public StageData(int stageIndex)
        {
            StageIndex = stageIndex;
        }
    }
}
