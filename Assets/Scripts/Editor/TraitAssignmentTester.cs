using UnityEngine;
using UnityEditor;
using TowerFusion;
using TowerFusion.UI;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Editor window for testing the complete trait assignment system
    /// </summary>
    public class TraitAssignmentTester : EditorWindow
    {
        private Tower selectedTower;
        private GameUI gameUI;
        
        [MenuItem("TowerFusion/Debug/Trait Assignment Tester")]
        public static void ShowWindow()
        {
            GetWindow<TraitAssignmentTester>("Trait Assignment Tester");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Trait Assignment System Testing", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            // Find components
            if (gameUI == null)
                gameUI = FindObjectOfType<GameUI>();
                
            selectedTower = (Tower)EditorGUILayout.ObjectField("Tower:", selectedTower, typeof(Tower), true);
            
            EditorGUILayout.Space();
            
            // Show system status
            EditorGUILayout.LabelField("System Status:", EditorStyles.miniBoldLabel);
            
            if (GameManager.Instance != null)
            {
                EditorGUILayout.LabelField("GameManager:", "Found ✓");
                EditorGUILayout.LabelField("Current Gold:", GameManager.Instance.CurrentGold.ToString());
                EditorGUILayout.LabelField("Game State:", GameManager.Instance.GameState.ToString());
            }
            else
            {
                EditorGUILayout.LabelField("GameManager:", "Missing ✗", EditorStyles.boldLabel);
            }
            
            if (TowerManager.Instance != null)
            {
                EditorGUILayout.LabelField("TowerManager:", "Found ✓");
                EditorGUILayout.LabelField("Selected Tower:", 
                    TowerManager.Instance.GetSelectedTower()?.name ?? "None");
            }
            else
            {
                EditorGUILayout.LabelField("TowerManager:", "Missing ✗", EditorStyles.boldLabel);
            }
            
            if (gameUI != null)
            {
                EditorGUILayout.LabelField("GameUI:", "Found ✓");
            }
            else
            {
                EditorGUILayout.LabelField("GameUI:", "Missing ✗", EditorStyles.boldLabel);
            }
            
            EditorGUILayout.Space();
            
            // Test buttons
            EditorGUILayout.LabelField("Test Actions:", EditorStyles.miniBoldLabel);
            
            if (GUILayout.Button("Simulate Trait Generation"))
            {
                SimulateTraitGeneration();
            }
            
            if (GUILayout.Button("Select Test Tower"))
            {
                SelectTestTower();
            }
            
            if (GUILayout.Button("Simulate Trait Assignment"))
            {
                SimulateTraitAssignment();
            }
            
            if (GUILayout.Button("Test Complete Flow"))
            {
                TestCompleteFlow();
            }
            
            EditorGUILayout.Space();
            
            // Show current state
            if (selectedTower != null)
            {
                EditorGUILayout.LabelField("Selected Tower Info:", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("Name:", selectedTower.name);
                EditorGUILayout.LabelField("Type:", selectedTower.TowerData?.towerName ?? "Unknown");
                
                var traits = selectedTower.GetAppliedTraits();
                EditorGUILayout.LabelField("Applied Traits:", traits.Count.ToString());
                foreach (var trait in traits)
                {
                    EditorGUILayout.LabelField($"  - {trait.traitName}:", trait.description);
                }
            }
        }
        
        void SimulateTraitGeneration()
        {
            if (gameUI == null)
            {
                Debug.LogWarning("GameUI not found! Please ensure GameUI exists in scene.");
                return;
            }
            
            Debug.Log("=== Simulating Trait Generation ===");
            
            // This would normally happen when player clicks "trait" button
            var traits = Resources.LoadAll<TowerTrait>("Data/Traits");
            if (traits.Length == 0)
            {
                Debug.LogWarning("No traits found! Please run Tools > Tower Fusion > Setup Trait System");
                return;
            }
            
            var randomTrait = traits[Random.Range(0, traits.Length)];
            Debug.Log($"Generated trait: {randomTrait.traitName} - {randomTrait.description}");
            
            // Simulate accepting the trait
            Debug.Log("Simulating 'done' button click...");
            Debug.Log($"Trait '{randomTrait.traitName}' is now available for assignment!");
        }
        
        void SelectTestTower()
        {
            if (selectedTower == null)
            {
                Debug.LogWarning("Please assign a tower in the Tower field above.");
                return;
            }
            
            if (TowerManager.Instance == null)
            {
                Debug.LogWarning("TowerManager not found! Please ensure TowerManager exists in scene.");
                return;
            }
            
            Debug.Log($"=== Selecting Tower: {selectedTower.name} ===");
            TowerManager.Instance.SelectTower(selectedTower);
            Debug.Log("Tower selected! Check if trait assignment button appears in UI.");
        }
        
        void SimulateTraitAssignment()
        {
            if (selectedTower == null)
            {
                Debug.LogWarning("Please assign a tower in the Tower field above.");
                return;
            }
            
            Debug.Log("=== Simulating Trait Assignment ===");
            
            // Create a test trait
            var testTrait = ScriptableObject.CreateInstance<TowerTrait>();
            testTrait.traitName = "Test Trait";
            testTrait.description = "A test trait for validation";
            testTrait.category = TraitCategory.Elemental;
            testTrait.damageMultiplier = 1.2f;
            testTrait.overlayColor = Color.red;
            
            Debug.Log($"Attempting to assign '{testTrait.traitName}' to {selectedTower.name}");
            
            if (selectedTower.AddTrait(testTrait))
            {
                Debug.Log("✅ Trait assignment successful!");
                Debug.Log($"Tower now has {selectedTower.GetAppliedTraits().Count} traits applied.");
            }
            else
            {
                Debug.LogWarning("❌ Trait assignment failed! Tower may already have this trait or reached limit.");
            }
        }
        
        void TestCompleteFlow()
        {
            Debug.Log("=== Testing Complete Trait Assignment Flow ===");
            Debug.Log("1. Simulating trait generation...");
            SimulateTraitGeneration();
            
            Debug.Log("2. Selecting tower...");
            SelectTestTower();
            
            Debug.Log("3. Attempting trait assignment...");
            SimulateTraitAssignment();
            
            Debug.Log("=== Complete Flow Test Finished ===");
            Debug.Log("Check the UI to verify all components are working correctly.");
            Debug.Log("Expected behavior:");
            Debug.Log("- Trait button should show available trait");
            Debug.Log("- Selected tower should show in tower info panel");
            Debug.Log("- Apply trait button should be visible and functional");
        }
    }
}