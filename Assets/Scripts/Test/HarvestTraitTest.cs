using UnityEngine;
using TowerFusion;

namespace TowerFusion.Test
{
    /// <summary>
    /// Comprehensive test component to verify Harvest trait functionality
    /// </summary>
    public class HarvestTraitTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private Tower testTower;
        [SerializeField] private Enemy testEnemy;
        [SerializeField] private bool autoTest = false;
        [SerializeField] private float testInterval = 3f;
        
        [Header("Test Results")]
        [SerializeField] private int initialGold;
        [SerializeField] private int goldAfterKill;
        [SerializeField] private int expectedGoldGain = 1;
        [SerializeField] private bool lastTestPassed;
        
        private float lastTestTime;
        private TowerTrait harvestTrait;
        
        void Start()
        {
            if (testTower == null)
                testTower = FindObjectOfType<Tower>();
                
            if (testEnemy == null)
                testEnemy = FindObjectOfType<Enemy>();
                
            // Create or find harvest trait
            CreateHarvestTrait();
                
            Debug.Log($"Harvest Trait Test initialized. Tower: {testTower?.name}, Enemy: {testEnemy?.name}");
        }
        
        void Update()
        {
            if (autoTest && Time.time - lastTestTime > testInterval)
            {
                TestHarvestTrait();
                lastTestTime = Time.time;
            }
        }
        
        /// <summary>
        /// Create harvest trait for testing
        /// </summary>
        private void CreateHarvestTrait()
        {
            harvestTrait = ScriptableObject.CreateInstance<TowerTrait>();
            harvestTrait.traitName = "Harvest";
            harvestTrait.description = "+1 gold per kill";
            harvestTrait.category = TraitCategory.Utility;
            harvestTrait.hasGoldReward = true;
            harvestTrait.goldPerKill = 1;
            harvestTrait.overlayColor = Color.yellow;
            harvestTrait.overlayAlpha = 0.2f;
        }
        
        /// <summary>
        /// Test harvest trait functionality
        /// </summary>
        [ContextMenu("Test Harvest Trait")]
        public void TestHarvestTrait()
        {
            if (testTower == null || testEnemy == null)
            {
                Debug.LogError("HarvestTraitTest: Missing tower or enemy reference!");
                return;
            }
            
            Debug.Log("=== Testing Harvest Trait ===");
            
            // Record initial gold
            initialGold = GameManager.Instance?.CurrentGold ?? 0;
            Debug.Log($"Initial gold: {initialGold}");
            
            // Apply harvest trait to tower
            if (!testTower.HasTrait(harvestTrait))
            {
                testTower.AddTrait(harvestTrait);
                Debug.Log($"Applied Harvest trait to {testTower.name}");
            }
            
            // Verify trait application
            VerifyTraitApplication();
            
            // Simulate enemy kill
            SimulateEnemyKill();
            
            // Check results
            VerifyGoldIncrease();
        }
        
        /// <summary>
        /// Verify that the harvest trait is properly applied
        /// </summary>
        private void VerifyTraitApplication()
        {
            var traitManager = testTower.GetComponent<TowerTraitManager>();
            if (traitManager == null)
            {
                Debug.LogError("Tower doesn't have TowerTraitManager component!");
                return;
            }
            
            bool hasHarvestTrait = testTower.HasTrait(harvestTrait);
            Debug.Log($"Tower has Harvest trait: {hasHarvestTrait}");
            
            var appliedTraits = traitManager.AppliedTraits;
            Debug.Log($"Applied traits count: {appliedTraits.Count}");
            
            foreach (var trait in appliedTraits)
            {
                Debug.Log($"- Trait: {trait.traitName}, hasGoldReward: {trait.hasGoldReward}, goldPerKill: {trait.goldPerKill}");
            }
        }
        
        /// <summary>
        /// Simulate killing an enemy
        /// </summary>
        private void SimulateEnemyKill()
        {
            if (testEnemy == null)
            {
                Debug.LogError("No test enemy available!");
                return;
            }
            
            var traitManager = testTower.GetComponent<TowerTraitManager>();
            if (traitManager == null)
            {
                Debug.LogError("Tower doesn't have TowerTraitManager component!");
                return;
            }
            
            Debug.Log($"Simulating kill of enemy: {testEnemy.name}");
            Debug.Log($"Calling ApplyTraitEffectsOnKill...");
            
            // Directly call the kill method to test harvest trait
            traitManager.ApplyTraitEffectsOnKill(testEnemy);
        }
        
        /// <summary>
        /// Verify that gold increased correctly
        /// </summary>
        private void VerifyGoldIncrease()
        {
            goldAfterKill = GameManager.Instance?.CurrentGold ?? 0;
            int actualGoldGain = goldAfterKill - initialGold;
            
            Debug.Log($"Gold after kill: {goldAfterKill}");
            Debug.Log($"Expected gold gain: {expectedGoldGain}");
            Debug.Log($"Actual gold gain: {actualGoldGain}");
            
            lastTestPassed = (actualGoldGain == expectedGoldGain);
            
            if (lastTestPassed)
            {
                Debug.Log("✅ HARVEST TRAIT TEST PASSED!");
            }
            else
            {
                Debug.LogWarning($"❌ HARVEST TRAIT TEST FAILED! Expected +{expectedGoldGain} gold, got +{actualGoldGain}");
            }
        }
        
        /// <summary>
        /// Create a test enemy for testing
        /// </summary>
        [ContextMenu("Create Test Enemy")]
        public void CreateTestEnemy()
        {
            if (testEnemy != null)
            {
                DestroyImmediate(testEnemy.gameObject);
            }
            
            // Create a simple test enemy
            GameObject enemyObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemyObj.name = "TestEnemy";
            enemyObj.transform.position = Vector3.zero;
            
            // Add Enemy component (assuming it exists)
            testEnemy = enemyObj.GetComponent<Enemy>();
            if (testEnemy == null)
            {
                testEnemy = enemyObj.AddComponent<Enemy>();
            }
            
            Debug.Log("Created test enemy for Harvest trait testing");
        }
        
        /// <summary>
        /// Remove harvest trait from tower
        /// </summary>
        [ContextMenu("Remove Harvest Trait")]
        public void RemoveHarvestTrait()
        {
            if (testTower != null && harvestTrait != null)
            {
                testTower.RemoveTrait(harvestTrait);
                Debug.Log("Removed Harvest trait from tower");
            }
        }
        
        /// <summary>
        /// Test multiple kills to verify stacking
        /// </summary>
        [ContextMenu("Test Multiple Kills")]
        public void TestMultipleKills()
        {
            if (testTower == null || testEnemy == null)
            {
                Debug.LogError("Missing tower or enemy reference!");
                return;
            }
            
            Debug.Log("=== Testing Multiple Kills ===");
            
            initialGold = GameManager.Instance?.CurrentGold ?? 0;
            Debug.Log($"Starting gold: {initialGold}");
            
            // Apply harvest trait
            if (!testTower.HasTrait(harvestTrait))
            {
                testTower.AddTrait(harvestTrait);
            }
            
            var traitManager = testTower.GetComponent<TowerTraitManager>();
            int killsToTest = 5;
            
            for (int i = 0; i < killsToTest; i++)
            {
                Debug.Log($"Simulating kill #{i + 1}");
                traitManager.ApplyTraitEffectsOnKill(testEnemy);
            }
            
            goldAfterKill = GameManager.Instance?.CurrentGold ?? 0;
            int actualGoldGain = goldAfterKill - initialGold;
            int expectedTotalGold = killsToTest * expectedGoldGain;
            
            Debug.Log($"After {killsToTest} kills:");
            Debug.Log($"Expected total gold gain: {expectedTotalGold}");
            Debug.Log($"Actual total gold gain: {actualGoldGain}");
            
            bool multiKillTestPassed = (actualGoldGain == expectedTotalGold);
            
            if (multiKillTestPassed)
            {
                Debug.Log("✅ MULTIPLE KILLS TEST PASSED!");
            }
            else
            {
                Debug.LogWarning($"❌ MULTIPLE KILLS TEST FAILED!");
            }
        }
        
        void OnDrawGizmos()
        {
            if (testTower != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(testTower.transform.position, 0.5f);
                Gizmos.DrawLine(testTower.transform.position, testTower.transform.position + Vector3.up * 2f);
            }
            
            if (testEnemy != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(testEnemy.transform.position, 0.3f);
            }
            
            // Draw test status
            if (testTower != null)
            {
                Gizmos.color = lastTestPassed ? Color.green : Color.red;
                Vector3 statusPos = testTower.transform.position + Vector3.up * 3f;
                Gizmos.DrawWireCube(statusPos, Vector3.one * 0.2f);
            }
        }
    }
}