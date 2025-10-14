using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace TowerFusion
{
    /// <summary>
    /// Procedurally generates balanced wave configurations based on difficulty settings
    /// </summary>
    public class WaveGenerator : MonoBehaviour
    {
        [Header("Auto-Generation Settings")]
        [SerializeField] private bool useAutoGeneration = false;
        [Tooltip("1=Easy, 2=Medium, 3=Hard, 4=Very Hard, 5=Extreme")]
        [SerializeField] [Range(1, 5)] private int difficultyLevel = 2;
        [SerializeField] [Range(5, 100)] private int numberOfWaves = 20;
        
        [Header("Enemy Pool")]
        [SerializeField] private List<EnemyData> availableEnemies = new List<EnemyData>();
        
        [Header("Balance Parameters")]
        [Tooltip("How much enemy count increases per wave (%)")]
        [SerializeField] [Range(0f, 50f)] private float enemyCountGrowth = 10f;
        [Tooltip("How much enemy health increases per wave (%)")]
        [SerializeField] [Range(0f, 30f)] private float enemyHealthGrowth = 8f;
        [Tooltip("Base enemies in first wave")]
        [SerializeField] [Range(1, 20)] private int baseEnemyCount = 5;
        [Tooltip("Maximum enemies per wave")]
        [SerializeField] [Range(10, 200)] private int maxEnemiesPerWave = 50;
        
        // Cached generated waves
        private List<WaveData> generatedWaves = new List<WaveData>();
        private bool wavesGenerated = false;
        
        public bool UseAutoGeneration => useAutoGeneration;
        public int DifficultyLevel => difficultyLevel;
        public int NumberOfWaves => numberOfWaves;
        
        /// <summary>
        /// Generate all waves based on current settings
        /// </summary>
        public List<WaveData> GenerateWaves()
        {
            if (wavesGenerated && generatedWaves.Count == numberOfWaves)
            {
                return generatedWaves;
            }
            
            generatedWaves.Clear();
            
            if (availableEnemies == null || availableEnemies.Count == 0)
            {
                Debug.LogError("WaveGenerator: No enemies available for wave generation!");
                return generatedWaves;
            }
            
            Debug.Log($"<color=cyan>Generating {numberOfWaves} waves at difficulty level {difficultyLevel}...</color>");
            
            for (int i = 1; i <= numberOfWaves; i++)
            {
                WaveData wave = GenerateWave(i);
                if (wave != null)
                {
                    generatedWaves.Add(wave);
                }
            }
            
            wavesGenerated = true;
            Debug.Log($"<color=green>✓ Generated {generatedWaves.Count} balanced waves!</color>");
            
            return generatedWaves;
        }
        
        /// <summary>
        /// Generate a single wave
        /// </summary>
        private WaveData GenerateWave(int waveNumber)
        {
            // Create wave data (not a ScriptableObject, just runtime data)
            WaveData wave = ScriptableObject.CreateInstance<WaveData>();
            wave.waveName = $"Auto Wave {waveNumber}";
            wave.waveNumber = waveNumber;
            
            // Calculate wave difficulty progression
            float waveProgression = (float)(waveNumber - 1) / (numberOfWaves - 1); // 0 to 1
            float difficultyMultiplier = GetDifficultyMultiplier();
            
            // Calculate enemy count for this wave
            int enemyCount = CalculateEnemyCount(waveNumber, waveProgression, difficultyMultiplier);
            
            // Determine enemy composition based on wave progression
            List<EnemyGroup> groups = GenerateEnemyGroups(enemyCount, waveNumber, waveProgression);
            wave.enemyGroups = groups;
            
            // Set timing based on difficulty
            wave.timeBetweenEnemies = Mathf.Lerp(0.8f, 0.3f, difficultyMultiplier / 5f);
            wave.timeBetweenGroups = Mathf.Lerp(3f, 1f, difficultyMultiplier / 5f);
            
            Debug.Log($"  Wave {waveNumber}: {enemyCount} enemies in {groups.Count} groups");
            
            return wave;
        }
        
        /// <summary>
        /// Calculate number of enemies for a wave
        /// </summary>
        private int CalculateEnemyCount(int waveNumber, float progression, float difficultyMultiplier)
        {
            // Exponential growth with diminishing returns
            float growthFactor = 1f + (enemyCountGrowth / 100f);
            float count = baseEnemyCount * Mathf.Pow(growthFactor, waveNumber - 1);
            
            // Apply difficulty multiplier
            count *= difficultyMultiplier;
            
            // Add some randomness (±10%)
            count *= Random.Range(0.9f, 1.1f);
            
            // Clamp to reasonable values
            int finalCount = Mathf.RoundToInt(count);
            finalCount = Mathf.Clamp(finalCount, baseEnemyCount, maxEnemiesPerWave);
            
            return finalCount;
        }
        
        /// <summary>
        /// Generate enemy groups with balanced composition
        /// </summary>
        private List<EnemyGroup> GenerateEnemyGroups(int totalEnemies, int waveNumber, float progression)
        {
            List<EnemyGroup> groups = new List<EnemyGroup>();
            
            // Determine how many different enemy types to use based on progression
            int enemyTypeCount = GetEnemyTypeCount(waveNumber, progression);
            
            // Select enemy types based on wave progression
            List<EnemyData> selectedEnemies = SelectEnemiesForWave(enemyTypeCount, waveNumber, progression);
            
            if (selectedEnemies.Count == 0)
            {
                Debug.LogWarning($"No enemies selected for wave {waveNumber}");
                return groups;
            }
            
            // Distribute enemies across selected types
            int remainingEnemies = totalEnemies;
            
            // Early waves: mostly one type
            // Later waves: mixed groups
            if (progression < 0.3f)
            {
                // Simple waves - one or two groups
                int groupCount = selectedEnemies.Count == 1 ? 1 : 2;
                for (int i = 0; i < groupCount && remainingEnemies > 0; i++)
                {
                    int count = remainingEnemies / (groupCount - i);
                    EnemyGroup group = new EnemyGroup
                    {
                        enemyType = selectedEnemies[i % selectedEnemies.Count],
                        count = count,
                        delayBeforeGroup = i > 0 ? Random.Range(1f, 2f) : 0f
                    };
                    groups.Add(group);
                    remainingEnemies -= count;
                }
            }
            else
            {
                // Complex waves - multiple groups with varied composition
                int groupCount = Mathf.Min(selectedEnemies.Count * 2, 6);
                
                for (int i = 0; i < groupCount && remainingEnemies > 0; i++)
                {
                    // Vary group sizes - some small, some large
                    float groupSizeFactor = Random.Range(0.1f, 0.3f);
                    int count = Mathf.Max(1, Mathf.RoundToInt(remainingEnemies * groupSizeFactor));
                    count = Mathf.Min(count, remainingEnemies);
                    
                    EnemyGroup group = new EnemyGroup
                    {
                        enemyType = selectedEnemies[Random.Range(0, selectedEnemies.Count)],
                        count = count,
                        delayBeforeGroup = i > 0 ? Random.Range(0.5f, 2f) : 0f
                    };
                    groups.Add(group);
                    remainingEnemies -= count;
                }
            }
            
            // Add any remaining enemies to the last group
            if (remainingEnemies > 0 && groups.Count > 0)
            {
                groups[groups.Count - 1].count += remainingEnemies;
            }
            
            return groups;
        }
        
        /// <summary>
        /// Determine how many enemy types to use in a wave
        /// </summary>
        private int GetEnemyTypeCount(int waveNumber, float progression)
        {
            if (availableEnemies == null || availableEnemies.Count == 0)
                return 0;
            
            int maxTypes = Mathf.Min(availableEnemies.Count, 4);
            
            // Gradually introduce more enemy types
            if (progression < 0.2f)
                return Mathf.Max(1, Mathf.Min(1, maxTypes)); // Early: single type
            else if (progression < 0.4f)
                return Mathf.Max(1, Mathf.Min(2, maxTypes)); // Mid-early: two types
            else if (progression < 0.7f)
                return Mathf.Max(1, Mathf.Min(3, maxTypes)); // Mid-late: three types
            else
                return Mathf.Max(1, maxTypes); // Late: all available types
        }
        
        /// <summary>
        /// Select which enemy types to use for this wave
        /// </summary>
        private List<EnemyData> SelectEnemiesForWave(int count, int waveNumber, float progression)
        {
            List<EnemyData> selected = new List<EnemyData>();
            
            if (availableEnemies.Count == 0)
                return selected;
            
            // Sort enemies by difficulty (using health as proxy)
            var sortedEnemies = availableEnemies.OrderBy(e => e.maxHealth).ToList();
            
            // Early waves: easier enemies
            // Later waves: introduce harder enemies
            int maxDifficultyIndex = Mathf.RoundToInt(progression * (sortedEnemies.Count - 1));
            maxDifficultyIndex = Mathf.Clamp(maxDifficultyIndex, 0, sortedEnemies.Count - 1);
            
            // Select from available enemy pool
            List<EnemyData> candidatePool = sortedEnemies.Take(maxDifficultyIndex + 1).ToList();
            
            // Ensure at least one enemy is available
            if (candidatePool.Count == 0)
                candidatePool.Add(sortedEnemies[0]);
            
            // Randomly select from candidate pool
            count = Mathf.Min(count, candidatePool.Count);
            
            // Prefer mixing difficulties
            if (candidatePool.Count >= count && count > 0)
            {
                // Take from different difficulty tiers
                for (int i = 0; i < count; i++)
                {
                    int index;
                    if (count == 1)
                    {
                        // Single enemy - pick from middle of pool
                        index = candidatePool.Count / 2;
                    }
                    else
                    {
                        // Multiple enemies - spread across difficulty range
                        index = Mathf.RoundToInt((float)i / (count - 1) * (candidatePool.Count - 1));
                    }
                    
                    // Ensure index is valid
                    index = Mathf.Clamp(index, 0, candidatePool.Count - 1);
                    
                    if (!selected.Contains(candidatePool[index]))
                    {
                        selected.Add(candidatePool[index]);
                    }
                }
            }
            
            // Fill remaining slots randomly
            // If all unique enemies from pool are already selected, we can add duplicates
            int maxAttempts = count * 10; // Prevent infinite loop
            int attempts = 0;
            
            while (selected.Count < count && attempts < maxAttempts)
            {
                attempts++;
                EnemyData randomEnemy = candidatePool[Random.Range(0, candidatePool.Count)];
                
                // If we've selected all unique enemies, allow duplicates
                if (selected.Count >= candidatePool.Count || !selected.Contains(randomEnemy))
                {
                    selected.Add(randomEnemy);
                }
            }
            
            // Final safety: if still not enough enemies, just fill with first enemy
            while (selected.Count < count && candidatePool.Count > 0)
            {
                selected.Add(candidatePool[0]);
            }
            
            return selected;
        }
        
        /// <summary>
        /// Get difficulty multiplier based on difficulty level
        /// </summary>
        private float GetDifficultyMultiplier()
        {
            switch (difficultyLevel)
            {
                case 1: return 0.7f;  // Easy: 70% enemies
                case 2: return 1.0f;  // Medium: 100% enemies
                case 3: return 1.3f;  // Hard: 130% enemies
                case 4: return 1.6f;  // Very Hard: 160% enemies
                case 5: return 2.0f;  // Extreme: 200% enemies
                default: return 1.0f;
            }
        }
        
        /// <summary>
        /// Get a specific generated wave
        /// </summary>
        public WaveData GetGeneratedWave(int waveNumber)
        {
            if (!wavesGenerated || waveNumber < 1 || waveNumber > generatedWaves.Count)
                return null;
            
            return generatedWaves[waveNumber - 1];
        }
        
        /// <summary>
        /// Clear generated waves (force regeneration)
        /// </summary>
        public void ClearGeneratedWaves()
        {
            generatedWaves.Clear();
            wavesGenerated = false;
            Debug.Log("Cleared generated waves - will regenerate on next request");
        }
        
        /// <summary>
        /// Validate configuration
        /// </summary>
        private void OnValidate()
        {
            if (useAutoGeneration && (availableEnemies == null || availableEnemies.Count == 0))
            {
                Debug.LogWarning("WaveGenerator: Auto-generation enabled but no enemies assigned!");
            }
        }
    }
}
