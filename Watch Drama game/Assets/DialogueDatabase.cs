using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Kahin/Dialog Database")]
public class DialogueDatabase : ScriptableObject
{
    [TableList]
    public List<DialogueNode> dialogueNodes = new List<DialogueNode>();
} 