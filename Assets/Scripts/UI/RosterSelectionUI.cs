using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RogueLike2D.Core;
using RogueLike2D.Content;
using RogueLike2D.ScriptableObjects;

namespace RogueLike2D.UI
{
    // Barebones roster selection: single Warrior with 4 abilities.
    public class RosterSelectionUI : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Button startRunButton;

        private void Awake()
        {
            // Find the first GameManager instance so we can start runs from the UI.
            if (!gameManager) gameManager = UnityEngine.Object.FindFirstObjectByType<GameManager>();
            if (startRunButton) startRunButton.onClick.AddListener(StartRunWithWarrior);
        }

        public void StartRunWithWarrior()
        {
            var warrior = ContentFactory.CreateWarriorDefinition();
            var squad = new List<CharacterDefinitionSO> { warrior };
            gameManager.StartNewRun(squad);
        }
    }
}
