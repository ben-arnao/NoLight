using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting;
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
    [Preserve]
    public static class InputModuleBootstrap
    {
        // === EARLIEST HOOKS ===
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void BootstrapSubsystemRegistration()
        {
            Debug.Log("[InputModuleBootstrap] SubsystemRegistration");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void HookSceneLoaded()
        {
            try
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                Debug.Log("[InputModuleBootstrap] Hooked sceneLoaded (BeforeSceneLoad).");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[InputModuleBootstrap] Failed to hook sceneLoaded: {ex}");
            }
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            try
            {
#if UNITY_2023_1_OR_NEWER
                var es = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
#else
                var es = UnityEngine.Object.FindObjectOfType<EventSystem>();
#endif
                if (!es)
                {
                    var go = new GameObject("EventSystem", typeof(EventSystem));
                    es = go.GetComponent<EventSystem>();
                    Debug.Log("[InputModuleBootstrap] Created EventSystem in OnSceneLoaded.");
                }

                EnsureCorrectInputModule(es, $"OnSceneLoaded:{scene.name}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[InputModuleBootstrap] OnSceneLoaded failed: {ex}");
            }
        }

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

        // Extra safety: ensure this runs even in CLI builds and different script reload orders.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoFixEventSystem()
        {
            try
            {
#if UNITY_2023_1_OR_NEWER
                var es = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
#else
                var es = UnityEngine.Object.FindObjectOfType<EventSystem>();
#endif
                if (!es)
                {
                    var go = new GameObject("EventSystem", typeof(EventSystem));
                    es = go.GetComponent<EventSystem>();
                    Debug.Log("[InputModuleBootstrap] AutoFixEventSystem created EventSystem");
                }

                EnsureCorrectInputModule(es, "AutoFixEventSystem");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[InputModuleBootstrap] AutoFixEventSystem failed: {ex}");
            }
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

            // Remove any existing UI input modules to avoid mixed modules causing issues.
            try
            {
                var existing = es.GetComponents<BaseInputModule>();
                if (existing != null && existing.Length > 0)
                {
                    foreach (var m in existing)
                    {
                        try
                        {
                            m.enabled = false;
                        }
                        catch { /* ignore */ }
#if UNITY_EDITOR
                        UnityEngine.Object.DestroyImmediate(m, true);
#else
                        UnityEngine.Object.DestroyImmediate(m);
#endif
                    }
                    Debug.Log($"[InputModuleBootstrap] Removed {existing.Length} existing BaseInputModule component(s) immediately");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[InputModuleBootstrap] Failed to enumerate/remove existing input modules: {ex}");
            }

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            // Project is configured for Input System (New) only: add the new input system module.
            var uim = es.GetComponent<InputSystemUIInputModule>();
            if (!uim)
            {
                uim = es.gameObject.AddComponent<InputSystemUIInputModule>();
                Debug.Log("[InputModuleBootstrap] Added InputSystemUIInputModule (New only)");
            }
            // If no actions asset is assigned, wire up the module with the built-in default
            // so basic UI input (mouse, keyboard, gamepad) will work out of the box.
            try
            {
                if (uim.actionsAsset == null)
                {
                    // Load the default UI actions provided by the Input System package.
                    uim.actionsAsset = InputSystemUIInputModule.LoadDefaultActions();
                    Debug.Log("[InputModuleBootstrap] Assigned default UI actions asset to InputSystemUIInputModule");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[InputModuleBootstrap] Could not assign default actions asset: {ex}");
            }
#else
            // Legacy or Both: ensure StandaloneInputModule is present for maximum compatibility.
            var legacy = es.GetComponent<StandaloneInputModule>();
            if (!legacy)
            {
                es.gameObject.AddComponent<StandaloneInputModule>();
                Debug.Log("[InputModuleBootstrap] Added StandaloneInputModule (Legacy or Both)");
            }
#endif
            // Log the resulting modules for diagnostics.
            try
            {
                var finalMods = es.GetComponents<BaseInputModule>();
                Debug.Log($"[InputModuleBootstrap] Final modules ({context ?? "n/a"}): {string.Join(", ", Array.ConvertAll(finalMods, m => m.GetType().Name))}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[InputModuleBootstrap] Failed to log final modules: {ex}");
            }
        }
    }
}
