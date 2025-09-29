using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Build automation for TowerFusion4
    /// </summary>
    public class BuildScript
    {
        [MenuItem("Tower Fusion/Build iOS")]
        public static void BuildIOS()
        {
            string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "iOS");
            
            // Ensure build directory exists
            if (!Directory.Exists(buildPath))
            {
                Directory.CreateDirectory(buildPath);
            }
            
            // Configure build settings
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetScenePaths();
            buildPlayerOptions.locationPathName = buildPath;
            buildPlayerOptions.target = BuildTarget.iOS;
            buildPlayerOptions.options = BuildOptions.None;
            
            // Set iOS specific settings
            PlayerSettings.iOS.targetOSVersionString = "11.0";
            PlayerSettings.iOS.buildNumber = System.DateTime.Now.ToString("yyyyMMdd");
            
            Debug.Log("Starting iOS build...");
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize} bytes");
                Debug.Log($"Build location: {buildPath}");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed!");
            }
        }
        
        [MenuItem("Tower Fusion/Build Android")]
        public static void BuildAndroid()
        {
            string buildPath = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Android", "TowerFusion4.apk");
            
            // Ensure build directory exists
            string buildDir = Path.GetDirectoryName(buildPath);
            if (!Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }
            
            // Configure build settings
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetScenePaths();
            buildPlayerOptions.locationPathName = buildPath;
            buildPlayerOptions.target = BuildTarget.Android;
            buildPlayerOptions.options = BuildOptions.None;
            
            // Set Android specific settings
            PlayerSettings.Android.bundleVersionCode = System.DateTime.Now.Day;
            
            Debug.Log("Starting Android build...");
            
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {summary.totalSize} bytes");
                Debug.Log($"Build location: {buildPath}");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed!");
            }
        }
        
        private static string[] GetScenePaths()
        {
            string[] scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path;
            }
            return scenes;
        }
    }
}