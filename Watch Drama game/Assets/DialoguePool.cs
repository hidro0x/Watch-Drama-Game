using UnityEngine;

public enum DialoguePoolType
{
    General,        // Genel diyalog havuzu
    MapSpecific,    // Haritaya özel diyalog havuzu
    SpecialGeneral  // Genel özel diyalog havuzu (her y turnde)
}

[System.Serializable]
public class DialoguePool
{
    public DialoguePoolType poolType;
    public string poolName;
    public System.Collections.Generic.List<DialogueNode> dialogues = new System.Collections.Generic.List<DialogueNode>();
} 