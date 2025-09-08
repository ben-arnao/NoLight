using UnityEngine;

namespace RogueLike2D.UI
{
    // Simple main menu controller to toggle between menu panels.
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject rosterPanel;
        [SerializeField] private GameObject collectionPanel;

        public void ShowMain()
        {
            if (mainPanel) mainPanel.SetActive(true);
            if (rosterPanel) rosterPanel.SetActive(false);
            if (collectionPanel) collectionPanel.SetActive(false);
        }

        public void ShowRoster()
        {
            if (mainPanel) mainPanel.SetActive(false);
            if (rosterPanel) rosterPanel.SetActive(true);
            if (collectionPanel) collectionPanel.SetActive(false);
        }

        public void ShowCollection()
        {
            if (mainPanel) mainPanel.SetActive(false);
            if (rosterPanel) rosterPanel.SetActive(false);
            if (collectionPanel) collectionPanel.SetActive(true);
        }
    }
}
