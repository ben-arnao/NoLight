using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RogueLike2D.Editor
{
    // Warns (or optionally fails) before builds if Active Input Handling may not match your runtime UI bootstrap path.
    // - Default: logs warnings only.
    // - To fail the build on mismatch, add scripting define: ROGUELIKE2D_FAIL_ON_INPUT_MISMATCH
    //
    // Notes:
    // 0 = Input Manager (Old), 1 = Input System Package (New), 2 = Both
    // We also check for the presence of the Input System UI module type and the project's InputModuleBootstrap.
    public class InputHandlingGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var targetGroup = report.summary.platformGroup;

            int active = PlayerSettings.GetPropertyInt("activeInputHandler", targetGroup);
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup) ?? string.Empty;
            bool strict = defines.Split(';').Any(d => d.Trim() == "ROGUELIKE2D_FAIL_ON_INPUT_MISMATCH");

            var bootstrapType = Type.GetType("RogueLike2D.UI.InputModuleBootstrap, Assembly-CSharp");
            var inputSystemType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");

            bool hasBootstrap = bootstrapType != null;
            bool hasInputSystemUIModule = inputSystemType != null;

            if (active == 2)
            {
                Debug.Log($"[InputHandlingGuard] Active Input Handling = Both. Proceeding. (Defines: {defines})");
                return;
            }

            if (active == 1)
            {
                if (!hasInputSystemUIModule)
                {
                    WarnOrFail(strict, "Active Input Handling is 'Input System Package (New)', but the Input System package (Unity.InputSystem) was not found. Install/enable it or switch to 'Both'.");
                }
                else if (!hasBootstrap)
                {
                    WarnOrFail(strict, "Active Input Handling is 'Input System Package (New)', but InputModuleBootstrap was not found. Ensure your EventSystem uses InputSystemUIInputModule or include a runtime bootstrap that adds it.");
                }
                else
                {
                    Debug.Log("[InputHandlingGuard] New Input System only + runtime bootstrap detected. Proceeding.");
                }
                return;
            }

            if (active == 0)
            {
                if (hasBootstrap && hasInputSystemUIModule)
                {
                    Debug.LogWarning("[InputHandlingGuard] Active Input Handling is 'Input Manager (Old)' while Input System package and bootstrap are present. Consider switching to 'Both' for safer Editor/Player behavior.");
                }
                else
                {
                    Debug.Log("[InputHandlingGuard] Active Input Handling = Old. Proceeding.");
                }
                return;
            }
        }

        private static void WarnOrFail(bool strict, string message)
        {
            if (strict)
            {
                throw new BuildFailedException("[InputHandlingGuard] " + message);
            }
            Debug.LogWarning("[InputHandlingGuard] " + message + " (Define ROGUELIKE2D_FAIL_ON_INPUT_MISMATCH to fail the build.)");
        }
    }
}
