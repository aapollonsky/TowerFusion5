using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TowerFusion.MapData))]
public class MapDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var map = target as TowerFusion.MapData;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("MapData Quick Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Add Tower Position at Scene View Pivot"))
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv == null)
            {
                EditorUtility.DisplayDialog("No Scene View", "Open a Scene View and position the camera where you want to add the tower position (Scene view pivot will be used).", "OK");
            }
            else
            {
                Undo.RecordObject(map, "Add Tower Position");
                map.towerPositions.Add(sv.pivot);
                EditorUtility.SetDirty(map);
                Debug.Log($"Added tower position at {sv.pivot}");
            }
        }

        if (GUILayout.Button("Add Tower Position from Selected Transform"))
        {
            if (Selection.activeTransform == null)
            {
                EditorUtility.DisplayDialog("No Transform Selected", "Select a GameObject in the Hierarchy whose position you want to add to the tower positions.", "OK");
            }
            else
            {
                Vector3 pos = Selection.activeTransform.position;
                Undo.RecordObject(map, "Add Tower Position");
                map.towerPositions.Add(pos);
                EditorUtility.SetDirty(map);
                Debug.Log($"Added tower position at {pos} from selected transform {Selection.activeTransform.name}");
            }
        }

        if (GUILayout.Button("Add Path Point at Scene View Pivot"))
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv == null)
            {
                EditorUtility.DisplayDialog("No Scene View", "Open a Scene View and position the camera where you want to add the path point (Scene view pivot will be used).", "OK");
            }
            else
            {
                Undo.RecordObject(map, "Add Path Point");
                map.pathPoints.Add(sv.pivot);
                EditorUtility.SetDirty(map);
                Debug.Log($"Added path point at {sv.pivot}");
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Clear Tower Positions"))
        {
            if (EditorUtility.DisplayDialog("Clear tower positions?", "This will remove all configured tower positions from the MapData.", "Yes", "Cancel"))
            {
                Undo.RecordObject(map, "Clear Tower Positions");
                map.towerPositions.Clear();
                EditorUtility.SetDirty(map);
                Debug.Log("Cleared tower positions");
            }
        }

        if (GUILayout.Button("Clear Path Points"))
        {
            if (EditorUtility.DisplayDialog("Clear path points?", "This will remove all configured path points from the MapData.", "Yes", "Cancel"))
            {
                Undo.RecordObject(map, "Clear Path Points");
                map.pathPoints.Clear();
                EditorUtility.SetDirty(map);
                Debug.Log("Cleared path points");
            }
        }
    }
}
