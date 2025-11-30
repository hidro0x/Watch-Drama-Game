using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main Menu Controller - Handles start button and language switching
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button turkishButton;
    [SerializeField] private Button englishButton;
    
    [Header("Intro Settings")]
    [SerializeField] private IntroController introController; // Reference to IntroController component
    [SerializeField] private GameObject mainMenuPanel; // Optional: Main menu panel to hide when starting
    
    [Header("Language Settings")]
    [SerializeField] private string defaultLanguage = "Turkish";
    
    private string currentLanguage = "Turkish";
    
    void Start()
    {
        // Initialize language from PlayerPrefs
        currentLanguage = PlayerPrefs.GetString("CurrentLanguage", defaultLanguage);
        
        // Validate language
        if (currentLanguage != "Turkish" && currentLanguage != "English")
        {
            currentLanguage = defaultLanguage;
        }
        
        // Setup button listeners
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        
        if (turkishButton != null)
        {
            turkishButton.onClick.AddListener(OnTurkishButtonClicked);
        }
        
        if (englishButton != null)
        {
            englishButton.onClick.AddListener(OnEnglishButtonClicked);
        }
        
        // Initialize localization system if available
        InitializeLocalization();
        
        // Update language display (highlight active button)
        UpdateLanguageDisplay();
    }
    
    private void InitializeLocalization()
    {
        // Try to initialize SimpleRuntimeLocalizer if it exists (for UI text)
        if (SimpleRuntimeLocalizer.Instance != null)
        {
            SimpleRuntimeLocalizer.Instance.SetLanguage(currentLanguage);
            // Subscribe to language changes to update UI
            SimpleRuntimeLocalizer.Instance.OnLanguageChanged += OnLocalizerLanguageChanged;
        }
        
        // Also initialize DialogueLocalizationManager (for dialogue text)
        if (DialogueLocalizationManager.Instance != null)
        {
            DialogueLocalizationManager.Instance.SetLanguage(currentLanguage);
        }
    }
    
    private void OnLocalizerLanguageChanged(string newLanguage)
    {
        // Sync current language with localizer
        currentLanguage = newLanguage;
        UpdateLanguageDisplay();
    }
    
    private void OnStartButtonClicked()
    {
        // Play button click sound if AudioManager is available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SoundEffectType.ButtonClick);
        }
        
        // Hide main menu panel if assigned
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        
        // Start the intro sequence
        if (introController != null)
        {
            // Ensure the IntroController GameObject is active
            if (!introController.gameObject.activeSelf)
            {
                introController.gameObject.SetActive(true);
            }
            // Start the intro sequence
            introController.StartIntro();
        }
        else
        {
            Debug.LogError("IntroController is not assigned in MainMenuController! Please assign the IntroController component.");
        }
    }
    
    private void OnTurkishButtonClicked()
    {
        SetLanguage("Turkish");
    }
    
    private void OnEnglishButtonClicked()
    {
        SetLanguage("English");
    }
    
    private void SetLanguage(string language)
    {
        // Don't switch if already on this language
        if (currentLanguage == language)
        {
            return;
        }
        
        // Play button click sound if AudioManager is available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SoundEffectType.ButtonClick);
        }
        
        // Set new language
        currentLanguage = language;
        
        // Save language preference
        PlayerPrefs.SetString("CurrentLanguage", currentLanguage);
        PlayerPrefs.Save();
        
        // Update language display (highlight active button)
        UpdateLanguageDisplay();
        
        // Update UI localization system
        if (SimpleRuntimeLocalizer.Instance != null)
        {
            SimpleRuntimeLocalizer.Instance.SetLanguage(currentLanguage);
        }
        
        // Update Dialogue localization system
        if (DialogueLocalizationManager.Instance != null)
        {
            DialogueLocalizationManager.Instance.SetLanguage(currentLanguage);
            Debug.Log($"🌍 DialogueLocalizationManager language set to: {currentLanguage}");
        }
        else
        {
            Debug.LogWarning("DialogueLocalizationManager.Instance is null! Make sure it exists in the scene.");
        }
    }
    
    private void UpdateLanguageDisplay()
    {
        // Highlight active language button, dim inactive one
        if (turkishButton != null)
        {
            var turkishColors = turkishButton.colors;
            turkishColors.normalColor = currentLanguage == "Turkish" ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
            turkishButton.colors = turkishColors;
            turkishButton.interactable = true; // Always interactable
        }
        
        if (englishButton != null)
        {
            var englishColors = englishButton.colors;
            englishColors.normalColor = currentLanguage == "English" ? Color.white : new Color(0.7f, 0.7f, 0.7f, 1f);
            englishButton.colors = englishColors;
            englishButton.interactable = true; // Always interactable
        }
    }
    
    /// <summary>
    /// Get current language
    /// </summary>
    public string GetCurrentLanguage()
    {
        return currentLanguage;
    }
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
        
        if (turkishButton != null)
        {
            turkishButton.onClick.RemoveListener(OnTurkishButtonClicked);
        }
        
        if (englishButton != null)
        {
            englishButton.onClick.RemoveListener(OnEnglishButtonClicked);
        }
        
        // Unsubscribe from localization events
        if (SimpleRuntimeLocalizer.Instance != null)
        {
            SimpleRuntimeLocalizer.Instance.OnLanguageChanged -= OnLocalizerLanguageChanged;
        }
    }
}

