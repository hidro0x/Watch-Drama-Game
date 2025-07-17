using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChoiceButtonSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI choiceText;
    [SerializeField] private Button choiceButton;

    private DialogueChoice choice;

    public void SetChoice(DialogueChoice choice)
    {
        this.choice = choice;
        SetChoiceText(choice.text);
    }

    void Awake()
    {
        choiceButton = GetComponent<Button>();
        choiceButton.onClick.AddListener(OnChoiceButtonClicked);
    }
    public void SetChoiceText(string text)
    {
        choiceText.text = text;
    }

    private void OnChoiceButtonClicked()
    {
        Debug.Log("Choice button clicked");
    }
}
