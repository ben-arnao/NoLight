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
            var es = EventSystem.current;
            if (es == null)
            {
                var go = new GameObject("EventSystem", typeof(EventSystem));
                es = go.GetComponent<EventSystem>();
            }

            ApplyCorrectInputModule(es);
        }

        private static void ApplyCorrectInputModule(EventSystem es)
        {
            if (es == null) return;

            var legacy = es.GetComponent<StandaloneInputModule>();

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            // Input System (New) only: ensure InputSystemUIInputModule and remove legacy module.
            var inputSystemModule = es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (inputSystemModule == null)
            {
                inputSystemModule = es.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
            if (legacy != null)
            {
                Object.Destroy(legacy);
            }
#else
            // Legacy or Both: ensure legacy StandaloneInputModule and remove InputSystemUIInputModule if present.
#if ENABLE_INPUT_SYSTEM
            var inputSystemModule = es.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            if (inputSystemModule != null)
            {
                Object.Destroy(inputSystemModule);
            }
#endif
            if (legacy == null)
            {
                es.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
        }
    }
}
