using UnityEngine;
using UnityEditor;
using TowerFusion;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Debug utilities for trait system testing
    /// </summary>
    public static class TraitSystemDebugger
    {
        [MenuItem("Tools/Tower Fusion/Debug: Show All Tower Traits")]
        public static void ShowAllTowerTraits()
        {
            Tower[] towers = Object.FindObjectsOfType<Tower>();
            
            if (towers.Length == 0)
            {
                Debug.Log("No towers found in scene");
                return;
            }
            
            Debug.Log($"=== Tower Traits Debug Report ({towers.Length} towers) ===");
            
            foreach (Tower tower in towers)
            {
                Debug.Log($"\nTower: {tower.name} (Position: {tower.transform.position})");
                
                if (tower.TraitManager == null)
                {
                    Debug.LogWarning($"  - No TraitManager component!");
                    continue;
                }
                
                var traits = tower.GetAppliedTraits();
                Debug.Log($"  - Applied Traits: {traits.Count}");
                
                foreach (var trait in traits)
                {
                    Debug.Log($"    • {trait.traitName}: {trait.description}");
                    Debug.Log($"      Category: {trait.category}");
                    Debug.Log($"      Has slow effect: {trait.hasSlowEffect} (Multiplier: {trait.slowMultiplier})");
                    Debug.Log($"      Has brittle effect: {trait.hasBrittleEffect} (Multiplier: {trait.brittleDamageMultiplier})");
                }
                
                if (traits.Count == 0)
                {
                    Debug.Log("    (No traits applied)");
                }
            }
        }
        
        [MenuItem("Tools/Tower Fusion/Debug: Test Ice Trait Now")]
        public static void TestIceTraitNow()
        {
            // Find tower with ice trait
            Tower iceTower = null;
            Tower[] towers = Object.FindObjectsOfType<Tower>();
            
            foreach (Tower tower in towers)
            {
                var traits = tower.GetAppliedTraits();
                foreach (var trait in traits)
                {
                    if (trait.traitName == "Ice")
                    {
                        iceTower = tower;
                        break;
                    }
                }
                if (iceTower != null) break;
            }
            
            if (iceTower == null)
            {
                Debug.LogError("No tower with Ice trait found! Please add Ice trait to a tower first.");
                return;
            }
            
            // Find an enemy
            Enemy testEnemy = Object.FindObjectOfType<Enemy>();
            if (testEnemy == null)
            {
                Debug.LogError("No enemy found in scene! Please start a wave first.");
                return;
            }
            
            Debug.Log($"=== Ice Trait Test ===");
            Debug.Log($"Tower: {iceTower.name}");
            Debug.Log($"Enemy: {testEnemy.name}");
            Debug.Log($"Enemy speed before: {testEnemy.CurrentSpeed}");
            Debug.Log($"Enemy is slowed before: {testEnemy.IsSlowed}");
            
            // Force apply ice trait effects
            if (iceTower.TraitManager != null)
            {
                Debug.Log("Manually applying ice trait effects...");
                iceTower.TraitManager.ApplyTraitEffectsOnAttack(testEnemy, 25f);
                
                // Wait a frame and check results
                EditorApplication.delayCall += () => {
                    Debug.Log($"Enemy speed after: {testEnemy.CurrentSpeed}");
                    Debug.Log($"Enemy is slowed after: {testEnemy.IsSlowed}");
                    Debug.Log($"Enemy is brittle after: {testEnemy.IsBrittle}");
                };
            }
        }
    }
}