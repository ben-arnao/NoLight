using UnityEngine;
using UnityEngine.UI;
using RogueLike2D.Core;

namespace RogueLike2D.UI
{
    // Minimal reward selection UI placeholder.
    public class RewardUI : MonoBehaviour
    {
        [SerializeField] private Button abilityRewardButton;
        [SerializeField] private Button itemRewardButton;
        [SerializeField] private Button consumableRewardButton;
        [SerializeField] private Button tokenRewardButton;

        private void Awake()
        {
            if (abilityRewardButton) abilityRewardButton.onClick.AddListener(ChooseReward);
            if (itemRewardButton) itemRewardButton.onClick.AddListener(ChooseReward);
            if (consumableRewardButton) consumableRewardButton.onClick.AddListener(ChooseReward);
            if (tokenRewardButton) tokenRewardButton.onClick.AddListener(ChooseReward);
        }

        private void ChooseReward()
        {
            // TODO: Apply actual chosen reward. For now, continue the run.
            GameManager.Instance.OnRewardChosen();
        }
    }
}
