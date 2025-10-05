using UnityEngine;
using UnityEditor;
using TowerFusion;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Editor utility to setup default traits for the trait system
    /// </summary>
    public static class TraitSystemSetup
    {
        [MenuItem("Tools/Tower Fusion/Setup Trait System")]
        public static void SetupTraitSystem()
        {
            // Ensure traits exist
            TowerTraitFactory.CreateDefaultTraits();
            
            Debug.Log("Trait system setup complete!");
            Debug.Log("Instructions:");
            Debug.Log("1. In GameUI component, assign the 5 trait assets to the 'Available Traits' array:");
            Debug.Log("   - Fire (Assets/Data/Traits/Fire.asset)");
            Debug.Log("   - Ice (Assets/Data/Traits/Ice.asset)");
            Debug.Log("   - Lightning (Assets/Data/Traits/Lightning.asset)");
            Debug.Log("   - Sniper (Assets/Data/Traits/Sniper.asset)");
            Debug.Log("   - Harvest (Assets/Data/Traits/Harvest.asset)");
            Debug.Log("2. Create UI elements for trait button and dialog in your scene");
            Debug.Log("3. Assign the UI references in the GameUI inspector");
        }
        
        [MenuItem("Tools/Tower Fusion/Test Trait Probabilities")]
        public static void TestTraitProbabilities()
        {
            float[] probabilities = {0.2f, 0.2f, 0.2f, 0.2f, 0.2f};
            string[] traitNames = {"Fire", "Ice", "Lightning", "Sniper", "Harvest"};
            int[] counts = new int[5];
            int testRuns = 1000;
            
            for (int i = 0; i < testRuns; i++)
            {
                int selectedIndex = GenerateRandomTraitIndex(probabilities);
                if (selectedIndex >= 0 && selectedIndex < counts.Length)
                {
                    counts[selectedIndex]++;
                }
            }
            
            Debug.Log($"Trait probability test results ({testRuns} runs):");
            for (int i = 0; i < traitNames.Length; i++)
            {
                float percentage = (counts[i] / (float)testRuns) * 100f;
                Debug.Log($"{traitNames[i]}: {counts[i]} times ({percentage:F1}%, expected: 20.0%)");
            }
        }
        
        private static int GenerateRandomTraitIndex(float[] probabilities)
        {
            float totalProbability = 0f;
            for (int i = 0; i < probabilities.Length; i++)
            {
                totalProbability += probabilities[i];
            }
            
            float randomValue = Random.Range(0f, totalProbability);
            
            float cumulativeProbability = 0f;
            for (int i = 0; i < probabilities.Length; i++)
            {
                cumulativeProbability += probabilities[i];
                if (randomValue <= cumulativeProbability)
                {
                    return i;
                }
            }
            
            return 0; // Fallback
        }
    }
}