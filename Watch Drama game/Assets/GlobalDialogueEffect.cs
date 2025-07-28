using System;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Sirenix.Serialization;

[System.Serializable]
public struct BarValues
{
    public int trust;
    public int faith;
    public int hostility;
}

[System.Serializable]
public class GlobalDialogueEffect
{
    [Title("Diyalog")]
    public DialogueNode dialogue;

    [Title("Global Bar Etkileri")]
    [DictionaryDrawerSettings(KeyLabel = "Ülke", ValueLabel = "Bar Değerleri")]
    [OdinSerialize]
    public Dictionary<MapType, BarValues> countryEffects = new Dictionary<MapType, BarValues>();

    
} 