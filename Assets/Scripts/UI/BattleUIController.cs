using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueLike2D.Battle;
using RogueLike2D.Characters;
using RogueLike2D.ScriptableObjects;

namespace RogueLike2D.UI
{
    // Minimal battle UI scaffolding: displays simple turn order and ability placeholders.
    public class BattleUIController : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private Text turnOrderText;
        [SerializeField] private Text turnCounterText;
        [SerializeField] private Transform abilitiesPanel; // parent for ability/target buttons
        [SerializeField] private Button abilityButtonPrefab;

        private void Awake()
        {
            if (!battleManager) battleManager = FindObjectOfType<BattleManager>();
        }

        private void OnEnable()
        {
            if (battleManager != null)
            {
                battleManager.OnTurnOrderUpdated += UpdateTurnOrderUI;
                battleManager.OnPlayerActionPrompt += OnPlayerActionPrompt;
                battleManager.OnTargetPrompt += OnTargetPrompt;
                battleManager.OnTurnCounterUpdated += OnTurnCounterUpdated;
            }
        }

        private void OnDisable()
        {
            if (battleManager != null)
            {
                battleManager.OnTurnOrderUpdated -= UpdateTurnOrderUI;
                battleManager.OnPlayerActionPrompt -= OnPlayerActionPrompt;
                battleManager.OnTargetPrompt -= OnTargetPrompt;
                battleManager.OnTurnCounterUpdated -= OnTurnCounterUpdated;
            }
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

        private void OnTurnCounterUpdated(int turnNumber, CharacterRuntime current, List<CharacterRuntime> upcoming)
        {
            if (!turnCounterText) return;
            string currentName = current?.Definition?.DisplayName ?? "Unknown";
            string nextName = upcoming != null && upcoming.Count > 0
                ? (upcoming[0]?.Definition?.DisplayName ?? "Unknown")
                : "None";
            turnCounterText.text = $"Turn {turnNumber} | Current: {currentName} | Next: {nextName}";
        }

        private void OnPlayerActionPrompt(CharacterRuntime actor, List<AbilitySO> abilities, List<CharacterRuntime> enemyTargets, List<CharacterRuntime> allyTargets)
        {
            ClearButtons();

            foreach (var ability in abilities)
            {
                if (ability == null) continue;
                var btn = Instantiate(abilityButtonPrefab, abilitiesPanel);
                var label = btn.GetComponentInChildren<Text>();
                if (label) label.text = $"{ability.DisplayName}";
                btn.interactable = !actor.IsOnCooldown(ability.Id);
                var captured = ability;
                btn.onClick.AddListener(() =>
                {
                    battleManager.TryChooseAbility(captured);
                });
            }
        }

        private void OnTargetPrompt(AbilitySO ability, List<CharacterRuntime> targets)
        {
            ClearButtons();

            foreach (var t in targets)
            {
                if (t == null || !t.IsAlive) continue;
                var btn = Instantiate(abilityButtonPrefab, abilitiesPanel);
                var label = btn.GetComponentInChildren<Text>();
                if (label) label.text = $"{t.Definition.DisplayName} (HP {t.Stats.CurrentHP}/{t.Stats.MaxHP})";
                var captured = t;
                btn.onClick.AddListener(() =>
                {
                    battleManager.TryChooseTarget(captured);
                });
            }
        }

        private void ClearButtons()
        {
            for (int i = abilitiesPanel.childCount - 1; i >= 0; i--)
            {
                Destroy(abilitiesPanel.GetChild(i).gameObject);
            }
        }
    }
}
