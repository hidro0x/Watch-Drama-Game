using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.Serialization;

[System.Serializable]
public class Node
{
    [LabelWidth(60)]
    public string id;

    [LabelWidth(60)]
    public string name;

    [PreviewField(70), LabelWidth(60)]
    public Sprite sprite;

    [TextArea(2, 4), LabelWidth(60)]
    public string text;

}

[System.Serializable]
public class DialogueNode : Node
{
    [TableList(ShowIndexLabels = true)]
    public List<DialogueChoice> choices = new List<DialogueChoice>();
    
    // Global diyalog olup olmadığını belirten flag
    public bool isGlobalDialogue = false;
}

[System.Serializable]
public class  GlobalDialogueNode : Node
{

    [TableList(ShowIndexLabels = true)]
    public List<GlobalDialogueChoice> choices = new List<GlobalDialogueChoice>();

}

[System.Serializable]
public class DialogueChoice
{
    [LabelWidth(60)]
    public string text;
    
    [LabelWidth(60)]
    public int trustChange;
    [LabelWidth(60)]
    public int faithChange;
    [LabelWidth(60)]
    public int hostilityChange;
    
    // Global diyalog seçimi olup olmadığını belirten flag
    public bool isGlobalChoice = false;
    // Bir sonraki node'un id'si
    public string nextNodeId;
} 

[System.Serializable]
public class GlobalDialogueChoice
{
    [LabelWidth(60)]
    public string text;
    
    [Title("Global Etkiler (Tüm Ülkeler)")]
    [TableList(ShowIndexLabels = true)]
    public List<CountryBarEffect> globalEffects = new List<CountryBarEffect>(){
        new CountryBarEffect(){
            country = MapType.Agnari,
            trustChange =0,
            faithChange =0,
            hostilityChange =0
        },
        new CountryBarEffect(){
            country = MapType.Astrahil,
            trustChange =0,
            faithChange =0,
            hostilityChange =0
        },
        new CountryBarEffect(){
            country = MapType.Varnan,
            trustChange =0,
            faithChange =0,
            hostilityChange = 0
        },
        new CountryBarEffect(){
            country = MapType.Theon,
            trustChange =0,
            faithChange =0,
            hostilityChange =0
        },
        new CountryBarEffect(){
            country = MapType.Solarya,
            trustChange =0,
            faithChange =0,
            hostilityChange =0
        },
        
    };
    
    public bool hasGlobalEffects => globalEffects != null && globalEffects.Count > 0;
}

[System.Serializable]
public class CountryBarEffect
{
    [LabelWidth(80)]
    public MapType country;
    
    [LabelWidth(60)]
    public int trustChange;
    
    [LabelWidth(60)]
    public int faithChange;
    
    [LabelWidth(60)]
    public int hostilityChange;
} 