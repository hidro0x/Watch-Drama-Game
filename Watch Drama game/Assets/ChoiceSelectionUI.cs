using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening; // DOTween ekle
using System;

public class ChoiceSelectionUI : MonoBehaviour
{
    [SerializeField] private Image choiceSelectionImage;
    [SerializeField] private List<ChoiceButtonSlot> choiceButtonSlotList = new List<ChoiceButtonSlot>();
    [SerializeField] private Button returnButton;

    private Node dialogueNode;
    [SerializeField]private RectTransform rectTransform;

    public static event Action OnDialogueChoiceMade;

    void Start()
    {
        gameObject.SetActive(false);
        returnButton.onClick.AddListener(OnReturnButtonClicked);
    }

    public void ShowUI(DialogueNode dialogueNode)
    {
        gameObject.SetActive(true);
        this.dialogueNode = dialogueNode;
        choiceSelectionImage.sprite = dialogueNode.sprite;
        SetChoices(dialogueNode.choices);
    }
    
    public void ShowUI(GlobalDialogueNode globalDialogueNode)
    {
        gameObject.SetActive(true);
        this.dialogueNode = globalDialogueNode;
        choiceSelectionImage.sprite = globalDialogueNode.sprite;
        SetGlobalChoices(globalDialogueNode.choices);
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
    
    private void SetGlobalChoices(List<GlobalDialogueChoice> globalChoices)
    {
        for (int i = 0; i < globalChoices.Count; i++)
        {
            choiceButtonSlotList[i].SetChoice(globalChoices[i]);
        }
    }

    public void OnReturnButtonClicked()
    {
        gameObject.SetActive(false);
        if (dialogueNode is DialogueNode dialogue)
        {
            MapManager.Instance.SaveDialogueForCurrentMap(dialogue);
        }
    }

    public void SetReturnButtonActive(bool active)
    {
        returnButton.enabled = active;
    }
}
