using UnityEngine;
using UnityEditor;
using TowerFusion;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Editor utility to create pre-defined tower traits
    /// </summary>
    public static class TowerTraitFactory
    {
        [MenuItem("Tools/Tower Fusion/Create Default Traits")]
        public static void CreateDefaultTraits()
        {
            string traitsPath = "Assets/Data/Traits";
            
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder(traitsPath))
            {
                AssetDatabase.CreateFolder("Assets/Data", "Traits");
            }
            
            // Check which traits already exist
            bool fireExists = AssetDatabase.LoadAssetAtPath<TowerTrait>($"{traitsPath}/Fire.asset") != null;
            bool iceExists = AssetDatabase.LoadAssetAtPath<TowerTrait>($"{traitsPath}/Ice.asset") != null;
            bool lightningExists = AssetDatabase.LoadAssetAtPath<TowerTrait>($"{traitsPath}/Lightning.asset") != null;
            bool sniperExists = AssetDatabase.LoadAssetAtPath<TowerTrait>($"{traitsPath}/Sniper.asset") != null;
            bool harvestExists = AssetDatabase.LoadAssetAtPath<TowerTrait>($"{traitsPath}/Harvest.asset") != null;
            bool explosionExists = AssetDatabase.LoadAssetAtPath<TowerTrait>($"{traitsPath}/Explosion.asset") != null;
            bool earthExists = AssetDatabase.LoadAssetAtPath<TowerTrait>($"{traitsPath}/Earth.asset") != null;
            
            // Only create traits that don't exist
            if (!fireExists) CreateFireTrait(traitsPath);
            if (!iceExists) CreateIceTrait(traitsPath);
            if (!lightningExists) CreateLightningTrait(traitsPath);
            if (!sniperExists) CreateSniperTrait(traitsPath);
            if (!harvestExists) CreateHarvestTrait(traitsPath);
            if (!explosionExists) CreateExplosionTrait(traitsPath);
            if (!earthExists) CreateEarthTrait(traitsPath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            int created = 0;
            if (!fireExists) created++;
            if (!iceExists) created++;
            if (!lightningExists) created++;
            if (!sniperExists) created++;
            if (!harvestExists) created++;
            if (!explosionExists) created++;
            if (!earthExists) created++;
            
            if (created > 0)
            {
                Debug.Log($"Created {created} new tower traits in {traitsPath}");
            }
            else
            {
                Debug.Log("All traits already exist. No new traits created.");
            }
        }
        
        [MenuItem("Tools/Tower Fusion/Update Earth Trait Only")]
        public static void UpdateEarthTraitOnly()
        {
            // Check Resources folder first (where traits should be for runtime loading)
            string resourcesPath = "Assets/Resources/Traits";
            string dataPath = "Assets/Data/Traits";
            
            string earthPathInResources = $"{resourcesPath}/Earth.asset";
            string earthPathInData = $"{dataPath}/Earth.asset";
            
            TowerTrait existingEarth = null;
            string foundPath = "";
            
            // Try to find Earth trait in Resources folder first
            existingEarth = AssetDatabase.LoadAssetAtPath<TowerTrait>(earthPathInResources);
            if (existingEarth != null)
            {
                foundPath = earthPathInResources;
                Debug.Log($"Found Earth trait in Resources folder: {foundPath}");
            }
            else
            {
                // Try Data folder as fallback
                existingEarth = AssetDatabase.LoadAssetAtPath<TowerTrait>(earthPathInData);
                if (existingEarth != null)
                {
                    foundPath = earthPathInData;
                    Debug.Log($"Found Earth trait in Data folder: {foundPath}");
                    Debug.LogWarning("Earth trait is in Data folder - it should be in Resources for runtime loading!");
                }
            }
            
            if (existingEarth != null)
            {
                // Update existing Earth trait
                existingEarth.description = "Hit enemy becomes black disk trap (3s) - other enemies fall in and die";
                existingEarth.trapDuration = 3f;
                existingEarth.trapRadius = 1f;
                existingEarth.overlayColor = new Color(0.6f, 0.4f, 0.2f);
                existingEarth.overlayAlpha = 0.4f;
                
                EditorUtility.SetDirty(existingEarth);
                AssetDatabase.SaveAssets();
                
                Debug.Log($"<color=green>✓ Updated Earth trait at {foundPath}</color>");
                Debug.Log("Changes: First hit converts enemy to black disk trap, other enemies fall in and die");
                
                // If trait was in Data folder, move it to Resources
                if (foundPath == earthPathInData)
                {
                    Debug.Log("Moving Earth trait to Resources folder...");
                    
                    // Ensure Resources/Traits folder exists
                    if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                    {
                        AssetDatabase.CreateFolder("Assets", "Resources");
                    }
                    if (!AssetDatabase.IsValidFolder(resourcesPath))
                    {
                        AssetDatabase.CreateFolder("Assets/Resources", "Traits");
                    }
                    
                    string moveError = AssetDatabase.MoveAsset(earthPathInData, earthPathInResources);
                    if (string.IsNullOrEmpty(moveError))
                    {
                        Debug.Log($"<color=green>✓ Moved Earth trait to {earthPathInResources}</color>");
                    }
                    else
                    {
                        Debug.LogError($"Failed to move Earth trait: {moveError}");
                    }
                }
            }
            else
            {
                // Create new Earth trait directly in Resources folder
                Debug.Log("Earth trait not found. Creating new one in Resources folder...");
                
                // Ensure Resources/Traits folder exists
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                if (!AssetDatabase.IsValidFolder(resourcesPath))
                {
                    AssetDatabase.CreateFolder("Assets/Resources", "Traits");
                }
                
                CreateEarthTrait(resourcesPath);
                AssetDatabase.SaveAssets();
                Debug.Log($"<color=green>✓ Created new Earth trait at {earthPathInResources}</color>");
            }
            
            AssetDatabase.Refresh();
            
            // Verify the trait is loadable at runtime
            Debug.Log("\n<color=cyan>=== Verifying Runtime Loading ===</color>");
            TowerTrait runtimeEarth = Resources.Load<TowerTrait>("Traits/Earth");
            if (runtimeEarth != null)
            {
                Debug.Log($"<color=green>✓ Earth trait verified! Can be loaded at runtime</color>");
                Debug.Log($"  Name: {runtimeEarth.traitName}");
                Debug.Log($"  Description: {runtimeEarth.description}");
                Debug.Log($"  Duration: {runtimeEarth.trapDuration}s");
            }
            else
            {
                Debug.LogError("<color=red>✗ Failed to load Earth trait via Resources.Load!</color>");
                Debug.LogError("The trait may not be in the correct Resources folder structure.");
            }
        }
        
        private static void CreateFireTrait(string path)
        {
            var trait = ScriptableObject.CreateInstance<TowerTrait>();
            trait.name = "Fire";
            trait.traitName = "Fire";
            trait.description = "+50% damage, burn DoT (3 seconds)";
            trait.category = TraitCategory.Elemental;
            
            // Stats
            trait.damageMultiplier = 1.5f; // +50% damage
            
            // Effects
            trait.hasBurnEffect = true;
            trait.burnDamagePerSecond = 10f;
            trait.burnDuration = 3f;
            
            // Visual
            trait.overlayColor = Color.red;
            trait.overlayAlpha = 0.4f;
            
            AssetDatabase.CreateAsset(trait, $"{path}/Fire.asset");
        }
        
        private static void CreateIceTrait(string path)
        {
            var trait = ScriptableObject.CreateInstance<TowerTrait>();
            trait.name = "Ice";
            trait.traitName = "Ice";
            trait.description = "-30% enemy speed, brittle effect (+25% incoming damage)";
            trait.category = TraitCategory.Elemental;
            
            // Effects
            trait.hasSlowEffect = true;
            trait.slowMultiplier = 0.7f; // 30% speed reduction
            trait.slowDuration = 2f;
            
            trait.hasBrittleEffect = true;
            trait.brittleDamageMultiplier = 1.25f; // +25% incoming damage
            trait.brittleDuration = 3f;
            
            // Visual
            trait.overlayColor = Color.cyan;
            trait.overlayAlpha = 0.4f;
            
            AssetDatabase.CreateAsset(trait, $"{path}/Ice.asset");
        }
        
        private static void CreateLightningTrait(string path)
        {
            var trait = ScriptableObject.CreateInstance<TowerTrait>();
            trait.name = "Lightning";
            trait.traitName = "Lightning";
            trait.description = "Chain to 2 additional enemies";
            trait.category = TraitCategory.Elemental;
            
            // Effects
            trait.hasChainEffect = true;
            trait.chainTargets = 2;
            trait.chainDamageMultiplier = 1f; // Same damage to chained enemies
            trait.chainRange = 2f;
            
            // Visual
            trait.overlayColor = Color.yellow;
            trait.overlayAlpha = 0.4f;
            
            AssetDatabase.CreateAsset(trait, $"{path}/Lightning.asset");
        }
        
        private static void CreateSniperTrait(string path)
        {
            var trait = ScriptableObject.CreateInstance<TowerTrait>();
            trait.name = "Sniper";
            trait.traitName = "Sniper";
            trait.description = "+100% range, +2 second charge time";
            trait.category = TraitCategory.Range;
            
            // Stats
            trait.rangeMultiplier = 2f; // +100% range
            trait.chargeTimeBonus = 2f; // +2 second charge time
            
            // Visual
            trait.overlayColor = Color.green;
            trait.overlayAlpha = 0.3f;
            
            AssetDatabase.CreateAsset(trait, $"{path}/Sniper.asset");
        }
        
        private static void CreateHarvestTrait(string path)
        {
            var trait = ScriptableObject.CreateInstance<TowerTrait>();
            trait.name = "Harvest";
            trait.traitName = "Harvest";
            trait.description = "+1 gold per kill";
            trait.category = TraitCategory.Utility;
            
            // Effects  
            trait.hasGoldReward = true;
            trait.goldPerKill = 1;
            
            // Visual
            trait.overlayColor = Color.yellow;
            trait.overlayAlpha = 0.2f;
            
            AssetDatabase.CreateAsset(trait, $"{path}/Harvest.asset");
        }
        
        private static void CreateExplosionTrait(string path)
        {
            var trait = ScriptableObject.CreateInstance<TowerTrait>();
            trait.name = "Explosion";
            trait.traitName = "Explosion";
            trait.description = "Impact causes explosion dealing 75% damage in 2 unit radius";
            trait.category = TraitCategory.Elemental;
            
            // Effects
            trait.hasExplosionEffect = true;
            trait.explosionRadius = 2f;
            trait.explosionDamageMultiplier = 0.75f; // 75% damage to nearby enemies
            
            // Visual
            trait.overlayColor = new Color(1f, 0.5f, 0f); // Orange
            trait.overlayAlpha = 0.4f;
            
            AssetDatabase.CreateAsset(trait, $"{path}/Explosion.asset");
        }
        
        private static void CreateEarthTrait(string path)
        {
            var trait = ScriptableObject.CreateInstance<TowerTrait>();
            trait.name = "Earth";
            trait.traitName = "Earth";
            trait.description = "Hit enemy becomes black disk trap (3s) - other enemies fall in and die";
            trait.category = TraitCategory.Elemental;
            
            // Effects
            trait.hasEarthTrapEffect = true;
            trait.trapDuration = 3f;
            trait.trapRadius = 1f;
            
            // Visual
            trait.overlayColor = new Color(0.6f, 0.4f, 0.2f); // Brown
            trait.overlayAlpha = 0.4f;
            
            AssetDatabase.CreateAsset(trait, $"{path}/Earth.asset");
        }
    }
}