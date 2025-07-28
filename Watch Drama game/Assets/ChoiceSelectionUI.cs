using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening; // DOTween ekle
using System;
using TMPro;

public class ChoiceSelectionUI : MonoBehaviour
{
    [SerializeField] private Image choiceSelectionImage;
    [SerializeField] private List<ChoiceButtonSlot> choiceButtonSlotList = new List<ChoiceButtonSlot>();

    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private GameObject barPanel;

    private Node dialogueNode;
    [SerializeField]private RectTransform rectTransform;

    public static event Action OnDialogueChoiceMade;

    void Start()
    {
        OnPanelClosed();
    }

    public void ShowUI(DialogueNode dialogueNode)
    {
        gameObject.SetActive(true);
        this.dialogueNode = dialogueNode;
        choiceSelectionImage.sprite = dialogueNode.sprite;
        descriptionText.text = dialogueNode.text;
        barPanel.SetActive(true);
        // Global diyalog kontrolü
        if (dialogueNode.isGlobalDialogue)
        {
            Debug.Log("Global diyalog gösteriliyor...");
            // Global diyalog için özel işlemler burada yapılabilir
        }
        
        SetChoices(dialogueNode.choices);
    }
    


    public void AnimateChoicePanel()
    {
        float screenHeight = Screen.height;
        // 1. Yukarı animasyon
        rectTransform.DOAnchorPosY(screenHeight, 0.5f).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            OnDialogueChoiceMade?.Invoke();
            // 2. Aniden aşağıya setle
            rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, -screenHeight);
            // 3. Tekrar yukarı animasyonla gel
            rectTransform.DOAnchorPosY(0, 0.5f).SetEase(Ease.InOutQuad);
        });

        
    }

    private void SetChoices(List<DialogueChoice> choices)
    {
        for (int i = 0; i < choices.Count; i++)
        {
            choiceButtonSlotList[i].SetChoice(choices[i]);
        }
    }
    


    public void OnPanelClosed()
    {
        gameObject.SetActive(false);
        barPanel.SetActive(false);
    }


    
    public void TriggerDialogueChoiceEvent()
    {
        OnDialogueChoiceMade?.Invoke();
    }
}
