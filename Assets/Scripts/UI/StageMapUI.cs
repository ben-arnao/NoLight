using UnityEngine;
using UnityEngine.UI;
using RogueLike2D.Core;

namespace RogueLike2D.UI
{
    // Minimal stage progression UI.
    public class StageMapUI : MonoBehaviour
    {
        [SerializeField] private Text stageText;
        [SerializeField] private Text encounterText;

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            var run = typeof(GameManager).GetField("currentRun", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(gm) as RunData;
            if (run == null) return;

            if (stageText) stageText.text = $"Stage: {run.StageIndex + 1}/4";
            if (encounterText) encounterText.text = $"Encounter: {run.EncounterIndex + 1}";
        }
    }
}
