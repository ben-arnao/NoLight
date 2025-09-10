using System;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
// using UnityEngine.InputSystem; // Uncomment if assigning actions asset in code
#endif

namespace RogueLike2D.UI
{
    // Ensures the correct UI input module is present at runtime to prevent freezes when
    // Active Input Handling is set to "Input System Package (New)".
    // - If only the new Input System is enabled, uses InputSystemUIInputModule.
    // - Otherwise (Legacy or Both), uses StandaloneInputModule.
    public static class InputModuleBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureEventSystemAndCorrectInputModule()
        {
            Debug.Log("[InputModuleBootstrap] AfterSceneLoad init - ensuring EventSystem and correct input module");
            var es = EventSystem.current;
            if (es == null)
            {
                var go = new GameObject("EventSystem", typeof(EventSystem));
                es = go.GetComponent<EventSystem>();
                Debug.Log("[InputModuleBootstrap] Created new EventSystem (no input module attached yet)");
            }
            else
            {
                Debug.Log("[InputModuleBootstrap] Found existing EventSystem at init");
            }

            try
            {
                var legacy = es != null ? es.GetComponent<StandaloneInputModule>() : null;
                var isType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                Component isModule = (isType != null && es != null) ? es.GetComponent(isType) : null;
                Debug.Log($"[InputModuleBootstrap] Pre-check modules at init: LegacyPresent={(legacy!=null)}, ISPresent={(isModule!=null)}, InputSystemTypeFound={(isType!=null)}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[InputModuleBootstrap] Pre-check failed: {ex}");
            }

            EnsureCorrectInputModule(es, "RuntimeInit.AfterSceneLoad");
        }

        // Public entry point to ensure an EventSystem exists and is configured
        // with the correct input module based on the active input handling.
        public static EventSystem EnsureEventSystem()
        {
            var es = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (!es)
            {
                var go = new GameObject("EventSystem", typeof(EventSystem));
                es = go.GetComponent<EventSystem>();
                Debug.Log("[InputModuleBootstrap] Created EventSystem");
            }

            EnsureCorrectInputModule(es, "EnsureEventSystem");
            return es;
        }

        public static void EnsureCorrectInputModule(EventSystem es, string context = null)
        {
            if (es == null)
            {
                Debug.LogWarning("[InputModuleBootstrap] EnsureCorrectInputModule called with null EventSystem");
                return;
            }

#if ENABLE_INPUT_SYSTEM
            // Prefer the new Input System UI module whenever the define is present (New or Both).
            var legacy = es.GetComponent<StandaloneInputModule>();
            if (legacy)
            {
                UnityEngine.Object.Destroy(legacy);
                Debug.Log("[InputModuleBootstrap] Removed StandaloneInputModule (legacy)");
            }

            var uim = es.GetComponent<InputSystemUIInputModule>();
            if (!uim)
            {
                uim = es.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log("[InputModuleBootstrap] Added InputSystemUIInputModule");
            }

            // OPTIONAL: assign a UI actions asset if needed for keyboard/controller navigation.
            // uim.actionsAsset = uim.actionsAsset ?? Resources.Load<InputActionAsset>(\"UIActions\");
#else
            // Legacy path: ensure StandaloneInputModule is present.
            var legacy = es.GetComponent<StandaloneInputModule>();
            if (!legacy)
            {
                es.gameObject.AddComponent<StandaloneInputModule>();
                Debug.Log("[InputModuleBootstrap] Added StandaloneInputModule (legacy)");
            }
#endif
        }
    }
}
