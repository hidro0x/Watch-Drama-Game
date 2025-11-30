using UnityEngine;
using Sirenix.OdinInspector;

// MapData system for managing map information in MapCompletionPanelUI

[System.Serializable]
public class MapData
{
    [Title("Map Information")]
    [LabelWidth(100)]
    public MapType mapType;
    
    [LabelWidth(100)]
    public string displayName;
    
    [LabelWidth(100)]
    [PreviewField(70)]
    public Sprite mapIcon;
    
    [LabelWidth(100)]
    [TextArea(2, 4)]
    public string description;
    
    [Title("Visual Settings")]
    [LabelWidth(100)]
    public Color mapColor = Color.white;
    
    [LabelWidth(100)]
    public Color textColor = Color.white;
}

[CreateAssetMenu(fileName = "MapDataCollection", menuName = "Game Data/Map Data Collection")]
public class MapDataCollection : ScriptableObject
{
    [Title("Map Data Collection")]
    [TableList(ShowIndexLabels = true)]
    public MapData[] mapDataArray = new MapData[5]; // One for each MapType
    
    /// <summary>
    /// Get map data for a specific map type
    /// </summary>
    public MapData GetMapData(MapType mapType)
    {
        foreach (var mapData in mapDataArray)
        {
            if (mapData != null && mapData.mapType == mapType)
            {
                return mapData;
            }
        }
        
        // Return default data if not found
        Debug.LogWarning($"MapData not found for {mapType}, returning default data");
        return CreateDefaultMapData(mapType);
    }
    
    /// <summary>
    /// Create default map data for a map type
    /// </summary>
    private MapData CreateDefaultMapData(MapType mapType)
    {
        return new MapData
        {
            mapType = mapType,
            displayName = GetDefaultMapName(mapType),
            mapIcon = null,
            description = $"Default description for {mapType}",
            mapColor = Color.white,
            textColor = Color.white
        };
    }
    
    /// <summary>
    /// Get default map name for a map type
    /// </summary>
    private string GetDefaultMapName(MapType mapType)
    {
        switch (mapType)
        {
            case MapType.Astrahil:
                return "Astrahil";
            case MapType.Agnari:
                return "Agnari";
            case MapType.Solarya:
                return "Solarya";
            case MapType.Theon:
                return "Theon";
            case MapType.Varnan:
                return "Varnan";
            default:
                return mapType.ToString();
        }
    }
    
    /// <summary>
    /// Initialize default map data for all map types
    /// </summary>
    [Button("Initialize Default Map Data")]
    public void InitializeDefaultMapData()
    {
        mapDataArray = new MapData[5];
        
        mapDataArray[0] = new MapData
        {
            mapType = MapType.Astrahil,
            displayName = "Astrahil",
            mapIcon = null,
            description = "The mystical realm of Astrahil, where ancient magic flows through every stone.",
            mapColor = new Color(0.8f, 0.9f, 1.0f), // Light blue
            textColor = Color.white
        };
        
        mapDataArray[1] = new MapData
        {
            mapType = MapType.Agnari,
            displayName = "Agnari",
            mapIcon = null,
            description = "The proud kingdom of Agnari, where honor and tradition reign supreme.",
            mapColor = new Color(1.0f, 0.9f, 0.8f), // Light orange
            textColor = Color.white
        };
        
        mapDataArray[2] = new MapData
        {
            mapType = MapType.Solarya,
            displayName = "Solarya",
            mapIcon = null,
            description = "The radiant empire of Solarya, bathed in eternal golden light.",
            mapColor = new Color(1.0f, 1.0f, 0.8f), // Light yellow
            textColor = Color.black
        };
        
        mapDataArray[3] = new MapData
        {
            mapType = MapType.Theon,
            displayName = "Theon",
            mapIcon = null,
            description = "The scholarly nation of Theon, where wisdom and knowledge are treasured above all.",
            mapColor = new Color(0.9f, 0.8f, 1.0f), // Light purple
            textColor = Color.white
        };
        
        mapDataArray[4] = new MapData
        {
            mapType = MapType.Varnan,
            displayName = "Varnan",
            mapIcon = null,
            description = "The shadowed lands of Varnan, where mysteries and secrets dwell in every corner.",
            mapColor = new Color(0.7f, 0.7f, 0.7f), // Light gray
            textColor = Color.white
        };
        
        Debug.Log("Default map data initialized for all map types");
    }
}
