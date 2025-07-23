using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueNode
{
    [LabelWidth(60)]
    public string id;

    [PreviewField(70), LabelWidth(60)]
    public Sprite sprite;

    [TextArea(2, 4), LabelWidth(60)]
    public string text;

    [TableList(ShowIndexLabels = true)]
    public List<DialogueChoice> choices = new List<DialogueChoice>();

    public bool isSpecial; // özel diyalog flag'i
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
} 