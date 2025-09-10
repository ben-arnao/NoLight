using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RogueLike2D.Editor
{
    // Ensures legacy Input Manager is available alongside the new Input System
    // so scenes using StandaloneInputModule keep working.
    // 
    // Note: Direct references to PlayerSettings.ActiveInputHandling / PlayerSettings.activeInputHandling
    // cause compile errors on Unity versions where that API is not available. This implementation
    // uses reflection so the editor script compiles across Unity versions and only attempts to set
    // the value when the property and enum are present.
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
                var psType = typeof(PlayerSettings);
                // look for property "activeInputHandling"
                var prop = psType.GetProperty("activeInputHandling", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop == null)
                {
                    Debug.LogWarning("[EnsureInputSettings] PlayerSettings.activeInputHandling property not found in this Unity version. Skipping enforcement.");
                    return;
                }

                // Try to get the enum type PlayerSettings.ActiveInputHandling
                var enumType = psType.Assembly.GetType("UnityEditor.PlayerSettings+ActiveInputHandling")
                               ?? psType.GetNestedType("ActiveInputHandling", BindingFlags.Public | BindingFlags.NonPublic);

                if (enumType == null || !enumType.IsEnum)
                {
                    Debug.LogWarning("[EnsureInputSettings] ActiveInputHandling enum not found. Skipping enforcement.");
                    return;
                }

                // Get the enum value "Both"
                object desired = null;
                try
                {
                    desired = Enum.Parse(enumType, "Both");
                }
                catch
                {
                    // Fallback: try numeric value 2 which corresponds to Both in many Unity versions
                    try { desired = Enum.ToObject(enumType, 2); } catch { desired = null; }
                }

                if (desired == null)
                {
                    Debug.LogWarning("[EnsureInputSettings] Could not determine 'Both' enum value for ActiveInputHandling. Skipping enforcement.");
                    return;
                }

                var current = prop.GetValue(null, null);
                if (current == null || !current.Equals(desired))
                {
                    prop.SetValue(null, desired, null);
                    Debug.Log("[EnsureInputSettings] Set PlayerSettings.activeInputHandling=Both to prevent UI input exceptions.");
                }
                else if (logEvenIfAlreadySet)
                {
                    Debug.Log("[EnsureInputSettings] PlayerSettings.activeInputHandling already set to Both.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[EnsureInputSettings] Could not enforce Input Handling setting: {ex}");
            }
        }
    }
}
