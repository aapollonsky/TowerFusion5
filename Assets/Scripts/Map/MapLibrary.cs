using UnityEngine;
using System.Collections.Generic;

namespace TowerFusion
{
    [CreateAssetMenu(fileName = "MapLibrary", menuName = "Tower Fusion/Map Library")]
    public class MapLibrary : ScriptableObject
    {
        public List<MapData> maps = new List<MapData>();

        public MapData GetMap(int index)
        {
            if (maps == null || maps.Count == 0)
                return null;
            if (index < 0 || index >= maps.Count)
                return null;
            return maps[index];
        }

        public int GetIndex(MapData map)
        {
            if (maps == null)
                return -1;
            return maps.IndexOf(map);
        }

        public MapData GetMapByName(string name)
        {
            if (string.IsNullOrEmpty(name) || maps == null)
                return null;
            return maps.Find(m => m != null && m.mapName == name);
        }
    }
}
