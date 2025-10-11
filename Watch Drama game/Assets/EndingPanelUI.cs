using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public enum EndingScenario
{
    TrustVictory,
    FaithVictory,
    HostilityVictory,
    BalancedVictory,
    AllMapsCompleted,
    TrustDefeat,
    FaithDefeat,
    HostilityDefeat,
    Custom
}

[System.Serializable]
public class EndingData
{
    [Title("Ending Information")]
    public EndingScenario scenario;
    public string title;
    [TextArea(4, 8)]
    public string description;
    
    [Title("Visual Elements")]
    [PreviewField(100)]
    public Sprite endingImage;
    public Color backgroundColor = Color.black;
    public Color textColor = Color.white;
    public Color titleColor = Color.white;
    
    [Title("Audio")]
    public AudioClip endingMusic;
    public AudioClip endingSoundEffect;
}

public class EndingPanelUI : MonoBehaviour
{
    [Title("UI Components", "Main UI elements for the ending panel")]
    [SerializeField] private Image endingImage;
    [SerializeField] private TextMeshProUGUI endingTitle;
    [SerializeField] private TextMeshProUGUI endingText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    
    [Title("Scenario Buttons", "Buttons for different ending scenarios")]
    [SerializeField] private Transform scenarioButtonsParent;
    [SerializeField] private GameObject scenarioButtonPrefab;
    
    [Title("Animation Settings", "Timing and animation parameters")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float fadeOutDuration = 0.8f;
    [SerializeField] private float typewriterSpeed = 0.03f;
    [SerializeField] private float buttonDelay = 0.5f;
    [SerializeField] private float scenarioButtonSpacing = 10f;
    
    [Title("Ending Data Collection", "All possible ending scenarios")]
    [SerializeField] private List<EndingData> endingDataCollection = new List<EndingData>();
    
    private string fullText;
    private bool isAnimating = false;
    private bool isShowingScenarioButtons = false;
    private EndingData currentEndingData;

    private void Start()
    {
        SetupCanvasGroup();
        SetupButtonListeners();
        InitializeEndingData();
        HidePanel();
    }
    
    private void SetupCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }
    
    private void SetupButtonListeners()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
    }
    
    private void InitializeEndingData()
    {
        if (endingDataCollection.Count == 0)
        {
            CreateDefaultEndingData();
        }
    }
    
    [Button("Create Default Ending Data")]
    private void CreateDefaultEndingData()
    {
        endingDataCollection.Clear();
        
        // Trust Victory
        endingDataCollection.Add(new EndingData
        {
            scenario = EndingScenario.TrustVictory,
            title = "Trust Triumphs",
            description = "Through your wise decisions and diplomatic approach, you have built unbreakable bonds of trust across the lands. The kingdoms now stand united in peace and cooperation.",
            backgroundColor = new Color(0.2f, 0.6f, 1.0f, 1.0f),
            textColor = Color.white,
            titleColor = new Color(0.8f, 0.9f, 1.0f, 1.0f)
        });
        
        // Faith Victory
        endingDataCollection.Add(new EndingData
        {
            scenario = EndingScenario.FaithVictory,
            title = "Faith Restored",
            description = "Your unwavering dedication to spiritual values has restored faith across the realm. The people now believe in a brighter future and the power of unity.",
            backgroundColor = new Color(1.0f, 0.8f, 0.2f, 1.0f),
            textColor = Color.black,
            titleColor = new Color(0.9f, 0.7f, 0.1f, 1.0f)
        });
        
        // Hostility Victory
        endingDataCollection.Add(new EndingData
        {
            scenario = EndingScenario.HostilityVictory,
            title = "Power Through Strength",
            description = "Through decisive action and firm leadership, you have eliminated all threats to the realm. The lands now bow to your authority and strength.",
            backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1.0f),
            textColor = Color.white,
            titleColor = new Color(1.0f, 0.4f, 0.4f, 1.0f)
        });
        
        // Balanced Victory
        endingDataCollection.Add(new EndingData
        {
            scenario = EndingScenario.BalancedVictory,
            title = "Perfect Balance",
            description = "You have achieved the impossible - a perfect balance of trust, faith, and strength. The realm now thrives under your enlightened leadership.",
            backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1.0f),
            textColor = Color.white,
            titleColor = new Color(0.8f, 0.8f, 0.8f, 1.0f)
        });
        
        // All Maps Completed
        endingDataCollection.Add(new EndingData
        {
            scenario = EndingScenario.AllMapsCompleted,
            title = "Journey Complete",
            description = "You have successfully navigated all the challenges across every kingdom. Your wisdom and leadership have brought peace to the entire realm.",
            backgroundColor = new Color(0.3f, 0.7f, 0.3f, 1.0f),
            textColor = Color.white,
            titleColor = new Color(0.6f, 1.0f, 0.6f, 1.0f)
        });
    }

    [Title("Public Methods", "Methods to show different ending scenarios")]
    public void ShowEnding(EndingScenario scenario)
    {
        if (isAnimating) return;
        
        EndingData endingData = GetEndingData(scenario);
        if (endingData == null)
        {
            Debug.LogError($"No ending data found for scenario: {scenario}");
            return;
        }
        
        ShowEnding(endingData);
    }
    
    public void ShowEnding(EndingData endingData)
    {
        if (isAnimating) return;
        
        isAnimating = true;
        currentEndingData = endingData;
        fullText = endingData.description;
        
        // Setup UI elements
        SetupEndingUI(endingData);
        
        // Show panel and start animation
        gameObject.SetActive(true);
        StartCoroutine(AnimateEnding());
    }
    
    public void ShowEnding(Sprite sprite, string text)
    {
        // Legacy method for backward compatibility
        EndingData legacyEnding = new EndingData
        {
            scenario = EndingScenario.Custom,
            title = "Game Complete",
            description = text,
            endingImage = sprite,
            backgroundColor = Color.black,
            textColor = Color.white,
            titleColor = Color.white
        };
        
        ShowEnding(legacyEnding);
    }
    
    [Button("Show Scenario Buttons")]
    public void ShowScenarioButtons()
    {
        if (isAnimating || isShowingScenarioButtons) return;
        
        isShowingScenarioButtons = true;
        CreateScenarioButtons();
        StartCoroutine(AnimateScenarioButtons());
    }
    
    private void SetupEndingUI(EndingData endingData)
    {
        // Set background color
        if (GetComponent<Image>() != null)
        {
            GetComponent<Image>().color = endingData.backgroundColor;
        }
        
        // Set ending image
        if (endingImage != null)
        {
            endingImage.sprite = endingData.endingImage;
            endingImage.gameObject.SetActive(endingData.endingImage != null);
        }
        
        // Set title
        if (endingTitle != null)
        {
            endingTitle.text = endingData.title;
            endingTitle.color = endingData.titleColor;
        }
        
        // Clear description text
        if (endingText != null)
        {
            endingText.text = "";
            endingText.color = endingData.textColor;
        }
        
        // Hide action buttons initially
        HideActionButtons();
    }
    
    private void HideActionButtons()
    {
        if (continueButton != null) continueButton.gameObject.SetActive(false);
        if (restartButton != null) restartButton.gameObject.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.gameObject.SetActive(false);
    }
    
    private EndingData GetEndingData(EndingScenario scenario)
    {
        return endingDataCollection.Find(data => data.scenario == scenario);
    }

    private IEnumerator AnimateEnding()
    {
        // 1. Alpha fade-in animation
        canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        yield return new WaitForSeconds(fadeInDuration);
        
        // 2. Typewriter effect for description
        yield return StartCoroutine(TypewriterEffect());
        
        // 3. Show action buttons with delay
        yield return new WaitForSeconds(buttonDelay);
        yield return StartCoroutine(ShowActionButtons());
        
        isAnimating = false;
    }
    
    private IEnumerator ShowActionButtons()
    {
        // Show continue button first
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.transform.localScale = Vector3.zero;
            continueButton.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.2f);
        }
        
        // Show restart button
        if (restartButton != null)
        {
            restartButton.gameObject.SetActive(true);
            restartButton.transform.localScale = Vector3.zero;
            restartButton.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.2f);
        }
        
        // Show main menu button
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(true);
            mainMenuButton.transform.localScale = Vector3.zero;
            mainMenuButton.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }
    }
    
    private IEnumerator AnimateScenarioButtons()
    {
        // Clear existing buttons
        ClearScenarioButtons();
        
        // Create new buttons
        CreateScenarioButtons();
        
        // Animate buttons in
        Transform[] buttons = scenarioButtonsParent.GetComponentsInChildren<Transform>();
        for (int i = 1; i < buttons.Length; i++) // Skip parent
        {
            if (buttons[i].GetComponent<Button>() != null)
            {
                buttons[i].localScale = Vector3.zero;
                buttons[i].DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetDelay(i * 0.1f);
            }
        }
        
        yield return new WaitForSeconds(buttons.Length * 0.1f + 0.3f);
    }
    
    private void CreateScenarioButtons()
    {
        if (scenarioButtonsParent == null || scenarioButtonPrefab == null) return;
        
        foreach (EndingData endingData in endingDataCollection)
        {
            GameObject buttonObj = Instantiate(scenarioButtonPrefab, scenarioButtonsParent);
            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (buttonText != null)
            {
                buttonText.text = endingData.title;
                buttonText.color = endingData.titleColor;
            }
            
            if (button != null)
            {
                EndingData capturedData = endingData; // Capture for lambda
                button.onClick.AddListener(() => OnScenarioButtonClicked(capturedData));
            }
        }
    }
    
    private void ClearScenarioButtons()
    {
        if (scenarioButtonsParent == null) return;
        
        for (int i = scenarioButtonsParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(scenarioButtonsParent.GetChild(i).gameObject);
        }
    }
    
    private IEnumerator TypewriterEffect()
    {
        string currentText = "";
        
        for (int i = 0; i < fullText.Length; i++)
        {
            currentText += fullText[i];
            if (endingText != null)
            {
                endingText.text = currentText;
            }
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
    
    [Title("Button Event Handlers", "Handle button clicks")]
    private void OnScenarioButtonClicked(EndingData endingData)
    {
        Debug.Log($"Scenario button clicked: {endingData.scenario}");
        ShowEnding(endingData);
    }
    
    private void OnContinueButtonClicked()
    {
        Debug.Log("Continue button clicked");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinishGameWithScenario(currentEndingData?.scenario ?? EndingScenario.Custom);
        }
        HidePanel();
    }
    
    private void OnRestartButtonClicked()
    {
        Debug.Log("Restart button clicked");
        
        // Stop all animations and coroutines
        StopAllCoroutines();
        
        // Reset game state
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
        }
        
        // Reset map manager
        if (MapManager.Instance != null)
        {
            MapManager.Instance.ResetAllMaps();
        }
        
        // Load scene immediately
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void OnMainMenuButtonClicked()
    {
        Debug.Log("Main menu button clicked");
        HidePanel();
        // Load main menu scene (adjust scene name as needed)
        SceneManager.LoadScene("MainMenu");
    }
    
    [Title("Utility Methods", "Helper methods for panel management")]
    public void HidePanel()
    {
        StopAllCoroutines();
        isAnimating = false;
        isShowingScenarioButtons = false;
        
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0f, fadeOutDuration).SetEase(Ease.InQuad).OnComplete(() =>
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
            });
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    
    public void ForceShow()
    {
        StopAllCoroutines();
        isAnimating = false;
        isShowingScenarioButtons = false;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        gameObject.SetActive(true);
    }
    
    public bool IsShowing()
    {
        return gameObject.activeInHierarchy && (canvasGroup == null || canvasGroup.alpha > 0f);
    }
    
    [Title("Debug Methods", "Methods for testing and debugging")]
    [Button("Test Trust Victory")]
    private void TestTrustVictory()
    {
        ShowEnding(EndingScenario.TrustVictory);
    }
    
    [Button("Test Faith Victory")]
    private void TestFaithVictory()
    {
        ShowEnding(EndingScenario.FaithVictory);
    }
    
    [Button("Test Hostility Victory")]
    private void TestHostilityVictory()
    {
        ShowEnding(EndingScenario.HostilityVictory);
    }
    
    [Button("Test All Maps Completed")]
    private void TestAllMapsCompleted()
    {
        ShowEnding(EndingScenario.AllMapsCompleted);
    }
    
    [Button("Hide Panel")]
    private void TestHidePanel()
    {
        HidePanel();
    }
}
