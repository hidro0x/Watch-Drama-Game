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

    [LabelWidth(60)]
    public int? minTurn;
    [LabelWidth(60)]
    public int? maxTurn;
    [LabelWidth(60)]
    public int? minTrust;
    [LabelWidth(60)]
    public int? maxTrust;
    [LabelWidth(60)]
    public int? minFaith;
    [LabelWidth(60)]
    public int? maxFaith;
    [LabelWidth(60)]
    public int? minHostility;
    [LabelWidth(60)]
    public int? maxHostility;

    [TableList(ShowIndexLabels = true)]
    public List<DialogueChoice> choices = new List<DialogueChoice>();
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
    [LabelWidth(60)]
} 