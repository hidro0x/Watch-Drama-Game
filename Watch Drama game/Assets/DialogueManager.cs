using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    public DialogueDatabase dialogueDatabase;
    private Node currentNode;
    
    private int currentTurn = 1;
    
    [Header("Turn Ayarları")]
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private int maxTurnCount = 200;

    ChoiceSelectionUI choiceSelectionUI;
    BarUIController barUIController;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        choiceSelectionUI = FindFirstObjectByType<ChoiceSelectionUI>(FindObjectsInactive.Include);
    }

    private void Start()
    {
        if (turnText != null)
            turnText.text = "";
    }

    public void UpdateMapName(string mapName)
    {
        if (turnText != null)
        {
            turnText.text = mapName;
        }
    }
    
    public void UpdateTurnText()
    {
        if (turnText != null)
        {
            turnText.text = $"Turn: {currentTurn}";
        }
    }


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
        FindFirstObjectByType<ChoiceSelectionUI>(FindObjectsInactive.Include).AnimateChoicePanel();
    }

    void ShowDialogue(DialogueNode node)
    {
        // ChoiceSelectionUI ile entegre gösterim
        if (choiceSelectionUI != null)
        {
            choiceSelectionUI.ShowUI(node);
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
        UpdateTurnText();
        
        // Turn limit kontrolü
        CheckTurnLimit();
    }
    
    // Turn artır (test için)
    public void IncrementTurn()
    {
        currentTurn++;
        
        // Turn limit kontrolü
        CheckTurnLimit();
    }
    
    // Turn limit kontrolü
    private void CheckTurnLimit()
    {
        TurnConditionSystem turnConditionSystem = FindFirstObjectByType<TurnConditionSystem>();
        if (turnConditionSystem != null)
        {
            turnConditionSystem.CheckTurnConditionsOnTurnChange();
        }
    }

    // Turn sıfırla
    public void ResetTurn()
    {
        currentTurn = 1;
    }
}