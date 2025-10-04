using UnityEngine;
using UnityEditor;
using TowerFusion;
using System.Linq;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Custom inspector for Tower component with trait management
    /// </summary>
    [CustomEditor(typeof(Tower))]
    public class TowerEditor : UnityEditor.Editor
    {
        private Tower tower;
        private TowerTrait[] availableTraits;
        
        private void OnEnable()
        {
            tower = target as Tower;
            LoadAvailableTraits();
        }
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (tower == null) return;
            
            EditorGUILayout.Space(10);
            DrawTraitSection();
        }
        
        private void DrawTraitSection()
        {
            EditorGUILayout.LabelField("Tower Traits", EditorStyles.boldLabel);
            
            // Show current stats
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Current Stats:", EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField($"Damage: {tower.ModifiedDamage:F1}");
                EditorGUILayout.LabelField($"Range: {tower.ModifiedRange:F1}");
                EditorGUILayout.LabelField($"Attack Speed: {tower.TowerData.attackSpeed:F1}/s");
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }
            
            // Show applied traits
            if (tower.TraitManager != null)
            {
                var appliedTraits = tower.GetAppliedTraits();
                
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"Applied Traits ({appliedTraits.Count}):", EditorStyles.miniBoldLabel);
                
                if (appliedTraits.Count == 0)
                {
                    EditorGUILayout.LabelField("No traits applied", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    foreach (var trait in appliedTraits)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        // Trait info
                        EditorGUILayout.LabelField($"• {trait.traitName}", EditorStyles.label);
                        
                        // Remove button
                        if (GUILayout.Button("Remove", GUILayout.Width(60)))
                        {
                            tower.RemoveTrait(trait);
                        }
                        
                        EditorGUILayout.EndHorizontal();
                        
                        // Description
                        EditorGUILayout.LabelField($"    {trait.description}", EditorStyles.miniLabel);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.Space(5);
            
            // Add trait section
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Add Traits:", EditorStyles.miniBoldLabel);
            
            if (availableTraits == null || availableTraits.Length == 0)
            {
                EditorGUILayout.LabelField("No traits available", EditorStyles.centeredGreyMiniLabel);
                if (GUILayout.Button("Create Default Traits"))
                {
                    TowerTraitFactory.CreateDefaultTraits();
                    LoadAvailableTraits();
                }
            }
            else
            {
                // Group traits by category
                var traitsByCategory = availableTraits.GroupBy(t => t.category).ToList();
                
                foreach (var categoryGroup in traitsByCategory)
                {
                    EditorGUILayout.LabelField($"{categoryGroup.Key}:", EditorStyles.boldLabel);
                    
                    foreach (var trait in categoryGroup)
                    {
                        EditorGUILayout.BeginHorizontal();
                        
                        // Trait name and description
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField(trait.traitName, EditorStyles.label);
                        EditorGUILayout.LabelField(trait.description, EditorStyles.miniLabel);
                        EditorGUILayout.EndVertical();
                        
                        // Add button
                        GUI.enabled = !tower.HasTrait(trait);
                        if (GUILayout.Button("Add", GUILayout.Width(60)))
                        {
                            tower.AddTrait(trait);
                        }
                        GUI.enabled = true;
                        
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.Space(2);
                    }
                    
                    EditorGUILayout.Space(5);
                }
            }
            
            EditorGUILayout.EndVertical();
            
            // Utility buttons
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Clear All Traits"))
            {
                if (EditorUtility.DisplayDialog("Clear All Traits", 
                    "Are you sure you want to remove all traits from this tower?", 
                    "Yes", "No"))
                {
                    tower.ClearAllTraits();
                }
            }
            
            if (GUILayout.Button("Refresh Traits List"))
            {
                LoadAvailableTraits();
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void LoadAvailableTraits()
        {
            // Find all TowerTrait assets in the project
            string[] guids = AssetDatabase.FindAssets("t:TowerTrait");
            availableTraits = new TowerTrait[guids.Length];
            
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                availableTraits[i] = AssetDatabase.LoadAssetAtPath<TowerTrait>(path);
            }
        }
    }
}