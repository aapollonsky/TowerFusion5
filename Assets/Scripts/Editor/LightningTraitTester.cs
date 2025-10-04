using UnityEngine;
using UnityEditor;
using System.Linq;
using TowerFusion;
using TowerFusion.Editor;

[System.Serializable]
public class LightningTraitTester : EditorWindow
{
    private Tower selectedTower;
    private Enemy[] enemies;
    
    [MenuItem("TowerFusion/Debug/Lightning Trait Tester")]
    public static void ShowWindow()
    {
        GetWindow<LightningTraitTester>("Lightning Trait Tester");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Lightning Trait Testing", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        selectedTower = (Tower)EditorGUILayout.ObjectField("Tower:", selectedTower, typeof(Tower), true);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Apply Lightning Trait"))
        {
            ApplyLightningTrait();
        }
        
        if (GUILayout.Button("Test Chain Lightning"))
        {
            TestChainLightning();
        }
        
        if (GUILayout.Button("Spawn Test Enemies"))
        {
            SpawnTestEnemies();
        }
        
        if (GUILayout.Button("Remove All Traits"))
        {
            RemoveTraits();
        }
        
        EditorGUILayout.Space();
        
        if (selectedTower != null)
        {
            var traitManager = selectedTower.GetComponent<TowerTraitManager>();
            if (traitManager != null)
            {
                EditorGUILayout.LabelField("Applied Traits:", traitManager.AppliedTraits.Count.ToString());
                foreach (var trait in traitManager.AppliedTraits)
                {
                    EditorGUILayout.LabelField("- " + trait.traitName, trait.hasChainEffect ? "Chain Effect: Yes" : "Chain Effect: No");
                }
            }
        }
        
        // Show nearby enemies
        enemies = FindObjectsOfType<Enemy>();
        EditorGUILayout.LabelField("Enemies in Scene:", enemies.Length.ToString());
    }
    
    void ApplyLightningTrait()
    {
        if (selectedTower == null)
        {
            Debug.LogWarning("No tower selected!");
            return;
        }
        
        var traitManager = selectedTower.GetComponent<TowerTraitManager>();
        if (traitManager == null)
        {
            Debug.LogWarning("Tower doesn't have TowerTraitManager component!");
            return;
        }
        
        var lightningTrait = ScriptableObject.CreateInstance<TowerTrait>();
        lightningTrait.traitName = "Lightning";
        lightningTrait.description = "Chain lightning between enemies";
        lightningTrait.overlayColor = Color.yellow;
        lightningTrait.hasChainEffect = true;
        lightningTrait.chainTargets = 2;
        lightningTrait.chainRange = 2f;
        lightningTrait.chainDamageMultiplier = 1f;
        
        traitManager.AddTrait(lightningTrait);
        
        Debug.Log($"Applied Lightning Trait to {selectedTower.name}");
        Debug.Log($"Chain Targets: {lightningTrait.chainTargets}");
        Debug.Log($"Chain Range: {lightningTrait.chainRange}");
        Debug.Log($"Chain Damage Multiplier: {lightningTrait.chainDamageMultiplier}");
    }
    
    void TestChainLightning()
    {
        if (selectedTower == null)
        {
            Debug.LogWarning("No tower selected!");
            return;
        }
        
        var traitManager = selectedTower.GetComponent<TowerTraitManager>();
        if (traitManager == null)
        {
            Debug.LogWarning("Tower doesn't have TowerTraitManager component!");
            return;
        }
        
        // Find an enemy to target
        enemies = FindObjectsOfType<Enemy>();
        if (enemies.Length == 0)
        {
            Debug.LogWarning("No enemies found! Use 'Spawn Test Enemies' first.");
            return;
        }
        
        var targetEnemy = enemies[0];
        
        // Simulate trait effect application with damage
        traitManager.ApplyTraitEffectsOnAttack(targetEnemy, 50f); // 50 damage
        
        Debug.Log($"Testing chain lightning on {targetEnemy.name}");
    }
    
    void SpawnTestEnemies()
    {
        if (selectedTower == null)
        {
            Debug.LogWarning("No tower selected!");
            return;
        }
        
        // Find enemy prefab
        var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy/BasicEnemy.prefab");
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy prefab not found at Assets/Prefabs/Enemy/BasicEnemy.prefab");
            return;
        }
        
        Vector3 towerPosition = selectedTower.transform.position;
        
        // Spawn enemies in a circle around the tower
        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            Vector3 spawnPosition = towerPosition + new Vector3(
                Mathf.Cos(angle) * 3f, 
                Mathf.Sin(angle) * 3f, 
                0f
            );
            
            var enemy = PrefabUtility.InstantiatePrefab(enemyPrefab) as GameObject;
            if (enemy != null)
            {
                enemy.transform.position = spawnPosition;
                enemy.name = $"TestEnemy_{i}";
                Debug.Log($"Spawned {enemy.name} at {spawnPosition}");
            }
        }
    }
    
    void RemoveTraits()
    {
        if (selectedTower == null)
        {
            Debug.LogWarning("No tower selected!");
            return;
        }
        
        var traitManager = selectedTower.GetComponent<TowerTraitManager>();
        if (traitManager != null)
        {
            // Clear all traits
            traitManager.ClearAllTraits();
            
            Debug.Log($"Removed all traits from {selectedTower.name}");
        }
    }
}