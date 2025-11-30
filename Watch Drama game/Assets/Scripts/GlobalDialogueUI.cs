using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class GlobalDialogueUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private Image blackOverlay;
    [SerializeField] private Image fullscreenDialogueImage;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Choices Panel")]
    [SerializeField] private RectTransform choicesPanel;
    [SerializeField] private List<ChoiceButtonSlot> choiceButtonSlots = new List<ChoiceButtonSlot>();
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private float imageFadeInDuration = 1.5f;
    [SerializeField] private float typewriterSpeed = 0.03f;
    [SerializeField] private float choicesPanelSlideDuration = 0.6f;
    
    private DialogueNode currentDialogueNode;
    private bool isTypewriterActive = false;
    private Coroutine typewriterCoroutine;
    private bool isAnimating = false;
    
    private void Awake()
    {
        // Ensure components exist
        if (blackOverlay == null)
            Debug.LogError("Black Overlay is not assigned in GlobalDialogueUI!");
        if (fullscreenDialogueImage == null)
            Debug.LogError("Fullscreen Dialogue Image is not assigned in GlobalDialogueUI!");
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // Start hidden using alpha
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    
    /// <summary>
    /// Show the global dialogue with cinematic sequence
    /// </summary>
    public void ShowGlobalDialogue(DialogueNode dialogueNode)
    {
        if (isAnimating) return;
        
        Debug.Log($"GlobalDialogueUI.ShowGlobalDialogue called! Dialogue ID: {dialogueNode.id}, Text: {dialogueNode.text}, Background Sprite: {(dialogueNode.backgroundSprite != null ? dialogueNode.backgroundSprite.name : "NULL")}");
        
        currentDialogueNode = dialogueNode;
        
        // Make sure the GameObject is active
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
        
        // Setup initial state
        SetupInitialState();
        
        // Start the cinematic sequence
        StartCoroutine(CinematicSequence());
    }
    
    private void SetupInitialState()
    {
        // Black overlay starts at full black (full alpha)
        blackOverlay.color = new Color(0, 0, 0, 1);
        
        // Set fullscreen dialogue image from global dialogue data background sprite
        fullscreenDialogueImage.sprite = currentDialogueNode.backgroundSprite;
        
        // Set image alpha to 0 (invisible initially)
        Color imageColor = fullscreenDialogueImage.color;
        imageColor.a = 0f;
        fullscreenDialogueImage.color = imageColor;
        
        // Clear text initially
        dialogueText.text = "";
        
        // Hide choices panel (position it off-screen below)
        float screenHeight = Screen.height * 1.5f;
        choicesPanel.anchoredPosition = new Vector2(0, -screenHeight);
        
        // Set canvas group to be ready for animation
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
    
    private IEnumerator CinematicSequence()
    {
        isAnimating = true;
        
        // Phase 1: Fade out black overlay (1.0s)
        yield return StartCoroutine(Phase1_FadeOutBlackOverlay());
        
        // Phase 2: Slowly fade in the dialogue image (1.5s)
        yield return StartCoroutine(Phase2_FadeInDialogueImage());
        
        // Phase 3: Display dialogue text with typewriter (variable duration)
        yield return StartCoroutine(Phase3_DisplayDialogue());
        
        // Phase 4: Show choices panel (0.6s)
        yield return StartCoroutine(Phase4_ShowChoices());
        
        // Phase 5 happens after choice is made (handled in OnChoiceMade)
        
        isAnimating = false;
    }
    
    private IEnumerator Phase1_FadeOutBlackOverlay()
    {
        Debug.Log("Phase 1: Fading out black overlay");
        
        // Fade black overlay from full alpha (1) to transparent (0)
        blackOverlay.DOFade(0f, fadeOutDuration).SetEase(Ease.OutQuad);
        
        // Wait for fade to complete
        yield return new WaitForSeconds(fadeOutDuration);
    }
    
    private IEnumerator Phase2_FadeInDialogueImage()
    {
        Debug.Log("Phase 2: Slowly fading in dialogue image");
        
        // Slowly fade in the dialogue image from alpha 0 to 1
        fullscreenDialogueImage.DOFade(1f, imageFadeInDuration).SetEase(Ease.OutQuad);
        
        // Wait for fade to complete
        yield return new WaitForSeconds(imageFadeInDuration);
    }
    
    private IEnumerator Phase3_DisplayDialogue()
    {
        Debug.Log("Phase 3: Displaying dialogue with typewriter");
        
        // Start typewriter effect for dialogue text
        isTypewriterActive = true;
        
        // Show dialogue text
        yield return StartCoroutine(TypewriterEffect(dialogueText, currentDialogueNode.text, typewriterSpeed));
        
        isTypewriterActive = false;
        
        // Small pause before continuing
        yield return new WaitForSeconds(0.5f);
    }
    
    private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string fullText, float speed)
    {
        if (string.IsNullOrEmpty(fullText))
        {
            textComponent.text = "";
            yield break;
        }
        
        textComponent.text = "";
        
        for (int i = 0; i <= fullText.Length; i++)
        {
            if (!isTypewriterActive) // Skip if typewriter was cancelled
            {
                textComponent.text = fullText;
                yield break;
            }
            
            textComponent.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(speed);
        }
    }
    
    private void Update()
    {
        // Allow player to skip typewriter by clicking
        if (isTypewriterActive && Input.GetMouseButtonDown(0))
        {
            SkipTypewriter();
        }
    }
    
    private void SkipTypewriter()
    {
        isTypewriterActive = false;
        
        
        if (dialogueText != null && currentDialogueNode != null)
            dialogueText.text = currentDialogueNode.text;
    }
    
    
    private IEnumerator Phase4_ShowChoices()
    {
        Debug.Log("Phase 4: Showing choices");
        
        // Black overlay stays at 0 (transparent) since we want to show the image
        
        // Setup choices
        SetupChoices(currentDialogueNode.choices);
        
        // Slide choices panel up from bottom
        choicesPanel.DOAnchorPos(new Vector2(0, 0), choicesPanelSlideDuration).SetEase(Ease.OutQuad);
        
        yield return new WaitForSeconds(choicesPanelSlideDuration);
    }
    
    private void SetupChoices(List<DialogueChoice> choices)
    {
        for (int i = 0; i < choiceButtonSlots.Count && i < choices.Count; i++)
        {
            choiceButtonSlots[i].SetChoice(choices[i]);
            choiceButtonSlots[i].ResetForNewChoices();
        }
    }
    
    /// <summary>
    /// Called when a choice is made - triggers Phase 5
    /// </summary>
    public void OnChoiceMade()
    {
        StartCoroutine(Phase5_TransitionToCompletion());
    }
    
    private IEnumerator Phase5_TransitionToCompletion()
    {
        Debug.Log("Phase 5: Transitioning to map completion");
        
        // Fade out the dialogue image
        fullscreenDialogueImage.DOFade(0f, 0.8f).SetEase(Ease.InQuad);
        
        // Slide choices panel down
        float screenHeight = Screen.height * 1.5f;
        choicesPanel.DOAnchorPos(new Vector2(0, -screenHeight), 0.8f).SetEase(Ease.InQuad);
        
        yield return new WaitForSeconds(0.8f);
        
        // Hide this UI using canvas group alpha
        canvasGroup.DOFade(0f, 0.3f).SetEase(Ease.OutQuad).OnComplete(() => {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        });
        
        // Trigger map completion panel
        TriggerMapCompletion();
    }
    
    private void TriggerMapCompletion()
    {
        // Force close any normal dialogue panels first
        ForceCloseNormalDialoguePanels();
        
        // Get current map and final stats
        var mapManager = MapManager.Instance;
        if (mapManager == null || mapManager.GetCurrentMap() == null)
        {
            Debug.LogError("MapManager or current map not found!");
            return;
        }
        
        var currentMap = mapManager.GetCurrentMap().Value;
        var finalStats = GameManager.Instance.GetMapValues(currentMap);
        
        Debug.Log($"Triggering map completion for {currentMap}");
        
        // Trigger the map completion panel
        MapCompletionPanelUI.TriggerMapCompletion(currentMap, finalStats);
    }
    
    private void ForceCloseNormalDialoguePanels()
    {
        // Find and force close ChoiceSelectionUI
        var choiceSelectionUI = FindFirstObjectByType<ChoiceSelectionUI>();
        if (choiceSelectionUI != null)
        {
            Debug.Log("Force closing ChoiceSelectionUI after global dialogue");
            choiceSelectionUI.ForceCloseDialoguePanel();
        }
    }
    
    /// <summary>
    /// Force close the UI (for cleanup or scene changes)
    /// </summary>
    public void ForceClose()
    {
        StopAllCoroutines();
        isTypewriterActive = false;
        isAnimating = false;
        
        // Hide using canvas group alpha
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}

