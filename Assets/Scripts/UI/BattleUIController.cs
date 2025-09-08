using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueLike2D.Battle;
using RogueLike2D.Characters;

namespace RogueLike2D.UI
{
    // Minimal battle UI scaffolding: displays simple turn order and ability placeholders.
    public class BattleUIController : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private Text turnOrderText;
        [SerializeField] private Transform abilitiesPanel; // parent for ability buttons
        [SerializeField] private Button abilityButtonPrefab;

        private void Awake()
        {
            if (!battleManager) battleManager = FindObjectOfType<BattleManager>();
        }

        private void OnEnable()
        {
            if (battleManager != null)
                battleManager.OnTurnOrderUpdated += UpdateTurnOrderUI;
        }

        private void OnDisable()
        {
            if (battleManager != null)
                battleManager.OnTurnOrderUpdated -= UpdateTurnOrderUI;
        }

        private void UpdateTurnOrderUI(List<CharacterRuntime> order)
        {
            if (!turnOrderText) return;

            var names = new List<string>();
            foreach (var c in order)
            {
                string n = c?.Definition?.DisplayName ?? "Unknown";
                names.Add(n);
            }
            turnOrderText.text = "Turn Order: " + string.Join(" > ", names);
        }

        // TODO: Wire ability buttons to player input and call BattleManager actions.
    }
}
