using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using TowerFusion;

// Draws MapData path, tower positions and spawn/end points in the Scene view
[InitializeOnLoad]
public static class MapDataGizmoDrawer
{
    static MapDataGizmoDrawer()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private static void OnSceneGUI(SceneView sceneView)
    {
        var selected = Selection.activeObject as MapData;
        if (selected == null)
            return;

        // Draw path points
        var pts = selected.pathPoints;
        if (pts != null && pts.Count > 0)
        {
            Handles.color = Color.green;
            for (int i = 0; i < pts.Count; i++)
            {
                Handles.SphereHandleCap(0, pts[i], Quaternion.identity, 0.25f, EventType.Repaint);
                if (i < pts.Count - 1)
                    Handles.DrawLine(pts[i], pts[i + 1]);
            }
        }

        // Draw tower positions
        var towers = selected.towerPositions;
        if (towers != null && towers.Count > 0)
        {
            Handles.color = Color.cyan;
            foreach (var t in towers)
            {
                Handles.RectangleHandleCap(0, t, Quaternion.identity, 0.3f, EventType.Repaint);
            }
        }

        // Spawn and end points
        Handles.color = Color.red;
        Handles.SphereHandleCap(0, selected.enemySpawnPoint, Quaternion.identity, 0.3f, EventType.Repaint);

        Handles.color = Color.magenta;
        Handles.SphereHandleCap(0, selected.enemyEndPoint, Quaternion.identity, 0.3f, EventType.Repaint);
    }
}
#endif
