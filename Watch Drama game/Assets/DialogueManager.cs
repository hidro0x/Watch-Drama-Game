using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public DialogueDatabase dialogueDatabase;
    private Node currentNode;
    
    private int currentTurn = 1;
    
    [Header("Turn Ayarları")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private int maxTurnCount = 10;
    

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
    
    public void ShowSpecificDialogue(GlobalDialogueNode node)
    {
        if (node == null)
        {
            Debug.LogError("Gösterilecek global diyalog null!");
            return;
        }
        
        currentNode = node;
        ShowGlobalDialogue(node);
    }
    
    // Seçim yapıldığında çağrılacak
    public void OnChoiceMade()
    {
        FindFirstObjectByType<ChoiceSelectionUI>(FindObjectsInactive.Include).AnimateChoicePanel();
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
    
    void ShowGlobalDialogue(GlobalDialogueNode node)
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
    public Node GetCurrentNode() => currentNode;

    // Turn getter
    public int GetCurrentTurn() => currentTurn;
    
    // Max turn getter
    public int GetMaxTurnCount() => maxTurnCount;

    // Turn ilerlet
    public void NextTurn()
    {
        currentTurn++;
        turnText.text = "Turn: " + currentTurn;
        
        // Turn limit kontrolü
        CheckTurnLimit();
    }
    
    // Turn artır (test için)
    public void IncrementTurn()
    {
        currentTurn++;
        turnText.text = "Turn: " + currentTurn;
        
        // Turn limit kontrolü
        CheckTurnLimit();
    }
    
    // Turn limit kontrolü
    private void CheckTurnLimit()
    {
        TurnConditionSystem turnConditionSystem = UnityEngine.Object.FindObjectOfType<TurnConditionSystem>();
        if (turnConditionSystem != null)
        {
            turnConditionSystem.CheckTurnConditionsOnTurnChange();
        }
    }

    // Turn sıfırla
    public void ResetTurn()
    {
        currentTurn = 1;
        turnText.text = "Turn: " + currentTurn;
    }
} 