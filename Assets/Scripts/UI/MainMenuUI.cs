using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using RogueLike2D.Core;
using RogueLike2D.Content;
using RogueLike2D.ScriptableObjects;

namespace RogueLike2D.UI
{
    // Simple main menu controller to toggle between menu panels and build a basic UI at runtime if not present.
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject rosterPanel;
        [SerializeField] private GameObject collectionPanel;
        [SerializeField] private Button closeButton;

        private Text titleText;
        private Button startRunButton;
        private Button viewCollectionButton;
        private Text buildNumberText;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureMenuOnLoad()
        {
            var existing = UnityEngine.Object.FindObjectOfType<MainMenuUI>();
            if (existing == null)
            {
                var go = new GameObject("MainMenuUI");
                existing = go.AddComponent<MainMenuUI>();
            }
            if (existing != null)
            {
                existing.ShowMain();
            }
        }

        public void ShowMain()
        {
            EnsureBuilt();
            if (mainPanel) mainPanel.SetActive(true);
            if (rosterPanel) rosterPanel.SetActive(false);
            if (collectionPanel) collectionPanel.SetActive(false);
        }

        public void ShowRoster()
        {
            EnsureBuilt();
            if (mainPanel) mainPanel.SetActive(false);
            if (rosterPanel) rosterPanel.SetActive(true);
            if (collectionPanel) collectionPanel.SetActive(false);
        }

        public void ShowCollection()
        {
            EnsureBuilt();
            if (mainPanel) mainPanel.SetActive(false);
            if (rosterPanel) rosterPanel.SetActive(false);
            if (collectionPanel) collectionPanel.SetActive(true);
        }

        private void Start()
        {
            // Ensure the main menu is visible when the scene starts.
            EnsureBuilt();
            ShowMain();
        }

        private void EnsureBuilt()
        {
            if (mainPanel != null) return;

            // Ensure EventSystem exists
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(es);
            }

            // Create Canvas
            var canvasGO = new GameObject("MainMenuCanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 1000;
            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGO.AddComponent<GraphicRaycaster>();

            // Create full-screen black background panel
            mainPanel = new GameObject("MainPanel");
            mainPanel.transform.SetParent(canvasGO.transform, false);
            var panelRect = mainPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var bg = mainPanel.AddComponent<Image>();
            bg.color = Color.black;

            var font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Title "No Light"
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(mainPanel.transform, false);
            titleText = titleGO.AddComponent<Text>();
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.75f);
            titleRect.anchorMax = new Vector2(0.5f, 0.75f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(800, 160);
            titleText.text = "no light";
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            titleText.font = font;
            titleText.fontSize = 96;
            titleText.fontStyle = FontStyle.Bold;

            // "Start Run" link-style button
            startRunButton = CreateLinkButton(mainPanel.transform, "StartRun", "Start Run", new Vector2(0, -60));
            startRunButton.onClick.AddListener(OnStartRunClicked);

            // "View Collection" link-style button
            viewCollectionButton = CreateLinkButton(mainPanel.transform, "ViewCollection", "View Collection", new Vector2(0, -120));
            viewCollectionButton.onClick.AddListener(OnViewCollectionClicked);

            // Build number bottom-right
            var buildGO = new GameObject("BuildNumber");
            buildGO.transform.SetParent(mainPanel.transform, false);
            buildNumberText = buildGO.AddComponent<Text>();
            var buildRect = buildGO.GetComponent<RectTransform>();
            buildRect.anchorMin = new Vector2(1f, 0f);
            buildRect.anchorMax = new Vector2(1f, 0f);
            buildRect.pivot = new Vector2(1f, 0f);
            buildRect.anchoredPosition = new Vector2(-16, 12);
            buildRect.sizeDelta = new Vector2(400, 40);
            buildNumberText.text = $"Build {Application.version}";
            buildNumberText.alignment = TextAnchor.LowerRight;
            buildNumberText.color = Color.white;
            buildNumberText.font = font;
            buildNumberText.fontSize = 18;

            // Close (X) button - top-right. If a Button has been assigned in the Inspector, respect it;
            // otherwise create a small X button and wire it to QuitGame.
            if (closeButton == null)
            {
                var closeGO = new GameObject("CloseButton");
                closeGO.transform.SetParent(mainPanel.transform, false);
                var closeRect = closeGO.AddComponent<RectTransform>();
                closeRect.anchorMin = new Vector2(1f, 1f);
                closeRect.anchorMax = new Vector2(1f, 1f);
                closeRect.pivot = new Vector2(1f, 1f);
                closeRect.anchoredPosition = new Vector2(-16, -16);
                closeRect.sizeDelta = new Vector2(96, 48);

                var closeImg = closeGO.AddComponent<Image>();
                closeImg.color = new Color(0, 0, 0, 0);
                closeImg.raycastTarget = true;

                var btn = closeGO.AddComponent<Button>();
                btn.targetGraphic = closeImg;
                btn.interactable = true;
                var btnColors = btn.colors;
                btnColors.normalColor = new Color(0, 0, 0, 0);
                btnColors.highlightedColor = new Color(1f, 1f, 1f, 0.1f);
                btnColors.pressedColor = new Color(1f, 1f, 1f, 0.2f);
                btn.colors = btnColors;

                var closeTextGO = new GameObject("X");
                closeTextGO.transform.SetParent(closeGO.transform, false);
                var closeText = closeTextGO.AddComponent<Text>();
                var closeTextRect = closeTextGO.GetComponent<RectTransform>();
                closeTextRect.anchorMin = new Vector2(0, 0);
                closeTextRect.anchorMax = new Vector2(1, 1);
                closeTextRect.offsetMin = Vector2.zero;
                closeTextRect.offsetMax = Vector2.zero;
                closeText.text = "Exit";
                closeText.alignment = TextAnchor.MiddleCenter;
                closeText.color = Color.white;
                closeText.font = font;
                closeText.fontSize = 20;
                closeText.fontStyle = FontStyle.Bold;
                closeText.raycastTarget = false;

                closeButton = btn;
            }

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                // Add a small debug log and call QuitGame to ensure the click handler is robust.
                closeButton.onClick.AddListener(() => { Debug.Log("Close button clicked - quitting"); QuitGame(); });
                closeButton.interactable = true;
                var imgComp = closeButton.GetComponent<Image>();
                if (imgComp != null) imgComp.raycastTarget = true;
            }
        }

        private Button CreateLinkButton(Transform parent, string name, string label, Vector2 anchoredPos)
        {
            // Container
            var btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);
            var rect = btnGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(400, 48);

            // Invisible background for click area
            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);

            var button = btnGO.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(0, 0, 0, 0);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.1f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.2f);
            colors.selectedColor = colors.normalColor;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.1f);
            button.colors = colors;

            // Text
            var textGO = new GameObject("Label");
            textGO.transform.SetParent(btnGO.transform, false);
            var text = textGO.AddComponent<Text>();
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = 36;
            text.fontStyle = FontStyle.Bold;

            return button;
        }

        private void OnStartRunClicked()
        {
            // If a roster panel exists, navigate there; otherwise start a default run.
            if (rosterPanel != null)
            {
                ShowRoster();
                return;
            }

            if (GameManager.Instance != null)
            {
                var squad = new List<CharacterDefinitionSO> { ContentFactory.CreateWarriorDefinition() };
                GameManager.Instance.StartNewRun(squad);
            }
            else
            {
                Debug.LogWarning("GameManager.Instance not found.");
            }
        }

        private void OnViewCollectionClicked()
        {
            if (collectionPanel != null)
            {
                ShowCollection();
                return;
            }

            Debug.Log("Collection view is not set up yet.");
        }

        // Public method so it can be wired in the Inspector or called from UI.
        public void QuitGame()
        {
            Debug.Log("Quit requested from Main Menu.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
