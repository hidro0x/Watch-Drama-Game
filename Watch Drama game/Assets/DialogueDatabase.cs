using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

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
    
    [Title("Global Koşullu Diyaloglar")]
    public List<ConditionalDialogueNode> globalConditionalDialogues = new List<ConditionalDialogueNode>();
    
    [Title("Genel Özel Diyalog Aralığı")]
    [LabelWidth(150)]
    public int specialGeneralInterval = 15; // Her y turnde genel özel diyalog
    
} 