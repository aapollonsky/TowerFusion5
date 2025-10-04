using UnityEngine;
using UnityEditor;
using TowerFusion;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Editor utility to test ice trait effects on enemies
    /// </summary>
    public static class IceTraitTester
    {
        [MenuItem("Tools/Tower Fusion/Test Ice Trait on Selected Enemy")]
        public static void TestIceTraitOnSelectedEnemy()
        {
            Enemy selectedEnemy = Selection.activeGameObject?.GetComponent<Enemy>();
            if (selectedEnemy == null)
            {
                Debug.LogWarning("Please select an Enemy GameObject to test ice trait");
                return;
            }
            
            Debug.Log($"Testing ice trait on {selectedEnemy.name}");
            Debug.Log($"Original speed: {selectedEnemy.EnemyData.moveSpeed}");
            Debug.Log($"Current speed: {selectedEnemy.CurrentSpeed}");
            Debug.Log($"Is slowed: {selectedEnemy.IsSlowed}");
            
            // Apply ice effects manually for testing
            selectedEnemy.ApplySlowEffect(0.7f, 3f); // 30% speed reduction for 3 seconds
            selectedEnemy.ApplyBrittleEffect(1.25f, 3f); // 25% more damage for 3 seconds
            
            Debug.Log($"Applied ice effects - New current speed: {selectedEnemy.CurrentSpeed}");
        }
        
        [MenuItem("Tools/Tower Fusion/Test Ice Trait via Tower Attack")]
        public static void TestIceTraitViaTowerAttack()
        {
            // Find tower with ice trait
            Tower[] towers = Object.FindObjectsOfType<Tower>();
            Tower iceTower = null;
            
            foreach (var tower in towers)
            {
                if (tower.HasTraitOfCategory(TraitCategory.Elemental))
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
            }
            
            if (iceTower == null)
            {
                Debug.LogWarning("No tower with Ice trait found. Please add Ice trait to a tower first.");
                return;
            }
            
            // Find an enemy
            Enemy enemy = Object.FindObjectOfType<Enemy>();
            if (enemy == null)
            {
                Debug.LogWarning("No enemy found in scene");
                return;
            }
            
            Debug.Log($"Testing ice trait from {iceTower.name} on {enemy.name}");
            Debug.Log($"Enemy speed before: {enemy.CurrentSpeed}");
            
            // Simulate tower attack with ice trait
            if (iceTower.TraitManager != null)
            {
                iceTower.TraitManager.ApplyTraitEffectsOnAttack(enemy, 25f);
                Debug.Log($"Enemy speed after ice attack: {enemy.CurrentSpeed}");
                Debug.Log($"Enemy is slowed: {enemy.IsSlowed}");
                Debug.Log($"Enemy is brittle: {enemy.IsBrittle}");
            }
        }
        
        [MenuItem("Tools/Tower Fusion/Create Ice Tower for Testing")]
        public static void CreateIceTowerForTesting()
        {
            // Create a basic tower for testing
            GameObject towerObj = new GameObject("Ice Test Tower");
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
            
            // Add ice trait
            TowerTrait iceTrait = AssetDatabase.LoadAssetAtPath<TowerTrait>("Assets/Data/Traits/Ice.asset");
            if (iceTrait != null)
            {
                tower.AddTrait(iceTrait);
                Debug.Log("Created Ice Test Tower with Ice trait applied");
            }
            else
            {
                Debug.LogWarning("Ice trait asset not found. Please create traits first using 'Tools/Tower Fusion/Create Default Traits'");
            }
            
            Selection.activeGameObject = towerObj;
        }
    }
}