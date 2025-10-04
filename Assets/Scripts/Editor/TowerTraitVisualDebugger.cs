using UnityEngine;
using UnityEditor;
using TowerFusion;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Debug utilities for tower trait visual effects
    /// </summary>
    public static class TowerTraitVisualDebugger
    {
        [MenuItem("Tools/Tower Fusion/Debug: Show Tower Visual Components")]
        public static void ShowTowerVisualComponents()
        {
            Tower selectedTower = Selection.activeGameObject?.GetComponent<Tower>();
            if (selectedTower == null)
            {
                Debug.LogWarning("Please select a tower GameObject to inspect visuals");
                return;
            }
            
            Debug.Log($"=== Tower Visual Components: {selectedTower.name} ===");
            
            // Check main components
            SpriteRenderer mainRenderer = selectedTower.GetComponent<SpriteRenderer>();
            Debug.Log($"Main SpriteRenderer: {(mainRenderer != null ? "Found" : "Missing")}");
            if (mainRenderer != null)
            {
                Debug.Log($"  - Sprite: {(mainRenderer.sprite != null ? mainRenderer.sprite.name : "None")}");
                Debug.Log($"  - Color: {mainRenderer.color}");
                Debug.Log($"  - Sorting Order: {mainRenderer.sortingOrder}");
            }
            
            // Check trait manager
            TowerTraitManager traitManager = selectedTower.GetComponent<TowerTraitManager>();
            Debug.Log($"TowerTraitManager: {(traitManager != null ? "Found" : "Missing")}");
            
            if (traitManager != null)
            {
                var traits = selectedTower.GetAppliedTraits();
                Debug.Log($"  - Applied Traits: {traits.Count}");
                foreach (var trait in traits)
                {
                    Debug.Log($"    • {trait.traitName}: Color={trait.overlayColor}, Alpha={trait.overlayAlpha}");
                }
                
                // Check child objects for overlays and effects
                Transform[] children = selectedTower.GetComponentsInChildren<Transform>();
                Debug.Log($"  - Child Objects: {children.Length - 1}"); // -1 to exclude self
                
                foreach (Transform child in children)
                {
                    if (child == selectedTower.transform) continue;
                    
                    Debug.Log($"    • {child.name}:");
                    SpriteRenderer childRenderer = child.GetComponent<SpriteRenderer>();
                    if (childRenderer != null)
                    {
                        Debug.Log($"      - SpriteRenderer: Color={childRenderer.color}, Sprite={childRenderer.sprite?.name ?? "None"}");
                    }
                    ParticleSystem particles = child.GetComponent<ParticleSystem>();
                    if (particles != null)
                    {
                        Debug.Log($"      - ParticleSystem: Playing={particles.isPlaying}");
                    }
                }
            }
        }
        
        [MenuItem("Tools/Tower Fusion/Debug: Force Refresh Tower Visuals")]
        public static void ForceRefreshTowerVisuals()
        {
            Tower selectedTower = Selection.activeGameObject?.GetComponent<Tower>();
            if (selectedTower == null)
            {
                Debug.LogWarning("Please select a tower GameObject to refresh visuals");
                return;
            }
            
            TowerTraitManager traitManager = selectedTower.GetComponent<TowerTraitManager>();
            if (traitManager == null)
            {
                Debug.LogWarning("Selected tower has no TowerTraitManager component");
                return;
            }
            
            Debug.Log($"Forcing visual refresh for {selectedTower.name}...");
            
            // Use reflection to call private RefreshAllVisuals method
            var method = typeof(TowerTraitManager).GetMethod("RefreshAllVisuals", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (method != null)
            {
                method.Invoke(traitManager, null);
                Debug.Log("Visual refresh completed!");
            }
            else
            {
                Debug.LogError("Could not find RefreshAllVisuals method");
            }
        }
        
        [MenuItem("Tools/Tower Fusion/Debug: Test Tower Color Tint")]
        public static void TestTowerColorTint()
        {
            Tower selectedTower = Selection.activeGameObject?.GetComponent<Tower>();
            if (selectedTower == null)
            {
                Debug.LogWarning("Please select a tower GameObject to test color tint");
                return;
            }
            
            SpriteRenderer renderer = selectedTower.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                Debug.LogWarning("Selected tower has no SpriteRenderer component");
                return;
            }
            
            Debug.Log($"Testing color tint on {selectedTower.name}...");
            
            // Test different colors
            Color[] testColors = { Color.red, Color.blue, Color.green, Color.cyan, Color.yellow, Color.white };
            int colorIndex = 0;
            
            System.Action changeColor = null;
            changeColor = () => {
                if (renderer != null && colorIndex < testColors.Length)
                {
                    renderer.color = testColors[colorIndex];
                    Debug.Log($"Applied color: {testColors[colorIndex]}");
                    colorIndex++;
                    
                    if (colorIndex < testColors.Length)
                    {
                        EditorApplication.delayCall += () => {
                            System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ => changeColor());
                        };
                    }
                    else
                    {
                        Debug.Log("Color test completed");
                    }
                }
            };
            
            changeColor();
        }
        
        [MenuItem("Tools/Tower Fusion/Debug: Add Ice Trait with Enhanced Visuals")]
        public static void AddIceTraitWithEnhancedVisuals()
        {
            Tower selectedTower = Selection.activeGameObject?.GetComponent<Tower>();
            if (selectedTower == null)
            {
                Debug.LogWarning("Please select a tower GameObject to add Ice trait");
                return;
            }
            
            // Find Ice trait asset
            TowerTrait iceTrait = AssetDatabase.LoadAssetAtPath<TowerTrait>("Assets/Data/Traits/Ice.asset");
            if (iceTrait == null)
            {
                Debug.LogWarning("Ice trait not found. Please create traits using 'Tools/Tower Fusion/Create Default Traits' first");
                return;
            }
            
            // Enhance ice trait visuals
            iceTrait.overlayColor = Color.cyan;
            iceTrait.overlayAlpha = 0.6f;
            
            Debug.Log($"Adding Ice trait with enhanced visuals to {selectedTower.name}...");
            
            if (selectedTower.AddTrait(iceTrait))
            {
                Debug.Log("Ice trait added successfully!");
                
                // Force visual refresh after a short delay
                EditorApplication.delayCall += () => {
                    ForceRefreshTowerVisuals();
                };
            }
            else
            {
                Debug.LogWarning("Failed to add Ice trait");
            }
        }
    }
}