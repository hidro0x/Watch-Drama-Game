using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Language Switcher UI Component - Can be added to main menu or any scene
/// Provides a dropdown or button-based language selection interface
/// </summary>
public class LanguageSwitcherUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private Button turkishButton;
    [SerializeField] private Button englishButton;
    
    [Header("Display Options")]
    [SerializeField] private bool useDropdown = true;
    [SerializeField] private bool showLanguageNames = true; // Show "Türkçe" / "English" or flags
    
    [Header("Current Language Display")]
    [SerializeField] private TextMeshProUGUI currentLanguageText;
    
    private const string LANGUAGE_PREF_KEY = "SelectedLanguage";
    
    private void Start()
    {
        InitializeLanguageSwitcher();
    }
    
    private void OnEnable()
    {
        // Subscribe to language change events
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
        }
    }
    
    private void OnDisable()
    {
        // Unsubscribe from language change events
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
        }
    }
    
    /// <summary>
    /// Initialize the language switcher UI
    /// </summary>
    private void InitializeLanguageSwitcher()
    {
        // Load saved language preference
        Language savedLanguage = LoadLanguagePreference();
        
        // Set current language in LocalizationManager if it exists
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.SetLanguage(savedLanguage);
        }
        
        // Setup UI based on selected mode
        if (useDropdown && languageDropdown != null)
        {
            SetupDropdown();
        }
        else
        {
            SetupButtons();
        }
        
        UpdateCurrentLanguageDisplay();
    }
    
    /// <summary>
    /// Setup dropdown-based language selection
    /// </summary>
    private void SetupDropdown()
    {
        if (languageDropdown == null)
        {
            Debug.LogWarning("Language dropdown is not assigned!");
            return;
        }
        
        // Clear existing options
        languageDropdown.ClearOptions();
        
        // Add language options
        var options = new System.Collections.Generic.List<string>();
        options.Add(GetLanguageDisplayName(Language.Turkish));
        options.Add(GetLanguageDisplayName(Language.English));
        
        languageDropdown.AddOptions(options);
        
        // Set current selection
        Language currentLang = LocalizationManager.Instance != null 
            ? LocalizationManager.Instance.GetCurrentLanguage() 
            : LoadLanguagePreference();
        languageDropdown.value = (int)currentLang;
        
        // Add listener
        languageDropdown.onValueChanged.RemoveAllListeners();
        languageDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }
    
    /// <summary>
    /// Setup button-based language selection
    /// </summary>
    private void SetupButtons()
    {
        if (turkishButton != null)
        {
            turkishButton.onClick.RemoveAllListeners();
            turkishButton.onClick.AddListener(() => SwitchLanguage(Language.Turkish));
        }
        
        if (englishButton != null)
        {
            englishButton.onClick.RemoveAllListeners();
            englishButton.onClick.AddListener(() => SwitchLanguage(Language.English));
        }
        
        // Update button states to show current selection
        UpdateButtonStates();
    }
    
    /// <summary>
    /// Handle dropdown value change
    /// </summary>
    private void OnDropdownValueChanged(int value)
    {
        Language selectedLanguage = (Language)value;
        SwitchLanguage(selectedLanguage);
    }
    
    /// <summary>
    /// Switch to the specified language
    /// </summary>
    public void SwitchLanguage(Language language)
    {
        if (LocalizationManager.Instance == null)
        {
            Debug.LogWarning("LocalizationManager instance not found! Language switch may not work properly.");
            return;
        }
        
        LocalizationManager.Instance.SetLanguage(language);
        SaveLanguagePreference(language);
        UpdateCurrentLanguageDisplay();
        UpdateButtonStates();
        
        Debug.Log($"Language switched to: {language}");
    }
    
    /// <summary>
    /// Update button visual states to show which language is active
    /// </summary>
    private void UpdateButtonStates()
    {
        if (!useDropdown)
        {
            Language currentLang = LocalizationManager.Instance != null 
                ? LocalizationManager.Instance.GetCurrentLanguage() 
                : LoadLanguagePreference();
            
            // Update Turkish button
            if (turkishButton != null)
            {
                var colors = turkishButton.colors;
                colors.normalColor = currentLang == Language.Turkish 
                    ? new Color(0.8f, 0.9f, 1f, 1f) // Highlighted color
                    : Color.white;
                turkishButton.colors = colors;
            }
            
            // Update English button
            if (englishButton != null)
            {
                var colors = englishButton.colors;
                colors.normalColor = currentLang == Language.English 
                    ? new Color(0.8f, 0.9f, 1f, 1f) // Highlighted color
                    : Color.white;
                englishButton.colors = colors;
            }
        }
    }
    
    /// <summary>
    /// Update the current language display text
    /// </summary>
    private void UpdateCurrentLanguageDisplay()
    {
        if (currentLanguageText != null)
        {
            Language currentLang = LocalizationManager.Instance != null 
                ? LocalizationManager.Instance.GetCurrentLanguage() 
                : LoadLanguagePreference();
            
            currentLanguageText.text = $"Current Language: {GetLanguageDisplayName(currentLang)}";
        }
    }
    
    /// <summary>
    /// Get display name for a language
    /// </summary>
    private string GetLanguageDisplayName(Language language)
    {
        if (showLanguageNames)
        {
            return language switch
            {
                Language.Turkish => "Türkçe",
                Language.English => "English",
                _ => language.ToString()
            };
        }
        else
        {
            return language.ToString();
        }
    }
    
    /// <summary>
    /// Handle language change event from LocalizationManager
    /// </summary>
    private void OnLanguageChanged(Language newLanguage)
    {
        // Update dropdown if using dropdown mode
        if (useDropdown && languageDropdown != null)
        {
            languageDropdown.value = (int)newLanguage;
        }
        
        UpdateCurrentLanguageDisplay();
        UpdateButtonStates();
    }
    
    /// <summary>
    /// Save language preference to PlayerPrefs
    /// </summary>
    private void SaveLanguagePreference(Language language)
    {
        PlayerPrefs.SetInt(LANGUAGE_PREF_KEY, (int)language);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Load language preference from PlayerPrefs
    /// </summary>
    private Language LoadLanguagePreference()
    {
        if (PlayerPrefs.HasKey(LANGUAGE_PREF_KEY))
        {
            int langValue = PlayerPrefs.GetInt(LANGUAGE_PREF_KEY);
            if (System.Enum.IsDefined(typeof(Language), langValue))
            {
                return (Language)langValue;
            }
        }
        
        // Default to Turkish if no preference saved
        return Language.Turkish;
    }
}

