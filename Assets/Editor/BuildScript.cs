using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace RogueLike2D.Editor
{
    // Simple Editor build script that builds all enabled scenes to a Windows standalone.
    // Call via: -executeMethod RogueLike2D.Editor.BuildScript.PerformWindowsBuild
    public static class BuildScript
    {
        // Compatibility alias for older CLI usage: -executeMethod BuildScript.PerformBuild
        public static void PerformBuild()
        {
            PerformWindowsBuild();
        }

        public static void PerformWindowsBuild()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes == null || scenes.Length == 0)
            {
                Debug.LogError("No enabled scenes found in Build Settings. Aborting build.");
                throw new BuildFailedException("No enabled scenes found in Build Settings.");
            }

            const string outputDir = "Builds/Windows";
            System.IO.Directory.CreateDirectory(outputDir);
            string exePath = System.IO.Path.Combine(outputDir, "RogueLike2D.exe");

            var buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log($"Starting build to {exePath} with {scenes.Length} scene(s).");
            var report = BuildPipeline.BuildPlayer(buildOptions);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {exePath} ({report.summary.totalSize} bytes)");
            }
            else
            {
                Debug.LogError($"Build failed: {report.summary.result}");
            }
        }
    }
}
