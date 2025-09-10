using System;
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

        public static void EnsureCorrectInputModule(EventSystem es, string context = null)
        {
            if (es == null)
            {
                Debug.LogWarning("[InputModuleBootstrap] EnsureCorrectInputModule called with null EventSystem");
                return;
            }

            var legacy = es.GetComponent<StandaloneInputModule>();
            Type isType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            Component existingIS = null;
            bool inputSystemAvailable = false;
            if (isType != null)
            {
                inputSystemAvailable = true;
                try
                {
                    existingIS = es.GetComponent(isType);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[InputModuleBootstrap] Failed to probe InputSystemUIInputModule: {ex}");
                }
            }

            bool hadLegacy = legacy != null;
            bool hadIS = existingIS != null;

            bool newOnly = false;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            newOnly = true;
#endif

            Debug.Log($"[InputModuleBootstrap] EnsureCorrectInputModule ({context}) - Detected: NewOnly={newOnly}, InputSystemAvailable={inputSystemAvailable}, LegacyPresent={hadLegacy}, ISPresent={hadIS}");

            if (newOnly)
            {
                if (!inputSystemAvailable)
                {
                    Debug.LogError("[InputModuleBootstrap] Active Input is New-only but Input System package type was not found. UI input may fail.");
                }
                else if (!hadIS)
                {
                    try
                    {
                        es.gameObject.AddComponent(isType);
                        Debug.Log("[InputModuleBootstrap] Added InputSystemUIInputModule");
                    }
                    catch (Exception addEx)
                    {
                        Debug.LogError($"[InputModuleBootstrap] Failed to add InputSystemUIInputModule: {addEx}");
                    }
                }

                if (legacy != null)
                {
                    UnityEngine.Object.Destroy(legacy);
                    Debug.Log("[InputModuleBootstrap] Removed StandaloneInputModule (legacy)");
                }
            }
            else
            {
                // Legacy or Both: prefer StandaloneInputModule to keep compatibility with existing UI
                if (inputSystemAvailable && existingIS != null)
                {
                    UnityEngine.Object.Destroy(existingIS);
                    Debug.Log("[InputModuleBootstrap] Removed InputSystemUIInputModule");
                }

                if (legacy == null)
                {
                    es.gameObject.AddComponent<StandaloneInputModule>();
                    Debug.Log("[InputModuleBootstrap] Added StandaloneInputModule (legacy)");
                }
            }
        }
    }
}
