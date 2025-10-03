using UnityEngine;

public static class MapLaunchBridge
{
    // -1 means none
    public static int SelectedMapIndex = -1;
}

// Optional component to put on an always-present GameObject in the main scene.
// It will ask MapManager to load the selected map index if present.
public class MapLaunchApplier : MonoBehaviour
{
    public TowerFusion.MapManager mapManager;

    private void Start()
    {
        if (MapLaunchBridge.SelectedMapIndex >= 0)
        {
            if (mapManager == null)
            {
                mapManager = FindObjectOfType<TowerFusion.MapManager>();
            }

            if (mapManager != null && mapManager.MapLibrary != null)
            {
                int idx = MapLaunchBridge.SelectedMapIndex;
                MapLaunchBridge.SelectedMapIndex = -1;
                mapManager.LoadMapByIndex(idx);
            }
            else
            {
                Debug.LogWarning("MapLaunchApplier: No MapManager or MapLibrary found to apply selection.");
            }
        }
    }
}
