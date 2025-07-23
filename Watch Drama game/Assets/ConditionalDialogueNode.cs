using System;
using UnityEngine;

[Serializable]
public class ConditionalDialogueNode {
    public DialogueNode dialogue;
    public ConditionType conditionType;
    public int? minValue;
    public int? maxValue;
    // Gelişmiş koşullar için public Func<GameState, bool> predicate; eklenebilir
}

public enum ConditionType {
    Turn,
    Trust,
    Faith,
    Custom
} 