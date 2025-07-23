using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public DialogueDatabase dialogueDatabase;
    private DialogueNode currentNode;
    
    private int currentTurn = 1;

    [SerializeField] private TextMeshProUGUI turnText;
    

    public void ShowSpecificDialogue(DialogueNode node)
    {
        if (node == null)
        {
            Debug.LogError("Gösterilecek diyalog null!");
            return;
        }
        
        currentNode = node;
        ShowDialogue(node);
    }
    
    // Seçim yapıldığında çağrılacak
    public void OnChoiceMade()
    {
        FindFirstObjectByType<ChoiceSelectionUI>().AnimateChoicePanel();
    }

    void ShowDialogue(DialogueNode node)
    {
        // ChoiceSelectionUI ile entegre gösterim
        ChoiceSelectionUI ui = FindFirstObjectByType<ChoiceSelectionUI>(FindObjectsInactive.Include);
        if (ui != null)
        {
            ui.ShowUI(node);
        }
        else
        {
            Debug.LogError("ChoiceSelectionUI sahnede bulunamadı!");
        }
    }
    
    // Getter methodları
    public DialogueNode GetCurrentNode() => currentNode;

    // Turn getter
    public int GetCurrentTurn() => currentTurn;

    // Turn ilerlet
    public void NextTurn()
    {
        currentTurn++;
        turnText.text = "Turn: " + currentTurn;
    }

    // Turn sıfırla
    public void ResetTurn()
    {
        currentTurn = 1;
        turnText.text = "Turn: " + currentTurn;
    }
} 