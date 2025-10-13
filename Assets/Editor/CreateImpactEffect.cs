using UnityEngine;
using UnityEditor;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Editor tool to create simple impact effect prefabs
    /// </summary>
    public class CreateImpactEffect : EditorWindow
    {
        [MenuItem("Tower Fusion/Create Impact Effect")]
        public static void CreateBasicImpact()
        {
            // Ensure directory exists
            string path = "Assets/Prefab/Effects";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/Prefab", "Effects");
            }
            
            // Create impact effect GameObject
            GameObject impactEffect = new GameObject("BasicImpactEffect");
            
            // Add ParticleSystem
            ParticleSystem ps = impactEffect.AddComponent<ParticleSystem>();
            
            // Configure main module
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
            main.startColor = new Color(1f, 0.9f, 0.4f); // Bright yellow
            main.maxParticles = 25;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.stopAction = ParticleSystemStopAction.Destroy;
            main.playOnAwake = true;
            main.loop = false;
            
            // Configure emission - burst only
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 20)
            });
            
            // Configure shape - sphere
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;
            
            // Configure size over lifetime - shrink
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
            
            // Configure color over lifetime - fade out
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(Color.white, 0f), 
                    new GradientColorKey(new Color(1f, 0.5f, 0f), 0.5f),
                    new GradientColorKey(Color.gray, 1f) 
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1f, 0f), 
                    new GradientAlphaKey(0f, 1f) 
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
            
            // Configure renderer
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingLayerName = "Default"; // Use Default layer to ensure visibility
            renderer.sortingOrder = 100; // High order to appear on top
            renderer.material = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
            
            // Add auto-destroy component
            var autoDestroy = impactEffect.AddComponent<TowerFusion.AutoDestroyParticleSystem>();
            
            // Save as prefab
            string prefabPath = $"{path}/BasicImpactEffect.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(impactEffect, prefabPath);
            
            // Clean up scene object
            DestroyImmediate(impactEffect);
            
            // Select and highlight
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
            
            Debug.Log($"Created Basic Impact Effect at: {prefabPath}");
            
            // Now assign it to projectiles
            AssignImpactEffectToProjectiles(prefab);
        }
        
        private static void AssignImpactEffectToProjectiles(GameObject impactEffect)
        {
            // Find projectile prefabs directly in Assets/Prefab
            string[] projectilePaths = new string[] {
                "Assets/Prefab/Projectile.prefab",
                "Assets/Prefab/AdvancedProjectile.prefab"
            };
            
            int assignedCount = 0;
            
            foreach (string path in projectilePaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                
                if (prefab != null)
                {
                    Projectile projectile = prefab.GetComponent<Projectile>();
                    if (projectile != null)
                    {
                        // Use SerializedObject to modify the prefab
                        SerializedObject so = new SerializedObject(projectile);
                        SerializedProperty impactProp = so.FindProperty("impactEffectPrefab");
                        
                        if (impactProp != null)
                        {
                            impactProp.objectReferenceValue = impactEffect;
                            so.ApplyModifiedProperties();
                            assignedCount++;
                            
                            Debug.Log($"Assigned impact effect to: {prefab.name}");
                        }
                    }
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Assigned impact effect to {assignedCount} projectile prefabs");
        }
    }
}
