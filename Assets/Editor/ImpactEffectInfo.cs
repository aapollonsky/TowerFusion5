using UnityEngine;
using UnityEditor;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Quick info window about impact effects
    /// </summary>
    public class ImpactEffectInfo : EditorWindow
    {
        [MenuItem("Tower Fusion/Impact Effect Info")]
        public static void ShowWindow()
        {
            var window = GetWindow<ImpactEffectInfo>("Impact Effects");
            window.minSize = new Vector2(400, 300);
        }

        private void OnGUI()
        {
            GUILayout.Label("Impact Effect Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox(
                "Impact effects are visual particles that appear when projectiles hit enemies.",
                MessageType.Info
            );
            
            EditorGUILayout.Space();
            
            GUILayout.Label("Quick Setup:", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Click 'Tower Fusion > Create Impact Effect' in the menu\n" +
                "2. The tool will automatically create and assign effects\n" +
                "3. Run the game and test!",
                MessageType.None
            );
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Create Impact Effect Now", GUILayout.Height(40)))
            {
                CreateImpactEffect.CreateBasicImpact();
                Close();
            }
            
            EditorGUILayout.Space();
            
            GUILayout.Label("Current Status:", EditorStyles.boldLabel);
            
            // Check if effects exist
            bool effectExists = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Effects/BasicImpactEffect.prefab") != null;
            
            if (effectExists)
            {
                EditorGUILayout.HelpBox("✓ BasicImpactEffect.prefab exists", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("✗ Impact effect not created yet", MessageType.Warning);
            }
            
            // Check projectile assignments
            var projectile = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/Projectile.prefab");
            var advProjectile = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/AdvancedProjectile.prefab");
            
            bool projectileHasEffect = false;
            bool advProjectileHasEffect = false;
            
            if (projectile != null)
            {
                var proj = projectile.GetComponent<Projectile>();
                if (proj != null)
                {
                    SerializedObject so = new SerializedObject(proj);
                    var impactProp = so.FindProperty("impactEffectPrefab");
                    projectileHasEffect = impactProp?.objectReferenceValue != null;
                }
            }
            
            if (advProjectile != null)
            {
                var proj = advProjectile.GetComponent<Projectile>();
                if (proj != null)
                {
                    SerializedObject so = new SerializedObject(proj);
                    var impactProp = so.FindProperty("impactEffectPrefab");
                    advProjectileHasEffect = impactProp?.objectReferenceValue != null;
                }
            }
            
            if (projectileHasEffect)
                EditorGUILayout.HelpBox("✓ Projectile.prefab has impact effect", MessageType.Info);
            else
                EditorGUILayout.HelpBox("✗ Projectile.prefab needs impact effect", MessageType.Warning);
            
            if (advProjectileHasEffect)
                EditorGUILayout.HelpBox("✓ AdvancedProjectile.prefab has impact effect", MessageType.Info);
            else
                EditorGUILayout.HelpBox("✗ AdvancedProjectile.prefab needs impact effect", MessageType.Warning);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Read Full Instructions"))
            {
                var instructions = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/../IMPACT_EFFECTS.md");
                if (instructions != null)
                {
                    Selection.activeObject = instructions;
                    EditorGUIUtility.PingObject(instructions);
                }
            }
        }
    }
}
