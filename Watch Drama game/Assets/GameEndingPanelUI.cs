using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public class GameEndingPanelUI : MonoBehaviour
{
    [Title("UI Elements")]
    [SerializeField] private Image backgroundPanel;
    [SerializeField] private Image endingIcon;
    [SerializeField] private TextMeshProUGUI endingTitleText;
    [SerializeField] private TextMeshProUGUI endingDescriptionText;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Title("Victory Stats")]
    [SerializeField] private TextMeshProUGUI finalStatsText;
    [SerializeField] private Slider overallTrustSlider;
    [SerializeField] private Slider overallFaithSlider;
    [SerializeField] private Slider overallHostilitySlider;
    
    [Title("Completed Maps")]
    [SerializeField] private Transform completedMapsContainer;
    [SerializeField] private GameObject mapSummaryPrefab;
    
    [Title("Buttons")]
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI mainMenuButtonText;
    [SerializeField] private TextMeshProUGUI restartButtonText;
    [SerializeField] private TextMeshProUGUI continueButtonText;
    
    [Title("Animation Settings")]
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private float elementDelay = 0.4f;
    [SerializeField] private float sliderAnimationDuration = 1.5f;
    [SerializeField] private float buttonDelay = 0.8f;
    
    [Title("Ending Data")]
    [SerializeField] private MapDataCollection mapDataCollection;
    
    private EndingScenario currentEnding;
    private bool isAnimating = false;
    private List<GameObject> mapSummaryObjects = new List<GameObject>();

    void Start()
    {
        // Ensure canvas group exists
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // Initially hide all elements
        SetupInitialState();
        
        // Setup button listeners
        SetupButtonListeners();
        
        // Subscribe to game completion event
        GameManager.OnGameFinished += OnGameFinished;
    }
    
    private void OnDestroy()
    {
        GameManager.OnGameFinished -= OnGameFinished;
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
        
        // Hide ending icon
        if (endingIcon != null)
        {
            var iconColor = endingIcon.color;
            iconColor.a = 0f;
            endingIcon.color = iconColor;
        }
        
        // Hide text elements
        if (endingTitleText != null)
        {
            endingTitleText.alpha = 0f;
        }
        
        if (endingDescriptionText != null)
        {
            endingDescriptionText.alpha = 0f;
        }
        
        if (finalStatsText != null)
        {
            finalStatsText.alpha = 0f;
        }
        
        // Reset sliders to 0
        if (overallTrustSlider != null)
        {
            overallTrustSlider.value = 0f;
        }
        if (overallFaithSlider != null)
        {
            overallFaithSlider.value = 0f;
        }
        if (overallHostilitySlider != null)
        {
            overallHostilitySlider.value = 0f;
        }
        
        // Hide buttons
        HideButton(mainMenuButton, mainMenuButtonText);
        HideButton(restartButton, restartButtonText);
        HideButton(continueButton, continueButtonText);
        
        // Initially hide the panel using canvas group alpha
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    
    private void HideButton(Button button, TextMeshProUGUI buttonText)
    {
        if (button != null)
        {
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                var buttonColor = buttonImage.color;
                buttonColor.a = 0f;
                buttonImage.color = buttonColor;
            }
        }
        
        if (buttonText != null)
        {
            buttonText.alpha = 0f;
        }
    }

    private void SetupButtonListeners()
    {
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        }
        
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }
    }

    private void OnGameFinished(EndingScenario ending)
    {
        ShowGameEnding(ending);
    }

    /// <summary>
    /// Show the game ending panel with animation sequence
    /// </summary>
    /// <param name="ending">The ending scenario that was achieved</param>
    public void ShowGameEnding(EndingScenario ending)
    {
        if (isAnimating) return;
        
        this.currentEnding = ending;
        
        // Make sure the GameObject is active
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
        
        // Set canvas group to be ready for animation
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        // Start animation sequence
        StartCoroutine(PlayEndingAnimation());
    }

    /// <summary>
    /// Hide the game ending panel immediately
    /// </summary>
    public void HideGameEnding()
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

    private IEnumerator PlayEndingAnimation()
    {
        isAnimating = true;
        
        // Setup content
        SetupEndingContent();
        
        // 1. Fade in background
        if (backgroundPanel != null)
        {
            backgroundPanel.DOFade(0.95f, fadeInDuration).SetEase(Ease.OutQuad);
        }
        
        yield return new WaitForSeconds(elementDelay);
        
        // 2. Fade in ending icon
        if (endingIcon != null)
        {
            endingIcon.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
        }
        
        yield return new WaitForSeconds(elementDelay);
        
        // 3. Fade in ending title
        if (endingTitleText != null)
        {
            endingTitleText.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
        }
        
        yield return new WaitForSeconds(elementDelay);
        
        // 4. Fade in ending description
        if (endingDescriptionText != null)
        {
            endingDescriptionText.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
        }
        
        yield return new WaitForSeconds(elementDelay);
        
        // 5. Animate overall stats sliders
        yield return StartCoroutine(AnimateOverallSliders());
        
        yield return new WaitForSeconds(elementDelay);
        
        // 6. Show completed maps summary
        yield return StartCoroutine(ShowCompletedMapsSummary());
        
        yield return new WaitForSeconds(buttonDelay);
        
        // 7. Fade in buttons
        yield return StartCoroutine(ShowButtons());
        
        isAnimating = false;
    }

    private IEnumerator AnimateOverallSliders()
    {
        // Calculate overall stats from all maps
        var overallStats = CalculateOverallStats();
        
        // Animate trust slider
        if (overallTrustSlider != null)
        {
            float targetValue = overallStats.Trust / 100f;
            overallTrustSlider.DOValue(targetValue, sliderAnimationDuration).SetEase(Ease.OutQuad);
        }
        
        // Animate faith slider
        if (overallFaithSlider != null)
        {
            float targetValue = overallStats.Faith / 100f;
            overallFaithSlider.DOValue(targetValue, sliderAnimationDuration).SetEase(Ease.OutQuad);
        }
        
        // Animate hostility slider
        if (overallHostilitySlider != null)
        {
            float targetValue = overallStats.Hostility / 100f;
            overallHostilitySlider.DOValue(targetValue, sliderAnimationDuration).SetEase(Ease.OutQuad);
        }
        
        yield return new WaitForSeconds(sliderAnimationDuration);
    }

    private IEnumerator ShowCompletedMapsSummary()
    {
        if (completedMapsContainer == null || mapSummaryPrefab == null)
            yield break;
        
        // Clear existing map summaries
        foreach (var obj in mapSummaryObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        mapSummaryObjects.Clear();
        
        // Get all map types and create summaries
        var allMaps = GetAllMapTypes();
        
        foreach (var mapType in allMaps)
        {
            var mapStats = GameManager.Instance.GetMapValues(mapType);
            var mapData = GetMapData(mapType);
            
            // Create map summary object
            var summaryObj = Instantiate(mapSummaryPrefab, completedMapsContainer);
            mapSummaryObjects.Add(summaryObj);
            
            // Setup map summary content
            SetupMapSummary(summaryObj, mapType, mapStats, mapData);
            
            // Animate in
            var summaryCanvasGroup = summaryObj.GetComponent<CanvasGroup>();
            if (summaryCanvasGroup != null)
            {
                summaryCanvasGroup.alpha = 0f;
                summaryCanvasGroup.DOFade(1f, fadeInDuration * 0.5f).SetEase(Ease.OutQuad);
            }
            
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator ShowButtons()
    {
        // Show main menu button
        if (mainMenuButton != null)
        {
            ShowButton(mainMenuButton, mainMenuButtonText);
            yield return new WaitForSeconds(0.3f);
        }
        
        // Show restart button
        if (restartButton != null)
        {
            ShowButton(restartButton, restartButtonText);
            yield return new WaitForSeconds(0.3f);
        }
        
        // Show continue button (if applicable)
        if (continueButton != null)
        {
            ShowButton(continueButton, continueButtonText);
        }
    }
    
    private void ShowButton(Button button, TextMeshProUGUI buttonText)
    {
        if (button != null)
        {
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
            }
        }
        
        if (buttonText != null)
        {
            buttonText.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
        }
    }

    private void SetupEndingContent()
    {
        // Setup ending title and description based on ending scenario
        var endingData = GetEndingData(currentEnding);
        
        if (endingTitleText != null)
        {
            endingTitleText.text = endingData.title;
            endingTitleText.color = endingData.titleColor;
        }
        
        if (endingDescriptionText != null)
        {
            endingDescriptionText.text = endingData.description;
        }
        
        if (endingIcon != null)
        {
            endingIcon.sprite = endingData.endingImage;
            endingIcon.color = endingData.titleColor;
            endingIcon.gameObject.SetActive(endingData.endingImage != null);
        }
        
        if (finalStatsText != null)
        {
            var overallStats = CalculateOverallStats();
            finalStatsText.text = $"Final Stats: Trust {overallStats.Trust} | Faith {overallStats.Faith} | Hostility {overallStats.Hostility}";
        }
    }

    private void SetupMapSummary(GameObject summaryObj, MapType mapType, MapValues stats, MapData mapData)
    {
        // Setup map summary UI elements
        var mapNameText = summaryObj.transform.Find("MapNameText")?.GetComponent<TextMeshProUGUI>();
        var mapIconImage = summaryObj.transform.Find("MapIcon")?.GetComponent<Image>();
        var trustText = summaryObj.transform.Find("TrustText")?.GetComponent<TextMeshProUGUI>();
        var faithText = summaryObj.transform.Find("FaithText")?.GetComponent<TextMeshProUGUI>();
        var hostilityText = summaryObj.transform.Find("HostilityText")?.GetComponent<TextMeshProUGUI>();
        
        if (mapNameText != null)
        {
            mapNameText.text = mapData.displayName;
            mapNameText.color = mapData.textColor;
        }
        
        if (mapIconImage != null)
        {
            mapIconImage.sprite = mapData.mapIcon;
            mapIconImage.color = mapData.mapColor;
        }
        
        if (trustText != null)
            trustText.text = stats.Trust.ToString();
        if (faithText != null)
            faithText.text = stats.Faith.ToString();
        if (hostilityText != null)
            hostilityText.text = stats.Hostility.ToString();
    }

    private MapValues CalculateOverallStats()
    {
        var allMaps = GetAllMapTypes();
        int totalTrust = 0, totalFaith = 0, totalHostility = 0;
        int mapCount = allMaps.Count;
        
        foreach (var mapType in allMaps)
        {
            var stats = GameManager.Instance.GetMapValues(mapType);
            totalTrust += stats.Trust;
            totalFaith += stats.Faith;
            totalHostility += stats.Hostility;
        }
        
        return new MapValues(
            mapCount > 0 ? totalTrust / mapCount : 0,
            mapCount > 0 ? totalFaith / mapCount : 0,
            mapCount > 0 ? totalHostility / mapCount : 0
        );
    }

    private List<MapType> GetAllMapTypes()
    {
        return new List<MapType>
        {
            MapType.Astrahil,
            MapType.Agnari,
            MapType.Solarya,
            MapType.Theon,
            MapType.Varnan
        };
    }

    private MapData GetMapData(MapType mapType)
    {
        if (mapDataCollection != null)
        {
            return mapDataCollection.GetMapData(mapType);
        }
        else
        {
            return CreateFallbackMapData(mapType);
        }
    }
    
    private MapData CreateFallbackMapData(MapType mapType)
    {
        return new MapData
        {
            mapType = mapType,
            displayName = mapType.ToString(),
            mapIcon = null,
            description = $"Default description for {mapType}",
            mapColor = Color.white,
            textColor = Color.white
        };
    }

    private EndingData GetEndingData(EndingScenario ending)
    {
        switch (ending)
        {
            case EndingScenario.TrustVictory:
                return new EndingData
                {
                    scenario = EndingScenario.TrustVictory,
                    title = "🏆 Trust Victory!",
                    description = "You have achieved victory through trust and diplomacy. All nations have learned to trust each other.",
                    endingImage = null,
                    titleColor = Color.green,
                    textColor = Color.white,
                    backgroundColor = new Color(0.2f, 0.6f, 1.0f, 1.0f)
                };
            case EndingScenario.FaithVictory:
                return new EndingData
                {
                    scenario = EndingScenario.FaithVictory,
                    title = "🌟 Faith Victory!",
                    description = "You have achieved victory through faith and spiritual unity. All nations are united in faith.",
                    endingImage = null,
                    titleColor = Color.cyan,
                    textColor = Color.white,
                    backgroundColor = new Color(1.0f, 0.8f, 0.2f, 1.0f)
                };
            case EndingScenario.HostilityVictory:
                return new EndingData
                {
                    scenario = EndingScenario.HostilityVictory,
                    title = "⚔️ Hostility Victory!",
                    description = "You have achieved victory through strength and conquest. All nations bow to your power.",
                    endingImage = null,
                    titleColor = Color.red,
                    textColor = Color.white,
                    backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1.0f)
                };
            case EndingScenario.BalancedVictory:
                return new EndingData
                {
                    scenario = EndingScenario.BalancedVictory,
                    title = "⚖️ Balanced Victory!",
                    description = "You have achieved victory through balanced leadership. All nations are in perfect harmony.",
                    endingImage = null,
                    titleColor = Color.yellow,
                    textColor = Color.white,
                    backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f)
                };
            case EndingScenario.AllMapsCompleted:
                return new EndingData
                {
                    scenario = EndingScenario.AllMapsCompleted,
                    title = "🗺️ All Maps Completed!",
                    description = "You have successfully completed all maps. Your journey is complete!",
                    endingImage = null,
                    titleColor = Color.white,
                    textColor = Color.white,
                    backgroundColor = new Color(0.3f, 0.7f, 0.3f, 1.0f)
                };
            default:
                return new EndingData
                {
                    scenario = EndingScenario.Custom,
                    title = "🎮 Game Complete!",
                    description = "Your adventure has come to an end.",
                    endingImage = null,
                    titleColor = Color.white,
                    textColor = Color.white,
                    backgroundColor = Color.black
                };
        }
    }

    private void OnMainMenuButtonClicked()
    {
        Debug.Log("Main Menu button clicked");
        // TODO: Load main menu scene
    }

    private void OnRestartButtonClicked()
    {
        Debug.Log("🔄 Restart button clicked - Resetting to initial game state");
        
        // Hide ending panel first
        HideGameEnding();
        
        // Reset all game data to initial state
        ResetAllGameDataToInitial();
        
        Debug.Log("🎮 Game restarted - All data reset to initial state");
    }

    private void OnContinueButtonClicked()
    {
        Debug.Log("Continue button clicked");
        // TODO: Continue to next game mode or chapter
        HideGameEnding();
    }
    
    /// <summary>
    /// Reset all game data to initial state (oyun başlangıcındaki haline döndür)
    /// </summary>
    private void ResetAllGameDataToInitial()
    {
        if (GameManager.Instance == null) return;
        
        // GameManager'daki StartNewGame metodunu çağır - bu tüm değerleri oyun başlangıcındaki haline döndürür
        GameManager.Instance.StartNewGame();
        
        // MapManager'ı da resetle
        if (MapManager.Instance != null)
        {
            MapManager.Instance.ResetAllMaps();
        }
        
        // UI'yi güncelle
        GameManager.Instance.ForceUpdateBarUI();
        
        Debug.Log("🔧 All game data reset to initial state - Ready for new game!");
    }

    /// <summary>
    /// Public method to trigger game ending (can be called from other scripts)
    /// </summary>
    public static void TriggerGameEnding(EndingScenario ending)
    {
        GameEndingPanelUI panel = FindFirstObjectByType<GameEndingPanelUI>();
        if (panel != null)
        {
            panel.ShowGameEnding(ending);
        }
        else
        {
            Debug.LogError("GameEndingPanelUI not found in scene!");
        }
    }

    /// <summary>
    /// Public method to hide game ending panel (can be called from other scripts)
    /// </summary>
    public static void HideGameEndingPanel()
    {
        GameEndingPanelUI panel = FindFirstObjectByType<GameEndingPanelUI>();
        if (panel != null)
        {
            panel.HideGameEnding();
        }
        else
        {
            Debug.LogWarning("GameEndingPanelUI not found in scene!");
        }
    }
}

