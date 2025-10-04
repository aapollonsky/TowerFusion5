using UnityEngine;
using UnityEditor;
using TowerFusion;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Debug utilities for fire trait testing
    /// </summary>
    public static class FireTraitTester
    {
        [MenuItem("Tools/Tower Fusion/Test Fire Trait on Selected Enemy")]
        public static void TestFireTraitOnSelectedEnemy()
        {
            Enemy selectedEnemy = Selection.activeGameObject?.GetComponent<Enemy>();
            if (selectedEnemy == null)
            {
                Debug.LogWarning("Please select an Enemy GameObject to test fire trait");
                return;
            }
            
            Debug.Log($"Testing fire trait on {selectedEnemy.name}");
            Debug.Log($"Enemy health before: {selectedEnemy.CurrentHealth}");
            Debug.Log($"Is burning: {selectedEnemy.IsBurning}");
            
            // Apply fire effects manually for testing
            selectedEnemy.ApplyBurnEffect(10f, 5f); // 10 DPS for 5 seconds
            
            Debug.Log($"Applied fire effects - Is burning: {selectedEnemy.IsBurning}");
        }
        
        [MenuItem("Tools/Tower Fusion/Test Fire Trait via Tower Attack")]
        public static void TestFireTraitViaTowerAttack()
        {
            // Find tower with fire trait
            Tower[] towers = Object.FindObjectsOfType<Tower>();
            Tower fireTower = null;
            
            foreach (var tower in towers)
            {
                if (tower.HasTraitOfCategory(TraitCategory.Elemental))
                {
                    var traits = tower.GetAppliedTraits();
                    foreach (var trait in traits)
                    {
                        if (trait.traitName == "Fire")
                        {
                            fireTower = tower;
                            break;
                        }
                    }
                    if (fireTower != null) break;
                }
            }
            
            if (fireTower == null)
            {
                Debug.LogWarning("No tower with Fire trait found. Please add Fire trait to a tower first.");
                return;
            }
            
            // Find an enemy
            Enemy enemy = Object.FindObjectOfType<Enemy>();
            if (enemy == null)
            {
                Debug.LogWarning("No enemy found in scene");
                return;
            }
            
            Debug.Log($"Testing fire trait from {fireTower.name} on {enemy.name}");
            Debug.Log($"Enemy health before: {enemy.CurrentHealth}");
            Debug.Log($"Enemy is burning before: {enemy.IsBurning}");
            
            // Simulate tower attack with fire trait
            if (fireTower.TraitManager != null)
            {
                fireTower.TraitManager.ApplyTraitEffectsOnAttack(enemy, 25f);
                Debug.Log($"Enemy health after fire attack: {enemy.CurrentHealth}");
                Debug.Log($"Enemy is burning after: {enemy.IsBurning}");
            }
        }
        
        [MenuItem("Tools/Tower Fusion/Create Fire Tower for Testing")]
        public static void CreateFireTowerForTesting()
        {
            // Create a basic tower for testing
            GameObject towerObj = new GameObject("Fire Test Tower");
            towerObj.transform.position = Vector3.zero;
            
            // Add required components
            Tower tower = towerObj.AddComponent<Tower>();
            SpriteRenderer spriteRenderer = towerObj.AddComponent<SpriteRenderer>();
            CircleCollider2D collider = towerObj.AddComponent<CircleCollider2D>();
            
            // Find a TowerData asset
            string[] guids = AssetDatabase.FindAssets("t:TowerData");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                TowerData towerData = AssetDatabase.LoadAssetAtPath<TowerData>(path);
                
                // Use reflection to set the towerData (since it's private)
                var field = typeof(Tower).GetField("towerData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(tower, towerData);
                    tower.Initialize(towerData);
                }
            }
            
            // Add fire trait
            TowerTrait fireTrait = AssetDatabase.LoadAssetAtPath<TowerTrait>("Assets/Data/Traits/Fire.asset");
            if (fireTrait != null)
            {
                tower.AddTrait(fireTrait);
                Debug.Log("Created Fire Test Tower with Fire trait applied");
                Debug.Log($"Fire trait config: DPS={fireTrait.burnDamagePerSecond}, Duration={fireTrait.burnDuration}s");
            }
            else
            {
                Debug.LogWarning("Fire trait asset not found. Please create traits first using 'Tools/Tower Fusion/Create Default Traits'");
            }
            
            Selection.activeGameObject = towerObj;
        }
        
        [MenuItem("Tools/Tower Fusion/Show All Enemy Status Effects")]
        public static void ShowAllEnemyStatusEffects()
        {
            Enemy[] enemies = Object.FindObjectsOfType<Enemy>();
            
            if (enemies.Length == 0)
            {
                Debug.LogWarning("No enemies found in scene");
                return;
            }
            
            Debug.Log($"=== Enemy Status Effects Report ({enemies.Length} enemies) ===");
            
            foreach (Enemy enemy in enemies)
            {
                Debug.Log($"\nEnemy: {enemy.name}");
                Debug.Log($"  Health: {enemy.CurrentHealth}/{enemy.MaxHealth}");
                Debug.Log($"  Speed: {enemy.CurrentSpeed} (base: {enemy.EnemyData.moveSpeed})");
                Debug.Log($"  Status Effects:");
                Debug.Log($"    - Burning: {enemy.IsBurning}");
                Debug.Log($"    - Slowed: {enemy.IsSlowed}");
                Debug.Log($"    - Brittle: {enemy.IsBrittle}");
                
                var spriteRenderer = enemy.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Debug.Log($"  Current Color: {spriteRenderer.color}");
                }
            }
        }
    }
}