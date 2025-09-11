using UnityEngine;
using UnityEngine.UI;
using RogueLike2D.Battle;
using RogueLike2D.Core;

namespace RogueLike2D.UI
{
    // Very simple win/lose screen toggler.
    public class BattleResultUI : MonoBehaviour
    {
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private Button backToMenuButton;

        private void Awake()
        {
            // Acquire the BattleManager responsible for raising battle events.
            if (!battleManager) battleManager = UnityEngine.Object.FindFirstObjectByType<BattleManager>();
            if (backToMenuButton) backToMenuButton.onClick.AddListener(BackToMenu);
        }

        private void OnEnable()
        {
            if (battleManager != null)
                battleManager.OnBattleFinished += OnBattleFinished;
        }

        private void OnDisable()
        {
            if (battleManager != null)
                battleManager.OnBattleFinished -= OnBattleFinished;
        }

        private void OnBattleFinished(bool playerWon)
        {
            if (winPanel) winPanel.SetActive(playerWon);
            if (losePanel) losePanel.SetActive(!playerWon);
        }

        private void BackToMenu()
        {
            // For prototype, just hide both panels; hook scene loading as needed.
            if (winPanel) winPanel.SetActive(false);
            if (losePanel) losePanel.SetActive(false);
        }
    }
}
