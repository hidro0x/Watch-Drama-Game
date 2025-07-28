using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChoiceButtonSlot : MonoBehaviour
{
    private TextMeshProUGUI choiceText;
    private Button choiceButton;

    private object choice; // Hem DialogueChoice hem GlobalDialogueChoice için

    public void SetChoice(DialogueChoice choice)
    {
        this.choice = choice;
        SetChoiceText(choice.text);
    }
    
    public void SetChoice(GlobalDialogueChoice choice)
    {
        this.choice = choice;
        SetChoiceText(choice.text);
    }

    void Awake()
    {
        choiceButton = GetComponent<Button>();
        choiceText = GetComponentInChildren<TextMeshProUGUI>();
        choiceButton.onClick.AddListener(OnChoiceButtonClicked);
    }
    
    public void SetChoiceText(string text)
    {
        choiceText.text = text;
    }

    private void OnChoiceButtonClicked()
    {
        if (choice == null) return;
        
        if (choice is DialogueChoice dialogueChoice)
        {
            Debug.Log($"Dialogue choice clicked: {dialogueChoice.text}");
            
            // Normal diyalog: Sadece aktif ülkeyi etkiler
            ChoiceEffect effect = new ChoiceEffect(
                dialogueChoice.trustChange, 
                dialogueChoice.faithChange, 
                dialogueChoice.hostilityChange
            );
            
            GameManager.MakeChoice(effect);
        }
        else if (choice is GlobalDialogueChoice globalChoice)
        {
            Debug.Log($"Global dialogue choice clicked: {globalChoice.text}");
            
            // Global diyalog: Sadece global effects var
            ApplyGlobalEffects(globalChoice);
        }
    }
    
    private void ApplyGlobalEffects(GlobalDialogueChoice globalChoice)
    {
        var globalEffect = new GlobalDialogueEffect();
        
        if (globalChoice.globalEffects != null && globalChoice.globalEffects.Count > 0)
        {
            // Özel global effects varsa onları kullan
            globalEffect.countryEffects = new Dictionary<MapType, BarValues>();
            foreach (var countryEffect in globalChoice.globalEffects)
            {
                globalEffect.countryEffects[countryEffect.country] = new BarValues
                {
                    trust = countryEffect.trustChange,
                    faith = countryEffect.faithChange,
                    hostility = countryEffect.hostilityChange
                };
            }
        }
        else
        {
            // Global effects yoksa hiçbir şey yapma
            Debug.LogWarning("GlobalDialogueChoice'ta global effects tanımlanmamış!");
            return;
        }
        
        GameManager.Instance.ApplyGlobalDialogueEffect(globalEffect);
    }
    
}
