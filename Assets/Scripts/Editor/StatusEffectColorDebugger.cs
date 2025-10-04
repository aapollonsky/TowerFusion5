using UnityEngine;
using UnityEditor;
using TowerFusion;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Debug utilities for status effect color testing
    /// </summary>
    public static class StatusEffectColorDebugger
    {
        [MenuItem("Tools/Tower Fusion/Debug: Test Status Effect Colors")]
        public static void TestStatusEffectColors()
        {
            Enemy[] enemies = Object.FindObjectsOfType<Enemy>();
            
            if (enemies.Length == 0)
            {
                Debug.LogWarning("No enemies found in scene. Please start a wave first.");
                return;
            }
            
            Debug.Log($"=== Status Effect Color Test ({enemies.Length} enemies) ===");
            
            foreach (Enemy enemy in enemies)
            {
                Debug.Log($"\nEnemy: {enemy.name}");
                Debug.Log($"  Current Speed: {enemy.CurrentSpeed}");
                Debug.Log($"  Is Slowed: {enemy.IsSlowed}");
                Debug.Log($"  Is Brittle: {enemy.IsBrittle}");
                
                var spriteRenderer = enemy.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Debug.Log($"  Current Color: {spriteRenderer.color}");
                }
            }
        }
        
        [MenuItem("Tools/Tower Fusion/Debug: Apply Test Status Effects")]
        public static void ApplyTestStatusEffects()
        {
            Enemy testEnemy = Object.FindObjectOfType<Enemy>();
            
            if (testEnemy == null)
            {
                Debug.LogWarning("No enemy found in scene. Please start a wave first.");
                return;
            }
            
            Debug.Log($"=== Applying Test Status Effects to {testEnemy.name} ===");
            
            // Test slow effect
            Debug.Log("Applying slow effect (3 seconds)...");
            testEnemy.ApplySlowEffect(0.5f, 3f);
            
            // Test brittle effect (delayed)
            EditorApplication.delayCall += () => {
                System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ => {
                    UnityEngine.Debug.Log("Applying brittle effect (3 seconds)...");
                    testEnemy.ApplyBrittleEffect(1.5f, 3f);
                });
            };
        }
        
        [MenuItem("Tools/Tower Fusion/Debug: Reset Enemy Colors")]
        public static void ResetEnemyColors()
        {
            Enemy[] enemies = Object.FindObjectsOfType<Enemy>();
            
            foreach (Enemy enemy in enemies)
            {
                var spriteRenderer = enemy.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                    Debug.Log($"Reset color for {enemy.name}");
                }
            }
            
            Debug.Log($"Reset colors for {enemies.Length} enemies");
        }
        
        [MenuItem("Tools/Tower Fusion/Debug: Show Enemy Original Colors")]
        public static void ShowEnemyOriginalColors()
        {
            Enemy[] enemies = Object.FindObjectsOfType<Enemy>();
            
            if (enemies.Length == 0)
            {
                Debug.LogWarning("No enemies found in scene.");
                return;
            }
            
            Debug.Log($"=== Enemy Original Colors ({enemies.Length} enemies) ===");
            
            foreach (Enemy enemy in enemies)
            {
                var spriteRenderer = enemy.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Debug.Log($"{enemy.name}: Current={spriteRenderer.color}");
                    
                    // Use reflection to access the original color field
                    var field = typeof(Enemy).GetField("originalColor", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        Color originalColor = (Color)field.GetValue(enemy);
                        Debug.Log($"  Original stored: {originalColor}");
                    }
                }
            }
        }
    }
}