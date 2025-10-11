using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class MapCompletionPanelUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private Image mapIcon;
    [SerializeField] private TextMeshProUGUI traveledText;
    [SerializeField] private TextMeshProUGUI mapNameText;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Map Data")]
    [SerializeField] private MapDataCollection mapDataCollection;
    
    [Header("Stat Sliders")]
    [SerializeField] private Slider trustSlider;
    [SerializeField] private Slider faithSlider;
    [SerializeField] private Slider hostilitySlider;

    
    [Header("Buttons")]
    [SerializeField] private Button okayButton;
    [SerializeField] private TextMeshProUGUI okayButtonText;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.8f;
    [SerializeField] private float elementDelay = 0.3f;
    [SerializeField] private float sliderAnimationDuration = 1.2f;
    [SerializeField] private float buttonDelay = 0.5f;
    
    private MapType completedMapType;
    private MapValues finalStats;
    private bool isAnimating = false;

    void Start()
    {
        // Ensure canvas group exists
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // Initially hide all elements
        SetupInitialState();
        
        // Setup button listener
        if (okayButton != null)
        {
            okayButton.onClick.AddListener(OnOkayButtonClicked);
        }
    }

    private void SetupInitialState()
    {
        // Hide background panel
        if (backgroundPanel != null)
        {
            var bgColor = backgroundPanel.color;
            bgColor.a = 0f;
            backgroundPanel.color = bgColor;
        }
        
        // Hide map icon
        if (mapIcon != null)
        {
            var iconColor = mapIcon.color;
            iconColor.a = 0f;
            mapIcon.color = iconColor;
        }
        
        // Hide text elements
        if (traveledText != null)
        {
            traveledText.alpha = 0f;
        }
        
        if (mapNameText != null)
        {
            mapNameText.alpha = 0f;
        }
        
        // Reset sliders to 0
        if (trustSlider != null)
        {
            trustSlider.value = 0f;
        }
        if (faithSlider != null)
        {
            faithSlider.value = 0f;
        }
        if (hostilitySlider != null)
        {
            hostilitySlider.value = 0f;
        }
        

        
        // Hide button
        if (okayButton != null)
        {
            var buttonImage = okayButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                var buttonColor = buttonImage.color;
                buttonColor.a = 0f;
                buttonImage.color = buttonColor;
            }
        }
        
        if (okayButtonText != null)
        {
            okayButtonText.alpha = 0f;
        }
        
        // Initially hide the panel using canvas group alpha
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Show the map completion panel with animation sequence
    /// </summary>
    /// <param name="mapType">The completed map type</param>
    /// <param name="finalStats">Final statistics for the map</param>
    public void ShowMapCompletion(MapType mapType, MapValues finalStats)
    {
        if (isAnimating) return;
        
        this.completedMapType = mapType;
        this.finalStats = finalStats;
        
        // Make sure the GameObject is active
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
        
        // Set canvas group to be ready for animation
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        // Start animation sequence
        StartCoroutine(PlayCompletionAnimation());
    }

    /// <summary>
    /// Hide the map completion panel immediately
    /// </summary>
    public void HideMapCompletion()
    {
        if (isAnimating) return;
        
        // Stop any running animations
        StopAllCoroutines();
        
        // Hide the panel immediately using canvas group alpha
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        // Reset state
        isAnimating = false;
    }

    /// <summary>
    /// Check if the map completion panel is currently active
    /// </summary>
    public bool IsMapCompletionActive()
    {
        return canvasGroup.alpha > 0f;
    }

    private IEnumerator PlayCompletionAnimation()
    {
        isAnimating = true;
        
        // Setup content
        SetupMapContent();
        
        // 1. Fade in background
        if (backgroundPanel != null)
        {
            backgroundPanel.DOFade(0.9f, fadeInDuration).SetEase(Ease.OutQuad);
        }
        
        yield return new WaitForSeconds(elementDelay);
        
        // 2. Fade in map icon
        if (mapIcon != null)
        {
            mapIcon.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
        }
        
        yield return new WaitForSeconds(elementDelay);
        
        // 3. Fade in "You have traveled" text
        if (traveledText != null)
        {
            traveledText.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
        }
        
        yield return new WaitForSeconds(elementDelay);
        
        // 4. Fade in map name
        if (mapNameText != null)
        {
            mapNameText.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
        }
        
        yield return new WaitForSeconds(elementDelay);
        
        // 5. Animate sliders and value texts simultaneously
        yield return StartCoroutine(AnimateSliders());
        
        yield return new WaitForSeconds(buttonDelay);
        
        // 6. Fade in okay button
        if (okayButton != null)
        {
            var buttonImage = okayButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
            }
        }
        
        if (okayButtonText != null)
        {
            okayButtonText.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
        }
        
        isAnimating = false;
    }

    private IEnumerator AnimateSliders()
    {
        // Animate trust slider
        if (trustSlider != null)
        {
            float targetValue = finalStats.Trust / 100f;
            trustSlider.DOValue(targetValue, sliderAnimationDuration).SetEase(Ease.OutQuad);
        }
        
        // Animate faith slider
        if (faithSlider != null)
        {
            float targetValue = finalStats.Faith / 100f;
            faithSlider.DOValue(targetValue, sliderAnimationDuration).SetEase(Ease.OutQuad);
        }
        
        // Animate hostility slider
        if (hostilitySlider != null)
        {
            float targetValue = finalStats.Hostility / 100f;
            hostilitySlider.DOValue(targetValue, sliderAnimationDuration).SetEase(Ease.OutQuad);
        }
        
        // Sliders animate without any text labels or values
        
        yield return new WaitForSeconds(sliderAnimationDuration);
    }


    private void SetupMapContent()
    {
        // Get map data from collection
        MapData mapData = GetMapData(completedMapType);
        
        // Set map name
        if (mapNameText != null)
        {
            mapNameText.text = mapData.displayName;
            mapNameText.color = mapData.textColor;
        }
        
        // Set map icon
        if (mapIcon != null)
        {
            mapIcon.sprite = mapData.mapIcon;
            mapIcon.color = mapData.mapColor;
            
            // Hide icon if no sprite is assigned
            mapIcon.gameObject.SetActive(mapData.mapIcon != null);
        }
        
        // Keep normal dialogue background - don't change background color
    }
    
    private MapData GetMapData(MapType mapType)
    {
        if (mapDataCollection != null)
        {
            return mapDataCollection.GetMapData(mapType);
        }
        else
        {
            Debug.LogWarning("MapDataCollection not assigned! Using default map data.");
            return CreateFallbackMapData(mapType);
        }
    }
    
    private MapData CreateFallbackMapData(MapType mapType)
    {
        return new MapData
        {
            mapType = mapType,
            displayName = GetMapDisplayName(mapType),
            mapIcon = null,
            description = $"Default description for {mapType}",
            mapColor = Color.white,
            textColor = Color.white
        };
    }

    private string GetMapDisplayName(MapType mapType)
    {
        // Convert enum to readable map name
        switch (mapType)
        {
            case MapType.Astrahil:
                return "Astrahil";
            case MapType.Agnari:
                return "Agnari";
            case MapType.Solarya:
                return "Solarya";
            case MapType.Theon:
                return "Theon";
            case MapType.Varnan:
                return "Varnan";
            default:
                return mapType.ToString();
        }
    }

    private void OnOkayButtonClicked()
    {
        // Hide panel with fade out animation
        StartCoroutine(HidePanel());
    }

    private IEnumerator HidePanel()
    {
        // Fade out all elements
        if (backgroundPanel != null)
        {
            backgroundPanel.DOFade(0f, 0.5f);
        }
        
        if (mapIcon != null)
        {
            mapIcon.DOFade(0f, 0.5f);
        }
        
        if (traveledText != null)
        {
            traveledText.DOFade(0f, 0.5f);
        }
        
        if (mapNameText != null)
        {
            mapNameText.DOFade(0f, 0.5f);
        }
        
        if (okayButton != null)
        {
            var buttonImage = okayButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.DOFade(0f, 0.5f);
            }
        }
        
        if (okayButtonText != null)
        {
            okayButtonText.DOFade(0f, 0.5f);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Hide panel using canvas group alpha
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        isAnimating = false;
        
        // Notify that map completion is finished
        OnMapCompletionFinished();
    }

    private void OnMapCompletionFinished()
    {
        // Force close any remaining dialogue panels
        ForceCloseAllDialoguePanels();
        
        // You can add logic here for what happens after map completion
        // For example: Load next map, show ending panel, etc.
        Debug.Log($"Map completion finished for {completedMapType}");
        
        // Example: Trigger next map or ending sequence
        // MapManager.Instance.CompleteCurrentMap();
    }
    
    private void ForceCloseAllDialoguePanels()
    {
        // Force close ChoiceSelectionUI
        var choiceSelectionUI = FindFirstObjectByType<ChoiceSelectionUI>();
        if (choiceSelectionUI != null)
        {
            Debug.Log("Force closing ChoiceSelectionUI after map completion");
            choiceSelectionUI.ForceCloseDialoguePanel();
        }
        
        // Force close GlobalDialogueUI if it's still active
        var globalDialogueUI = FindFirstObjectByType<GlobalDialogueUI>();
        if (globalDialogueUI != null && globalDialogueUI.gameObject.activeInHierarchy)
        {
            Debug.Log("Force closing GlobalDialogueUI after map completion");
            globalDialogueUI.ForceClose();
        }
    }

    /// <summary>
    /// Public method to trigger map completion (can be called from other scripts)
    /// </summary>
    public static void TriggerMapCompletion(MapType mapType, MapValues finalStats)
    {
        MapCompletionPanelUI panel = FindFirstObjectByType<MapCompletionPanelUI>();
        if (panel != null)
        {
            panel.ShowMapCompletion(mapType, finalStats);
        }
        else
        {
            Debug.LogError("MapCompletionPanelUI not found in scene!");
        }
    }

    /// <summary>
    /// Public method to hide map completion panel (can be called from other scripts)
    /// </summary>
    public static void HideMapCompletionPanel()
    {
        MapCompletionPanelUI panel = FindFirstObjectByType<MapCompletionPanelUI>();
        if (panel != null)
        {
            panel.HideMapCompletion();
        }
        else
        {
            Debug.LogWarning("MapCompletionPanelUI not found in scene!");
        }
    }
    
    /// <summary>
    /// Set the map data collection for this panel
    /// </summary>
    public void SetMapDataCollection(MapDataCollection collection)
    {
        mapDataCollection = collection;
    }
    
    /// <summary>
    /// Get the current map data collection
    /// </summary>
    public MapDataCollection GetMapDataCollection()
    {
        return mapDataCollection;
    }
}
