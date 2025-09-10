using UnityEditor;
using UnityEngine;

namespace RogueLike2D.Editor
{
    // Ensures legacy Input Manager is available alongside the new Input System
    // so scenes using StandaloneInputModule keep working.
    [InitializeOnLoad]
    public static class EnsureInputSettings
    {
        static EnsureInputSettings()
        {
            TrySetBoth();
        }

        [MenuItem("Tools/RogueLike2D/Fix Input Handling (Set to Both)")]
        public static void FixNow()
        {
            TrySetBoth(true);
        }

        private static void TrySetBoth(bool logEvenIfAlreadySet = false)
        {
            try
            {
                var desired = PlayerSettings.ActiveInputHandling.Both;
                if (PlayerSettings.activeInputHandling != desired)
                {
                    PlayerSettings.activeInputHandling = desired;
                    Debug.Log("[EnsureInputSettings] Set PlayerSettings.activeInputHandling=Both to prevent UI input exceptions.");
                }
                else if (logEvenIfAlreadySet)
                {
                    Debug.Log("[EnsureInputSettings] PlayerSettings.activeInputHandling already set to Both.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[EnsureInputSettings] Could not enforce Input Handling setting: {ex}");
            }
        }
    }
}
