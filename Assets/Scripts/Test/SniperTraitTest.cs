using UnityEngine;
using TowerFusion;

namespace TowerFusion
{
    /// <summary>
    /// Test component to verify Sniper trait functionality
    /// </summary>
    public class SniperTraitTest : MonoBehaviour
    {
        [Header("Test Configuration")]
        public Tower testTower;
        public Enemy[] testEnemies;
        
        [Header("Test Results")]
        [SerializeField] private bool sniperTraitApplied = false;
        [SerializeField] private float originalRange = 0f;
        [SerializeField] private float modifiedRange = 0f;
        [SerializeField] private float chargeTime = 0f;
        [SerializeField] private bool isCharging = false;
        [SerializeField] private float chargeProgress = 0f;
        
        void Start()
        {
            if (testTower == null)
                testTower = FindObjectOfType<Tower>();
                
            if (testEnemies == null || testEnemies.Length == 0)
                testEnemies = FindObjectsOfType<Enemy>();
                
            Debug.Log($"Sniper Trait Test initialized. Tower: {testTower?.name}, Enemies: {testEnemies?.Length}");
        }
        
        void Update()
        {
            if (testTower != null)
            {
                // Update display values
                originalRange = testTower.TowerData.attackRange;
                modifiedRange = testTower.ModifiedRange;
                chargeTime = testTower.ChargeTime;
                isCharging = testTower.IsCharging;
                chargeProgress = testTower.ChargeProgress;
                
                // Check if sniper trait is applied
                var traitManager = testTower.GetComponent<TowerTraitManager>();
                if (traitManager != null)
                {
                    sniperTraitApplied = false;
                    foreach (var trait in traitManager.AppliedTraits)
                    {
                        if (trait.traitName == "Sniper")
                        {
                            sniperTraitApplied = true;
                            break;
                        }
                    }
                }
            }
        }
        
        [ContextMenu("Apply Sniper Trait")]
        public void ApplySniperTrait()
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
            
            // Create sniper trait
            var sniperTrait = ScriptableObject.CreateInstance<TowerTrait>();
            sniperTrait.traitName = "Sniper";
            sniperTrait.description = "+100% range, +2 second charge time";
            sniperTrait.category = TraitCategory.Range;
            sniperTrait.overlayColor = Color.green;
            sniperTrait.rangeMultiplier = 2f; // +100% range
            sniperTrait.chargeTimeBonus = 2f; // +2 second charge time
            
            bool added = traitManager.AddTrait(sniperTrait);
            Debug.Log($"Sniper trait added: {added}");
            
            // Log the changes
            Debug.Log($"Original range: {testTower.TowerData.attackRange}");
            Debug.Log($"Modified range: {testTower.ModifiedRange}");
            Debug.Log($"Charge time: {testTower.ChargeTime}");
            
            // Force show range indicator to visualize the change
            testTower.SetRangeIndicatorVisible(true);
        }
        
        [ContextMenu("Test Sniper Attack")]
        public void TestSniperAttack()
        {
            if (testTower == null)
            {
                Debug.LogWarning("No test tower assigned!");
                return;
            }
            
            if (testEnemies == null || testEnemies.Length == 0)
            {
                Debug.LogWarning("No test enemies found!");
                return;
            }
            
            // Position an enemy within the extended range but outside original range
            var targetEnemy = testEnemies[0];
            if (targetEnemy != null)
            {
                Vector3 towerPos = testTower.transform.position;
                float originalRange = testTower.TowerData.attackRange;
                float extendedDistance = originalRange * 1.5f; // Between original and extended range
                
                Vector3 enemyPos = towerPos + Vector3.right * extendedDistance;
                targetEnemy.transform.position = enemyPos;
                
                Debug.Log($"Positioned enemy at distance {extendedDistance} (original range: {originalRange}, extended range: {testTower.ModifiedRange})");
                Debug.Log($"Tower should start charging when enemy enters range");
            }
        }
        
        [ContextMenu("Setup Long Range Target")]
        public void SetupLongRangeTarget()
        {
            if (testTower == null || testEnemies == null || testEnemies.Length == 0)
            {
                Debug.LogWarning("Need tower and enemies to setup long range target!");
                return;
            }
            
            var targetEnemy = testEnemies[0];
            if (targetEnemy != null)
            {
                Vector3 towerPos = testTower.transform.position;
                float distance = testTower.ModifiedRange * 0.9f; // Just within extended range
                
                Vector3 enemyPos = towerPos + Vector3.right * distance;
                targetEnemy.transform.position = enemyPos;
                
                Debug.Log($"Enemy positioned at distance {distance} (tower range: {testTower.ModifiedRange})");
                Debug.Log($"Watch for charging behavior: charge time = {testTower.ChargeTime}s");
            }
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
        
        [ContextMenu("Toggle Range Indicator")]
        public void ToggleRangeIndicator()
        {
            if (testTower == null)
            {
                Debug.LogWarning("No test tower assigned!");
                return;
            }
            
            // Toggle the range indicator visibility
            testTower.SetRangeIndicatorVisible(!testTower.gameObject.activeInHierarchy);
            Debug.Log("Toggled range indicator visibility");
        }
        
        [ContextMenu("Show Range Indicator")]
        public void ShowRangeIndicator()
        {
            if (testTower == null)
            {
                Debug.LogWarning("No test tower assigned!");
                return;
            }
            
            testTower.SetRangeIndicatorVisible(true);
            Debug.Log($"Range indicator shown - Original: {testTower.TowerData.attackRange}, Modified: {testTower.ModifiedRange}");
        }
        
        [ContextMenu("Debug Range Calculation")]
        public void DebugRangeCalculation()
        {
            if (testTower == null)
            {
                Debug.LogWarning("No test tower assigned!");
                return;
            }
            
            var traitManager = testTower.GetComponent<TowerTraitManager>();
            if (traitManager == null)
            {
                Debug.LogWarning("No TowerTraitManager found!");
                return;
            }
            
            Debug.Log("=== RANGE DEBUG INFO ===");
            Debug.Log($"Tower: {testTower.name}");
            Debug.Log($"Base Range: {testTower.TowerData.attackRange}");
            Debug.Log($"Modified Range: {testTower.ModifiedRange}");
            Debug.Log($"Applied Traits Count: {traitManager.AppliedTraits.Count}");
            
            foreach (var trait in traitManager.AppliedTraits)
            {
                Debug.Log($"Trait: {trait.traitName}");
                Debug.Log($"  - Range Multiplier: {trait.rangeMultiplier}");
                Debug.Log($"  - Range Bonus: {trait.rangeBonus}");
                Debug.Log($"  - Category: {trait.category}");
            }
            
            // Force recalculate
            testTower.OnTraitsChanged();
            Debug.Log($"After forced recalculation - Modified Range: {testTower.ModifiedRange}");
        }
        
        void OnDrawGizmos()
        {
            if (testTower == null) return;
            
            // Draw original range in red
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(testTower.transform.position, testTower.TowerData.attackRange);
            
            // Draw modified range in green if different
            if (Mathf.Abs(testTower.ModifiedRange - testTower.TowerData.attackRange) > 0.1f)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(testTower.transform.position, testTower.ModifiedRange);
            }
            
            // Draw charge progress indicator
            if (testTower.IsCharging)
            {
                Gizmos.color = Color.yellow;
                Vector3 chargeBarPos = testTower.transform.position + Vector3.up * 2f;
                float barWidth = 2f;
                float progress = testTower.ChargeProgress;
                
                // Background bar
                Gizmos.color = Color.gray;
                Gizmos.DrawLine(chargeBarPos - Vector3.right * barWidth * 0.5f, 
                              chargeBarPos + Vector3.right * barWidth * 0.5f);
                
                // Progress bar
                Gizmos.color = Color.yellow;
                Vector3 progressEnd = chargeBarPos - Vector3.right * barWidth * 0.5f + Vector3.right * barWidth * progress;
                Gizmos.DrawLine(chargeBarPos - Vector3.right * barWidth * 0.5f, progressEnd);
            }
        }
    }
}