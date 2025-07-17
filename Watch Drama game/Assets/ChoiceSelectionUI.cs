using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChoiceSelectionUI : MonoBehaviour
{
    [SerializeField] private Image choiceSelectionImage;
    [SerializeField] private ChoiceButtonSlot choiceButtonSlotList;

    private DialogueNode dialogueNode;

    public void ShowUI(DialogueNode dialogueNode)
    {
        this.dialogueNode = dialogueNode;
        SetChoices(dialogueNode.choices);
    }


    private void SetChoices(List<DialogueChoice> choices)
    {
        foreach (var choice in choices)
        {
            choiceButtonSlotList.SetChoice(choice);
        }
    }
}
