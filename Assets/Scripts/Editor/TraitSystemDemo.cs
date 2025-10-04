using UnityEngine;
using UnityEditor;
using TowerFusion;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Editor utility to demonstrate tower trait system usage
    /// </summary>
    public static class TraitSystemDemo
    {
        [MenuItem("Tools/Tower Fusion/Demo: Add Fire Trait to Selected Tower")]
        public static void AddFireTraitToSelectedTower()
        {
            Tower selectedTower = Selection.activeGameObject?.GetComponent<Tower>();
            if (selectedTower == null)
            {
                Debug.LogWarning("Please select a tower GameObject to add Fire trait");
                return;
            }
            
            // Find Fire trait asset
            TowerTrait fireTrait = AssetDatabase.LoadAssetAtPath<TowerTrait>("Assets/Data/Traits/Fire.asset");
            if (fireTrait == null)
            {
                Debug.LogWarning("Fire trait not found. Please create traits using 'Tools/Tower Fusion/Create Default Traits' first");
                return;
            }
            
            if (selectedTower.AddTrait(fireTrait))
            {
                Debug.Log($"Added Fire trait to {selectedTower.name}");
            }
            else
            {
                Debug.LogWarning($"Failed to add Fire trait to {selectedTower.name}");
            }
        }
        
        [MenuItem("Tools/Tower Fusion/Demo: Add Lightning Trait to Selected Tower")]
        public static void AddLightningTraitToSelectedTower()
        {
            Tower selectedTower = Selection.activeGameObject?.GetComponent<Tower>();
            if (selectedTower == null)
            {
                Debug.LogWarning("Please select a tower GameObject to add Lightning trait");
                return;
            }
            
            // Find Lightning trait asset
            TowerTrait lightningTrait = AssetDatabase.LoadAssetAtPath<TowerTrait>("Assets/Data/Traits/Lightning.asset");
            if (lightningTrait == null)
            {
                Debug.LogWarning("Lightning trait not found. Please create traits using 'Tools/Tower Fusion/Create Default Traits' first");
                return;
            }
            
            if (selectedTower.AddTrait(lightningTrait))
            {
                Debug.Log($"Added Lightning trait to {selectedTower.name}");
            }
            else
            {
                Debug.LogWarning($"Failed to add Lightning trait to {selectedTower.name}");
            }
        }
        
        [MenuItem("Tools/Tower Fusion/Demo: Clear All Traits from Selected Tower")]
        public static void ClearTraitsFromSelectedTower()
        {
            Tower selectedTower = Selection.activeGameObject?.GetComponent<Tower>();
            if (selectedTower == null)
            {
                Debug.LogWarning("Please select a tower GameObject to clear traits");
                return;
            }
            
            selectedTower.ClearAllTraits();
            Debug.Log($"Cleared all traits from {selectedTower.name}");
        }
        
        [MenuItem("Tools/Tower Fusion/Demo: Show Selected Tower Traits")]
        public static void ShowSelectedTowerTraits()
        {
            Tower selectedTower = Selection.activeGameObject?.GetComponent<Tower>();
            if (selectedTower == null)
            {
                Debug.LogWarning("Please select a tower GameObject to show traits");
                return;
            }
            
            var traits = selectedTower.GetAppliedTraits();
            if (traits.Count == 0)
            {
                Debug.Log($"{selectedTower.name} has no traits applied");
            }
            else
            {
                Debug.Log($"{selectedTower.name} has {traits.Count} traits:");
                foreach (var trait in traits)
                {
                    Debug.Log($"  - {trait.traitName}: {trait.description}");
                }
            }
        }
    }
}