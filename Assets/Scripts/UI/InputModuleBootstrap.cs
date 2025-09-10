using UnityEngine;
using UnityEngine.EventSystems;

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

            EnsureCorrectInputModule(es, "RuntimeInit.AfterSceneLoad");
        }

        public static void EnsureCorrectInputModule(EventSystem es, string context = null)
        {
            if (es == null)
            {
                Debug.LogWarning("[InputModuleBootstrap] EnsureCorrectInputModule called with null EventSystem");
                return;
            }

            bool hadLegacy = es.GetComponent<StandaloneInputModule>() != null;
            bool hadIS = false;
#if ENABLE_INPUT_SYSTEM
            var existingIS = es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            hadIS = existingIS != null;
#endif

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            // Input System (New) only
            Debug.Log($"[InputModuleBootstrap] EnsureCorrectInputModule ({context}) - Active Input: New only. LegacyPresent={hadLegacy}, ISPresent={hadIS}");
#if ENABLE_INPUT_SYSTEM
            if (!hadIS)
            {
                es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                Debug.Log("[InputModuleBootstrap] Added InputSystemUIInputModule");
            }
#endif
            var legacy = es.GetComponent<StandaloneInputModule>();
            if (legacy != null)
            {
                Object.Destroy(legacy);
                Debug.Log("[InputModuleBootstrap] Removed StandaloneInputModule (legacy)");
            }
#else
            // Legacy or Both
            Debug.Log($"[InputModuleBootstrap] EnsureCorrectInputModule ({context}) - Active Input: Legacy or Both. LegacyPresent={hadLegacy}, ISPresent={hadIS}");
#if ENABLE_INPUT_SYSTEM
            if (hadIS)
            {
                Object.Destroy(existingIS);
                Debug.Log("[InputModuleBootstrap] Removed InputSystemUIInputModule");
            }
#endif
            var legacy = es.GetComponent<StandaloneInputModule>();
            if (legacy == null)
            {
                es.gameObject.AddComponent<StandaloneInputModule>();
                Debug.Log("[InputModuleBootstrap] Added StandaloneInputModule (legacy)");
            }
#endif
        }
    }
}
