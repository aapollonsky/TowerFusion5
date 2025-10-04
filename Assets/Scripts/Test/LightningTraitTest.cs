using UnityEngine;
using TowerFusion;

namespace TowerFusion
{
    /// <summary>
    /// Simple test component to verify lightning trait functionality
    /// </summary>
    public class LightningTraitTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public Tower testTower;
        public Enemy[] testEnemies;
        
        [Header("Test Controls")]
        [Space]
        public bool autoTest = false;
        public float testInterval = 3f;
        
        private float lastTestTime;
        
        void Start()
        {
            if (testTower == null)
                testTower = FindObjectOfType<Tower>();
                
            if (testEnemies == null || testEnemies.Length == 0)
                testEnemies = FindObjectsOfType<Enemy>();
                
            Debug.Log($"Lightning Trait Test initialized. Tower: {testTower?.name}, Enemies: {testEnemies?.Length}");
        }
        
        void Update()
        {
            if (autoTest && Time.time - lastTestTime > testInterval)
            {
                TestLightningChain();
                lastTestTime = Time.time;
            }
        }
        
        [ContextMenu("Apply Lightning Trait")]
        public void ApplyLightningTrait()
        {
            if (testTower == null)
            {
                Debug.LogWarning("No test tower assigned!");
                return;
            }
            
            var traitManager = testTower.GetComponent<TowerTraitManager>();
            if (traitManager == null)
            {
                Debug.LogWarning("Tower doesn't have TowerTraitManager component!");
                return;
            }
            
            // Create lightning trait
            var lightningTrait = ScriptableObject.CreateInstance<TowerTrait>();
            lightningTrait.traitName = "Lightning";
            lightningTrait.description = "Chain lightning between enemies";
            lightningTrait.overlayColor = Color.yellow;
            lightningTrait.hasChainEffect = true;
            lightningTrait.chainTargets = 2;
            lightningTrait.chainRange = 3f; // Slightly larger range for easier testing
            lightningTrait.chainDamageMultiplier = 1f;
            
            bool added = traitManager.AddTrait(lightningTrait);
            Debug.Log($"Lightning trait added: {added}. Applied traits count: {traitManager.AppliedTraits.Count}");
            
            // Log trait details
            foreach (var trait in traitManager.AppliedTraits)
            {
                Debug.Log($"Trait: {trait.traitName}, Chain Effect: {trait.hasChainEffect}, Targets: {trait.chainTargets}, Range: {trait.chainRange}");
            }
        }
        
        [ContextMenu("Test Lightning Chain")]
        public void TestLightningChain()
        {
            if (testTower == null)
            {
                Debug.LogWarning("No test tower assigned!");
                return;
            }
            
            var traitManager = testTower.GetComponent<TowerTraitManager>();
            if (traitManager == null)
            {
                Debug.LogWarning("Tower doesn't have TowerTraitManager component!");
                return;
            }
            
            if (testEnemies == null || testEnemies.Length == 0)
            {
                Debug.LogWarning("No test enemies found!");
                return;
            }
            
            // Test chain lightning on first enemy
            var targetEnemy = testEnemies[0];
            if (targetEnemy == null)
            {
                Debug.LogWarning("Target enemy is null!");
                return;
            }
            
            Debug.Log($"Testing chain lightning on {targetEnemy.name} at position {targetEnemy.transform.position}");
            
            // Show nearby enemies
            foreach (var enemy in testEnemies)
            {
                if (enemy != null && enemy != targetEnemy)
                {
                    float distance = Vector3.Distance(targetEnemy.transform.position, enemy.transform.position);
                    Debug.Log($"Enemy {enemy.name} at distance {distance:F2} from target");
                }
            }
            
            // Apply trait effects (this should trigger chain lightning if trait is applied)
            traitManager.ApplyTraitEffectsOnAttack(targetEnemy, 100f);
        }
        
        [ContextMenu("Remove All Traits")]
        public void RemoveAllTraits()
        {
            if (testTower == null)
            {
                Debug.LogWarning("No test tower assigned!");
                return;
            }
            
            var traitManager = testTower.GetComponent<TowerTraitManager>();
            if (traitManager != null)
            {
                traitManager.ClearAllTraits();
                Debug.Log("All traits removed");
            }
        }
        
        [ContextMenu("Setup Test Enemies")]
        public void SetupTestEnemies()
        {
            if (testTower == null)
            {
                Debug.LogWarning("No test tower assigned!");
                return;
            }
            
            // Find all enemies in scene
            var allEnemies = FindObjectsOfType<Enemy>();
            if (allEnemies.Length == 0)
            {
                Debug.LogWarning("No enemies found in scene!");
                return;
            }
            
            testEnemies = allEnemies;
            
            // Position enemies in a line for easy chain testing
            Vector3 towerPos = testTower.transform.position;
            for (int i = 0; i < testEnemies.Length && i < 4; i++)
            {
                if (testEnemies[i] != null)
                {
                    Vector3 enemyPos = towerPos + new Vector3(2f + i * 1.5f, 0f, 0f);
                    testEnemies[i].transform.position = enemyPos;
                    Debug.Log($"Positioned {testEnemies[i].name} at {enemyPos}");
                }
            }
            
            Debug.Log($"Setup {testEnemies.Length} test enemies");
        }
        
        [ContextMenu("Debug Chain Range")]
        public void DebugChainRange()
        {
            if (testTower == null || testEnemies == null || testEnemies.Length == 0)
            {
                Debug.LogWarning("Need tower and enemies to debug chain range!");
                return;
            }
            
            var traitManager = testTower.GetComponent<TowerTraitManager>();
            if (traitManager == null) return;
            
            foreach (var trait in traitManager.AppliedTraits)
            {
                if (trait.hasChainEffect)
                {
                    Debug.Log($"Chain trait '{trait.traitName}': Range={trait.chainRange}, Targets={trait.chainTargets}");
                    
                    // Test from first enemy
                    if (testEnemies[0] != null)
                    {
                        Vector3 centerPos = testEnemies[0].transform.position;
                        Debug.Log($"Testing chain from {testEnemies[0].name} at {centerPos}");
                        
                        // Find all colliders in range
                        Collider2D[] colliders = Physics2D.OverlapCircleAll(centerPos, trait.chainRange);
                        Debug.Log($"Found {colliders.Length} colliders in chain range");
                        
                        foreach (var col in colliders)
                        {
                            Enemy enemy = col.GetComponent<Enemy>();
                            if (enemy != null && enemy != testEnemies[0])
                            {
                                float distance = Vector3.Distance(centerPos, enemy.transform.position);
                                Debug.Log($"  -> {enemy.name} at distance {distance:F2}");
                            }
                        }
                    }
                }
            }
        }
        
        void OnDrawGizmos()
        {
            if (testTower == null) return;
            
            var traitManager = testTower.GetComponent<TowerTraitManager>();
            if (traitManager == null) return;
            
            // Draw chain ranges for lightning traits
            foreach (var trait in traitManager.AppliedTraits)
            {
                if (trait.hasChainEffect)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(testTower.transform.position, trait.chainRange);
                }
            }
            
            // Draw connections between test enemies
            if (testEnemies != null && testEnemies.Length > 1)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < testEnemies.Length - 1; i++)
                {
                    if (testEnemies[i] != null && testEnemies[i + 1] != null)
                    {
                        Gizmos.DrawLine(testEnemies[i].transform.position, testEnemies[i + 1].transform.position);
                    }
                }
            }
        }
    }
}