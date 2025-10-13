using UnityEngine;
using UnityEditor;
using System.IO;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Setup utility to organize traits for runtime loading
    /// </summary>
    public static class TraitResourcesSetup
    {
        [MenuItem("Tools/Tower Fusion/Setup Traits for Runtime Loading")]
        public static void SetupTraitsInResources()
        {
            string sourceFolder = "Assets/Data/Traits";
            string targetFolder = "Assets/Resources/Traits";
            
            // Create Resources/Traits folder if it doesn't exist
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                Debug.Log("Created Assets/Resources folder");
            }
            
            if (!AssetDatabase.IsValidFolder(targetFolder))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Traits");
                Debug.Log("Created Assets/Resources/Traits folder");
            }
            
            // Check if source folder exists
            if (!AssetDatabase.IsValidFolder(sourceFolder))
            {
                Debug.LogWarning($"Source folder {sourceFolder} does not exist. Creating default traits first...");
                TowerTraitFactory.CreateDefaultTraits();
            }
            
            // Find all trait assets in source folder
            string[] guids = AssetDatabase.FindAssets("t:TowerTrait", new[] { sourceFolder });
            
            if (guids.Length == 0)
            {
                Debug.LogWarning($"No traits found in {sourceFolder}. Run 'Create Default Traits' first.");
                return;
            }
            
            int movedCount = 0;
            
            foreach (string guid in guids)
            {
                string sourcePath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileName(sourcePath);
                string targetPath = Path.Combine(targetFolder, fileName);
                
                // Check if file already exists in target
                if (File.Exists(targetPath))
                {
                    Debug.Log($"Trait already exists in Resources: {fileName}");
                    continue;
                }
                
                // Move the asset to Resources folder
                string error = AssetDatabase.MoveAsset(sourcePath, targetPath);
                
                if (string.IsNullOrEmpty(error))
                {
                    Debug.Log($"✓ Moved: {fileName} → {targetPath}");
                    movedCount++;
                }
                else
                {
                    Debug.LogError($"✗ Failed to move {fileName}: {error}");
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"<color=green><b>Trait Setup Complete!</b></color>");
            Debug.Log($"Moved {movedCount} traits to Resources folder");
            Debug.Log($"Traits can now be loaded at runtime via Resources.LoadAll<TowerTrait>(\"Traits\")");
            Debug.Log($"\n<color=cyan>Next steps:</color>");
            Debug.Log("1. Your GameUI will now auto-load traits on Start");
            Debug.Log("2. Click the 'trait' button in-game to test");
            Debug.Log("3. A random trait should appear in the dialog");
        }
        
        [MenuItem("Tools/Tower Fusion/Debug: List All Traits")]
        public static void ListAllTraits()
        {
            Debug.Log("<b>=== Searching for Traits in Project ===</b>");
            
            // Search in Data/Traits
            string[] dataGuids = AssetDatabase.FindAssets("t:TowerTrait", new[] { "Assets/Data/Traits" });
            Debug.Log($"\n<color=yellow>Traits in Assets/Data/Traits: {dataGuids.Length}</color>");
            foreach (string guid in dataGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TowerTrait trait = AssetDatabase.LoadAssetAtPath<TowerTrait>(path);
                Debug.Log($"  • {trait.traitName} ({path})");
            }
            
            // Search in Resources/Traits
            string[] resourceGuids = AssetDatabase.FindAssets("t:TowerTrait", new[] { "Assets/Resources/Traits" });
            Debug.Log($"\n<color=green>Traits in Assets/Resources/Traits: {resourceGuids.Length}</color>");
            foreach (string guid in resourceGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                TowerTrait trait = AssetDatabase.LoadAssetAtPath<TowerTrait>(path);
                Debug.Log($"  • {trait.traitName} ({path})");
            }
            
            // Test runtime loading
            Debug.Log("\n<color=cyan>=== Testing Runtime Loading ===</color>");
            TowerTrait[] runtimeTraits = Resources.LoadAll<TowerTrait>("Traits");
            Debug.Log($"Resources.LoadAll<TowerTrait>(\"Traits\") returned: {runtimeTraits.Length} traits");
            foreach (var trait in runtimeTraits)
            {
                Debug.Log($"  ✓ {trait.traitName}");
            }
            
            if (runtimeTraits.Length == 0)
            {
                Debug.LogWarning("<color=red>No traits loaded at runtime!</color>");
                Debug.LogWarning("Run: Tools > Tower Fusion > Setup Traits for Runtime Loading");
            }
        }
        
        [MenuItem("Tools/Tower Fusion/Fix: Create and Setup All Traits")]
        public static void CreateAndSetupAllTraits()
        {
            Debug.Log("<b>=== Complete Trait Setup ===</b>");
            
            // Step 1: Create default traits
            Debug.Log("\n<color=yellow>Step 1: Creating default traits...</color>");
            TowerTraitFactory.CreateDefaultTraits();
            
            // Step 2: Move to Resources
            Debug.Log("\n<color=yellow>Step 2: Moving traits to Resources...</color>");
            SetupTraitsInResources();
            
            // Step 3: Verify
            Debug.Log("\n<color=yellow>Step 3: Verifying setup...</color>");
            ListAllTraits();
            
            Debug.Log("\n<color=green><b>✓ Complete! Your traits are ready to use in-game.</b></color>");
        }
    }
}
