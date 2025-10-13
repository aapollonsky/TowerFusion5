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
            
            CreateFireTrait(traitsPath);
            CreateIceTrait(traitsPath);
            CreateLightningTrait(traitsPath);
            CreateSniperTrait(traitsPath);
            CreateHarvestTrait(traitsPath);
            CreateExplosionTrait(traitsPath);
            CreateEarthTrait(traitsPath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("Created default tower traits in " + traitsPath);
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
            trait.description = "Turns hit enemy into ground trap (4 seconds, 1 unit radius)";
            trait.category = TraitCategory.Elemental;
            
            // Effects
            trait.hasEarthTrapEffect = true;
            trait.trapDuration = 4f;
            trait.trapRadius = 1f;
            
            // Visual
            trait.overlayColor = new Color(0.6f, 0.4f, 0.2f); // Brown
            trait.overlayAlpha = 0.4f;
            
            AssetDatabase.CreateAsset(trait, $"{path}/Earth.asset");
        }
    }
}