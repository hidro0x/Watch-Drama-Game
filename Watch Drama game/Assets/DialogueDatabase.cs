using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[System.Serializable]
public class MapInitialValues
{
    [Title("Harita Başlangıç Değerleri")]
    [LabelWidth(80)]
    public MapType mapType;
    
    [Title("Bar Başlangıç Değerleri")]
    [LabelWidth(80)]
    [Range(0, 100)]
    public int initialTrust = 50;
    
    [LabelWidth(80)]
    [Range(0, 100)]
    public int initialFaith = 50;
    
    [LabelWidth(80)]
    [Range(0, 100)]
    public int initialHostility = 0;
    
    // BarValues'e dönüştürme
    public BarValues GetBarValues()
    {
        return new BarValues
        {
            trust = initialTrust,
            faith = initialFaith,
            hostility = initialHostility
        };
    }
    
    // MapValues'e dönüştürme
    public MapValues GetMapValues()
    {
        return new MapValues(initialTrust, initialFaith, initialHostility);
    }
}

[CreateAssetMenu(menuName = "Kahin/Dialog Database")]
public class DialogueDatabase : SerializedScriptableObject
{
    [Title("Haritalar")]
    [TableList]
    public List<MapType> maps = new List<MapType>();
    
    [Title("Genel Diyalog Havuzu")]
    [TableList]
    public List<DialogueNode> generalDialogues = new List<DialogueNode>();
    
    [Title("Harita Özel Genel Diyalogları")] 
    public Dictionary<MapType, List<DialogueNode>> specialGeneralDialoguesByMap = new();
    
    
    [Title("Map'e Özel NORMAL Arkaplanlar")] 
    public Dictionary<MapType, Sprite> mapSpecificDialogueBackgrounds = new();
    
    [Title("Global Diyalog Etkileri")]
    [TableList]
    public List<GlobalDialogueNode> globalDialogueEffects = new List<GlobalDialogueNode>();
    
    [Title("Turn Aralıkları")]
    [LabelWidth(150)]
    public int mapSpecificInterval = 5; // Her 5 turn'de map'e özel diyalog
    
    [LabelWidth(150)]
    public int globalDialogueInterval = 16; // Her 15 turn'de global diyalog
    
    [Title("Rival Encounter Ayarları")]
    [LabelWidth(180)] public bool enableRivalEncounters = true;
    [LabelWidth(180), Range(0f, 1f)] public float rivalEncounterChance = 0.15f;
    [LabelWidth(180)] public int rivalEncounterInterval = 6; // Her 6 turda bir şans kontrolü
    
} 