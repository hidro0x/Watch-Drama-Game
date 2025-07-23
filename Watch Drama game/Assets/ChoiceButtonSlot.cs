using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChoiceButtonSlot : MonoBehaviour
{
    private TextMeshProUGUI choiceText;
    private Button choiceButton;

    private DialogueChoice choice;

    public void SetChoice(DialogueChoice choice)
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
        Debug.Log($"Choice button clicked: {choice.text}");
        
        // DialogueChoice'tan ChoiceEffect oluştur ve GameManager'a gönder
        if (choice != null)
        {
            ChoiceEffect effect = new ChoiceEffect(
                choice.trustChange, 
                choice.faithChange, 
                choice.hostilityChange
            );
            
            GameManager.MakeChoice(effect);
        }
    }
}
