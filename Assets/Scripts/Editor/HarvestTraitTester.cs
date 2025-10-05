using UnityEngine;
using UnityEditor;
using TowerFusion;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Editor window for testing Harvest trait functionality
    /// </summary>
    public class HarvestTraitTester : EditorWindow
    {
        private Tower selectedTower;
        private int initialGold;
        private int currentGold;
        private TowerTrait harvestTrait;
        
        [MenuItem("TowerFusion/Debug/Harvest Trait Tester")]
        public static void ShowWindow()
        {
            GetWindow<HarvestTraitTester>("Harvest Trait Tester");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Harvest Trait Testing", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            selectedTower = (Tower)EditorGUILayout.ObjectField("Tower:", selectedTower, typeof(Tower), true);
            
            EditorGUILayout.Space();
            
            // Show current game manager gold
            if (GameManager.Instance != null)
            {
                EditorGUILayout.LabelField("Current Gold:", GameManager.Instance.CurrentGold.ToString());
            }
            else
            {
                EditorGUILayout.LabelField("GameManager:", "Not Found", EditorStyles.boldLabel);
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Create & Apply Harvest Trait"))
            {
                CreateAndApplyHarvestTrait();
            }
            
            if (GUILayout.Button("Simulate Enemy Kill"))
            {
                SimulateEnemyKill();
            }
            
            if (GUILayout.Button("Test Multiple Kills (5x)"))
            {
                TestMultipleKills();
            }
            
            if (GUILayout.Button("Remove Harvest Trait"))
            {
                RemoveHarvestTrait();
            }
            
            EditorGUILayout.Space();
            
            // Show tower traits
            if (selectedTower != null)
            {
                var traitManager = selectedTower.GetComponent<TowerTraitManager>();
                if (traitManager != null)
                {
                    EditorGUILayout.LabelField("Applied Traits:", traitManager.AppliedTraits.Count.ToString());
                    foreach (var trait in traitManager.AppliedTraits)
                    {
                        EditorGUILayout.LabelField($"- {trait.traitName}", 
                            trait.hasGoldReward ? $"Gold per kill: {trait.goldPerKill}" : "No gold reward");
                    }
                }
            }
        }
        
        void CreateAndApplyHarvestTrait()
        {
            if (selectedTower == null)
            {
                Debug.LogWarning("No tower selected!");
                return;
            }
            
            // Create harvest trait
            harvestTrait = ScriptableObject.CreateInstance<TowerTrait>();
            harvestTrait.name = "Harvest";
            harvestTrait.traitName = "Harvest";
            harvestTrait.description = "+1 gold per kill";
            harvestTrait.category = TraitCategory.Utility;
            harvestTrait.hasGoldReward = true;
            harvestTrait.goldPerKill = 1;
            harvestTrait.overlayColor = Color.yellow;
            harvestTrait.overlayAlpha = 0.2f;
            
            var traitManager = selectedTower.GetComponent<TowerTraitManager>();
            if (traitManager == null)
            {
                Debug.LogWarning("Tower doesn't have TowerTraitManager component!");
                return;
            }
            
            traitManager.AddTrait(harvestTrait);
            
            Debug.Log($"Applied Harvest Trait to {selectedTower.name}");
            Debug.Log($"Trait configuration: hasGoldReward={harvestTrait.hasGoldReward}, goldPerKill={harvestTrait.goldPerKill}");
        }
        
        void SimulateEnemyKill()
        {
            if (selectedTower == null)
            {
                Debug.LogWarning("No tower selected!");
                return;
            }
            
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not found! Please ensure GameManager exists in scene.");
                return;
            }
            
            var traitManager = selectedTower.GetComponent<TowerTraitManager>();
            if (traitManager == null)
            {
                Debug.LogWarning("Tower doesn't have TowerTraitManager component!");
                return;
            }
            
            // Record initial gold
            initialGold = GameManager.Instance.CurrentGold;
            
            // Create a dummy enemy for the kill simulation
            GameObject dummyEnemyObj = new GameObject("DummyEnemy");
            Enemy dummyEnemy = dummyEnemyObj.AddComponent<Enemy>();
            
            Debug.Log($"=== Simulating Enemy Kill ===");
            Debug.Log($"Gold before kill: {initialGold}");
            
            // Simulate the kill
            traitManager.ApplyTraitEffectsOnKill(dummyEnemy);
            
            // Check results
            currentGold = GameManager.Instance.CurrentGold;
            int goldGain = currentGold - initialGold;
            
            Debug.Log($"Gold after kill: {currentGold}");
            Debug.Log($"Gold gained: {goldGain}");
            
            if (goldGain == 1)
            {
                Debug.Log("✅ HARVEST TRAIT TEST PASSED!");
            }
            else
            {
                Debug.LogWarning($"❌ HARVEST TRAIT TEST FAILED! Expected +1 gold, got +{goldGain}");
            }
            
            // Cleanup
            DestroyImmediate(dummyEnemyObj);
        }
        
        void TestMultipleKills()
        {
            if (selectedTower == null || GameManager.Instance == null)
            {
                Debug.LogWarning("Missing tower or GameManager!");
                return;
            }
            
            var traitManager = selectedTower.GetComponent<TowerTraitManager>();
            if (traitManager == null) return;
            
            initialGold = GameManager.Instance.CurrentGold;
            Debug.Log($"=== Testing Multiple Kills ===");
            Debug.Log($"Starting gold: {initialGold}");
            
            // Create dummy enemy
            GameObject dummyEnemyObj = new GameObject("DummyEnemy");
            Enemy dummyEnemy = dummyEnemyObj.AddComponent<Enemy>();
            
            int killsToTest = 5;
            for (int i = 0; i < killsToTest; i++)
            {
                Debug.Log($"Simulating kill #{i + 1}");
                traitManager.ApplyTraitEffectsOnKill(dummyEnemy);
            }
            
            currentGold = GameManager.Instance.CurrentGold;
            int totalGoldGain = currentGold - initialGold;
            
            Debug.Log($"After {killsToTest} kills:");
            Debug.Log($"Expected total gold: {killsToTest}");
            Debug.Log($"Actual total gold gained: {totalGoldGain}");
            
            if (totalGoldGain == killsToTest)
            {
                Debug.Log("✅ MULTIPLE KILLS TEST PASSED!");
            }
            else
            {
                Debug.LogWarning($"❌ MULTIPLE KILLS TEST FAILED!");
            }
            
            // Cleanup
            DestroyImmediate(dummyEnemyObj);
        }
        
        void RemoveHarvestTrait()
        {
            if (selectedTower == null || harvestTrait == null)
            {
                Debug.LogWarning("No tower or harvest trait selected!");
                return;
            }
            
            selectedTower.RemoveTrait(harvestTrait);
            Debug.Log("Removed Harvest trait from tower");
        }
    }
}