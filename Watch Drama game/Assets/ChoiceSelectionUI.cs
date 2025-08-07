using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;

public class ChoiceSelectionUI : MonoBehaviour
{
    [SerializeField] private Image choiceSelectionImage;
    [SerializeField] private List<ChoiceButtonSlot> choiceButtonSlotList = new List<ChoiceButtonSlot>();

    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private GameObject barPanel;

    [SerializeField] private Image characterImage;
    [SerializeField] private Image backgroundImage;

    private Node dialogueNode;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private RectTransform bottomPanel;

    [SerializeField] private RectTransform choicesPanel;

    public static event Action OnDialogueChoiceMade;

    BarUIController barUIController;

    // Animation settings
    private const float ANIMATION_DURATION = 0.5f;
    private const float SCREEN_OFFSET_MULTIPLIER = 1.5f;
    private const float TEXT_REVEAL_DURATION = 1.5f;
    private const float CHARACTER_NAME_DELAY = 0.3f;

    private bool hasBeenInitialized = false; // Panel'in daha önce açılıp açılmadığını takip etmek için

    void Start()
    {
        barUIController = barPanel.transform.parent.GetComponent<BarUIController>();
        // rectTransform ekranın sağ dışında başlasın
        float screenWidth = Screen.width * SCREEN_OFFSET_MULTIPLIER;
        rectTransform.anchoredPosition = new Vector2(screenWidth, 0);
        // bottomPanel ekranın üstünde başlasın (görünür)
        if (bottomPanel != null)
            bottomPanel.anchoredPosition = new Vector2(0, 0);
    }

    public void ShowUI(DialogueNode dialogueNode)
    {
        bool isFirstTime = !hasBeenInitialized;
        
        gameObject.SetActive(true);
        this.dialogueNode = dialogueNode;
        choiceSelectionImage.sprite = dialogueNode.sprite;
        barPanel.SetActive(true);
        barUIController.UpdateBars();
        
        // Global diyalog kontrolü
        if (dialogueNode.isGlobalDialogue)
        {
            Debug.Log("Global diyalog gösteriliyor...");
            // Global diyalog için özel işlemler burada yapılabilir
        }
        
        SetChoices(dialogueNode.choices);
        
        if (isFirstTime)
        {
            // İlk kez açılıyorsa bottomPanel animasyonu ve entrance animasyonu
            hasBeenInitialized = true;
            if (bottomPanel != null)
            {
                float screenHeight = Screen.height * SCREEN_OFFSET_MULTIPLIER;
                bottomPanel.DOAnchorPos(new Vector2(0, -screenHeight), ANIMATION_DURATION).SetEase(Ease.InQuad);
            }
            AnimateEntrance();
        }
        else
        {
            // Panel zaten açıksa sadece choice panel animasyonu
            AnimateChoicePanel();
        }
    }

    private void AnimateEntrance()
    {
        // Calculate screen dimensions with offset
        float screenWidth = Screen.width * SCREEN_OFFSET_MULTIPLIER;
        float screenHeight = Screen.height * SCREEN_OFFSET_MULTIPLIER;
        
        // Set initial positions
        rectTransform.anchoredPosition = new Vector2(-screenWidth, 0);
        choicesPanel.anchoredPosition = new Vector2(0, -screenHeight);
        characterImage.rectTransform.anchoredPosition = new Vector2(screenWidth *1.5f, 0);
        
        // Clear text initially
        characterNameText.text = "";
        descriptionText.text = "";
        
        // 1. Main panel slides in from left
        rectTransform.DOAnchorPos(new Vector2(0, 0), ANIMATION_DURATION).SetEase(Ease.OutQuad)
            .OnComplete(() => {
                // 2. Choices panel slides up from bottom
                choicesPanel.DOAnchorPos(new Vector2(0, 0), ANIMATION_DURATION).SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        // 3. Character image slides from right to center
                        characterImage.rectTransform.DOAnchorPos(new Vector2(0, 0), ANIMATION_DURATION).SetEase(Ease.OutQuad)
                            .OnComplete(() => {
                                // 4. Start text reveal animations
                                AnimateTextReveal();
                            });
                    });
            });
    }

    private void AnimateTextReveal()
    {
        // Start typewriter effects
        StartCoroutine(TypewriterEffect(characterNameText, dialogueNode.name, TEXT_REVEAL_DURATION * 0.6f, CHARACTER_NAME_DELAY));
        StartCoroutine(TypewriterEffect(descriptionText, dialogueNode.text, TEXT_REVEAL_DURATION, CHARACTER_NAME_DELAY + 0.2f));
    }

    private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string fullText, float duration, float delay)
    {
        // Wait for delay
        yield return new WaitForSeconds(delay);
        
        textComponent.text = "";
        int totalCharacters = fullText.Length;
        float timePerCharacter = duration / totalCharacters;
        
        for (int i = 0; i <= totalCharacters; i++)
        {
            textComponent.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(timePerCharacter);
        }
    }

    public void AnimateChoicePanel()
    {
        // 1. Choices panel slides down
        float screenHeight = Screen.height * SCREEN_OFFSET_MULTIPLIER;
        
        choicesPanel.DOAnchorPos(new Vector2(0, -screenHeight), ANIMATION_DURATION).SetEase(Ease.InQuad)
            .OnComplete(() => {
                // Refresh choices
                if (dialogueNode is DialogueNode dialogueNodeCast)
                {
                    SetChoices(dialogueNodeCast.choices);
                }
                
                // 2. Character image slides out to right
                float screenWidth = Screen.width * SCREEN_OFFSET_MULTIPLIER;
                characterImage.rectTransform.DOAnchorPos(new Vector2(screenWidth, 0), ANIMATION_DURATION).SetEase(Ease.InQuad)
                    .OnComplete(() => {
                        // 3. Character image teleports to left and slides back to center
                        characterImage.rectTransform.anchoredPosition = new Vector2(-screenWidth, 0);
                        characterImage.rectTransform.DOAnchorPos(new Vector2(0, 0), ANIMATION_DURATION).SetEase(Ease.OutQuad)
                            .OnComplete(() => {
                                // 4. Choices panel slides back up
                                choicesPanel.DOAnchorPos(new Vector2(0, 0), ANIMATION_DURATION).SetEase(Ease.OutQuad)
                                    .OnComplete(() => {
                                        // 5. Re-animate text reveal for new dialogue
                                        AnimateTextReveal();
                                    });
                            });
                    });
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
        // Animate exit to left
        float screenWidth = Screen.width * SCREEN_OFFSET_MULTIPLIER;
        
        rectTransform.DOAnchorPos(new Vector2(-screenWidth, 0), ANIMATION_DURATION).SetEase(Ease.InQuad)
            .OnComplete(() => {
                gameObject.SetActive(false);
                barPanel.SetActive(false);
                hasBeenInitialized = false; // Panel kapatıldığında flag'i sıfırla
            });
        // bottomPanel tekrar yukarı çıksın
        if (bottomPanel != null)
        {
            bottomPanel.DOAnchorPos(new Vector2(0, 0), ANIMATION_DURATION).SetEase(Ease.OutQuad);
        }
    }

    public void TriggerDialogueChoiceEvent()
    {
        OnDialogueChoiceMade?.Invoke();
        // Choice selection sonrası text'leri temizle
        if (characterNameText != null) characterNameText.text = "";
        if (descriptionText != null) descriptionText.text = "";
    }
}
