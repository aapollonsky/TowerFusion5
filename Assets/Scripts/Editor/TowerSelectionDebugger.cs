using UnityEngine;
using UnityEditor;
using TowerFusion;

namespace TowerFusion.Editor
{
    /// <summary>
    /// Simple tool to test tower selection and debug tower manager state
    /// </summary>
    public class TowerSelectionDebugger : EditorWindow
    {
        [MenuItem("TowerFusion/Debug/Tower Selection Debugger")]
        public static void ShowWindow()
        {
            GetWindow<TowerSelectionDebugger>("Tower Selection Debug");
        }
        
        void OnGUI()
        {
            GUILayout.Label("Tower Selection Debug", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            // Show current state
            if (TowerManager.Instance != null)
            {
                EditorGUILayout.LabelField("TowerManager:", "Found ✓");
                
                Tower selected = TowerManager.Instance.GetSelectedTower();
                EditorGUILayout.LabelField("Selected Tower:", selected != null ? selected.name : "None");
                
                var towers = TowerManager.Instance.GetActiveTowers();
                EditorGUILayout.LabelField("Active Towers:", towers.Count.ToString());
                
                if (towers.Count > 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Available Towers:", EditorStyles.miniBoldLabel);
                    
                    foreach (var tower in towers)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"  - {tower.name}");
                        
                        if (GUILayout.Button("Select", GUILayout.Width(60)))
                        {
                            TowerManager.Instance.SelectTower(tower);
                            Debug.Log($"Selected tower: {tower.name}");
                        }
                        
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("TowerManager:", "Missing ✗", EditorStyles.boldLabel);
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Refresh"))
            {
                Repaint();
            }
            
            if (GUILayout.Button("Find All Towers in Scene"))
            {
                var allTowers = FindObjectsOfType<Tower>();
                Debug.Log($"=== All Towers in Scene ({allTowers.Length}) ===");
                foreach (var tower in allTowers)
                {
                    Debug.Log($"Tower: {tower.name} at position {tower.transform.position}");
                }
            }
            
            if (GUILayout.Button("Test Manual Selection"))
            {
                var towers = FindObjectsOfType<Tower>();
                if (towers.Length > 0)
                {
                    if (TowerManager.Instance != null)
                    {
                        TowerManager.Instance.SelectTower(towers[0]);
                        Debug.Log($"Manually selected: {towers[0].name}");
                    }
                    else
                    {
                        Debug.LogError("TowerManager.Instance is null!");
                    }
                }
                else
                {
                    Debug.LogWarning("No towers found in scene!");
                }
            }
        }
    }
}