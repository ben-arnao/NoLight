using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif
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
            Debug.Log("[MainMenuUI] EnsureMenuOnLoad invoked after scene load");
            var existing = UnityEngine.Object.FindObjectOfType<MainMenuUI>();
            if (existing == null)
            {
                Debug.Log("[MainMenuUI] No MainMenuUI found in scene. Creating one.");
                var go = new GameObject("MainMenuUI");
                existing = go.AddComponent<MainMenuUI>();
            }
            if (existing != null)
            {
                Debug.Log("[MainMenuUI] Calling ShowMain()");
                existing.ShowMain();
            }
        }

        public void ShowMain()
        {
            Debug.Log("[MainMenuUI] ShowMain");
            FileLogger.EnsureBaselineMarkers("MainMenuUI.ShowMain");
            EnsureBuilt();
            if (mainPanel) mainPanel.SetActive(true);
            if (rosterPanel) rosterPanel.SetActive(false);
            if (collectionPanel) collectionPanel.SetActive(false);
        }

        public void ShowRoster()
        {
            Debug.Log("[MainMenuUI] ShowRoster");
            EnsureBuilt();
            if (mainPanel) mainPanel.SetActive(false);
            if (rosterPanel) rosterPanel.SetActive(true);
            if (collectionPanel) collectionPanel.SetActive(false);
        }

        public void ShowCollection()
        {
            Debug.Log("[MainMenuUI] ShowCollection");
            EnsureBuilt();
            if (mainPanel) mainPanel.SetActive(false);
            if (rosterPanel) rosterPanel.SetActive(false);
            if (collectionPanel) collectionPanel.SetActive(true);
        }

        private void Start()
        {
            Debug.Log("[MainMenuUI] Start");
            // Ensure the main menu is visible when the scene starts.
            EnsureBuilt();
            ShowMain();
        }

        private void EnsureBuilt()
        {
            Debug.Log("[MainMenuUI] EnsureBuilt called");
            if (mainPanel != null) { Debug.Log("[MainMenuUI] UI already built"); return; }

            // Ensure EventSystem exists and let InputModuleBootstrap configure the correct input module
            var es = FindObjectOfType<EventSystem>();
            if (es == null)
            {
                Debug.Log("[MainMenuUI] Creating EventSystem");
                var esGO = new GameObject("EventSystem", typeof(EventSystem));
                DontDestroyOnLoad(esGO);
                es = esGO.GetComponent<EventSystem>();
            }
            else
            {
                Debug.Log("[MainMenuUI] EventSystem already present");
            }

            InputModuleBootstrap.EnsureCorrectInputModule(es, "MainMenuUI.EnsureBuilt");

            // Create Canvas
            Debug.Log("[MainMenuUI] Creating Canvas and panels");
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
                // Ensure the button uses Color Tint transition so hover/pressed colors work.
                btn.transition = Selectable.Transition.ColorTint;
                btn.targetGraphic = closeImg;
                btn.interactable = true;
                var btnColors = btn.colors;
                btnColors.normalColor = new Color(0, 0, 0, 0);
                // Make hover/highlighted state a semi-transparent grey so the Exit looks grey on hover.
                btnColors.highlightedColor = new Color(0.6f, 0.6f, 0.6f, 0.4f);
                btnColors.pressedColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
                btnColors.selectedColor = btnColors.normalColor;
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

                Debug.Log("[MainMenuUI] Created close button");
                closeButton = btn;
            }

            if (closeButton != null)
            {
                Debug.Log("[MainMenuUI] Wiring close button OnClick -> OnExitButtonClicked");
                // Ensure the button will use color tinting for hover/pressed visuals (covers inspector-assigned buttons).
                closeButton.transition = Selectable.Transition.ColorTint;
                var cbColors = closeButton.colors;
                cbColors.normalColor = new Color(0, 0, 0, 0);
                cbColors.highlightedColor = new Color(0.6f, 0.6f, 0.6f, 0.4f);
                cbColors.pressedColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
                cbColors.selectedColor = cbColors.normalColor;
                closeButton.colors = cbColors;

                closeButton.onClick.RemoveAllListeners();
                // Use a handler that shows visual feedback before quitting so the user sees the click.
                closeButton.onClick.AddListener(OnExitButtonClicked);
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
            Debug.Log("[MainMenuUI] Start Run clicked");
            // If a roster panel exists, navigate there; otherwise start a default run.
            if (rosterPanel != null)
            {
                Debug.Log("[MainMenuUI] Roster panel exists; navigating to roster");
                ShowRoster();
                return;
            }

            if (GameManager.Instance != null)
            {
                Debug.Log("[MainMenuUI] Starting default run with a single Warrior");
                var squad = new List<CharacterDefinitionSO> { ContentFactory.CreateWarriorDefinition() };
                GameManager.Instance.StartNewRun(squad);
            }
            else
            {
                Debug.LogWarning("[MainMenuUI] GameManager.Instance not found.");
            }
        }

        private void OnViewCollectionClicked()
        {
            Debug.Log("[MainMenuUI] View Collection clicked");
            if (collectionPanel != null)
            {
                Debug.Log("[MainMenuUI] Collection panel exists; navigating to collection");
                ShowCollection();
                return;
            }

            Debug.Log("[MainMenuUI] Collection view is not set up yet.");
        }

        private void OnExitButtonClicked()
        {
            Debug.Log("[MainMenuUI] Exit button clicked");
            // Ensure correct input module just before quitting (diagnostic safety) — avoid re-attaching legacy module under new input system
            var es = EventSystem.current;
            if (es != null)
            {
                // If the wrong type of input module is present, remove it and ensure the correct one is attached
                foreach (var m in es.GetComponents<BaseInputModule>())
                {
#if ENABLE_INPUT_SYSTEM
                    if (!(m is InputSystemUIInputModule)) Destroy(m);
#else
                    if (!(m is StandaloneInputModule)) Destroy(m);
#endif
                }
#if ENABLE_INPUT_SYSTEM
                if (es.GetComponent<InputSystemUIInputModule>() == null) es.gameObject.AddComponent<InputSystemUIInputModule>();
#else
                if (es.GetComponent<StandaloneInputModule>() == null) es.gameObject.AddComponent<StandaloneInputModule>();
#endif
            }
            if (closeButton != null)
            {
                StartCoroutine(ExitButtonFeedbackAndQuit(closeButton, 0.2f));
            }
            else
            {
                Debug.Log("[MainMenuUI] Close button missing - quitting directly");
                QuitGame();
            }
        }

        private System.Collections.IEnumerator ExitButtonFeedbackAndQuit(Button btn, float postDelay)
        {
            Debug.Log("[MainMenuUI] ExitButtonFeedbackAndQuit starting");
            var rt = btn.GetComponent<RectTransform>();
            var img = btn.GetComponent<Image>();
            Vector3 origScale = rt != null ? rt.localScale : Vector3.one;
            Color origColor = img != null ? img.color : Color.white;
            float duration = 0.12f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float f = Mathf.Sin((t / duration) * Mathf.PI);
                if (rt != null) rt.localScale = origScale * (1f + 0.08f * f);
                if (img != null) img.color = Color.Lerp(origColor, Color.white, f * 0.5f);
                yield return null;
            }
            if (rt != null) rt.localScale = origScale;
            if (img != null) img.color = origColor;

            Debug.Log("[MainMenuUI] Exit animation complete, waiting briefly before quit");
            yield return new WaitForSecondsRealtime(postDelay);

            Debug.Log("[MainMenuUI] Quitting application now");
            QuitGame();
        }

        // Public method so it can be wired in the Inspector or called from UI.
        public void QuitGame()
        {
            Debug.Log($"[MainMenuUI] QuitGame requested. product={Application.productName} version={Application.version} platform={Application.platform}");

#if UNITY_EDITOR
            // Stop Play mode when running inside the Editor.
            UnityEditor.EditorApplication.isPlaying = false;
            Debug.Log("[MainMenuUI] Unity Editor: exiting Play mode");
#else
            // Request application quit for builds. Some platforms or build configurations
            // may not immediately terminate the process via Application.Quit(),
            // so follow up with Environment.Exit as a fallback to ensure the app exits.
            Application.Quit();
            Debug.Log("[MainMenuUI] Application.Quit() called; calling Environment.Exit(0) as fallback");
            System.Environment.Exit(0);
#endif
        }
    }
}
