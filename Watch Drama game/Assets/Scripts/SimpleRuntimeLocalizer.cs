using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Simple and robust runtime localization system
/// Loads JSON files from Resources/Localization folder
/// </summary>
public class SimpleRuntimeLocalizer : MonoBehaviour
{
    private static SimpleRuntimeLocalizer _instance;
    public static SimpleRuntimeLocalizer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SimpleRuntimeLocalizer>();
            }
            return _instance;
        }
    }

    [Header("Settings")]
    [SerializeField] private string defaultLanguage = "Turkish";
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Current language
    private string currentLanguage;
    
    // Localization data: key -> value
    private Dictionary<string, string> currentLocalization = new Dictionary<string, string>();
    
    // All loaded localizations: language -> (key -> value)
    private Dictionary<string, Dictionary<string, string>> allLocalizations = new Dictionary<string, Dictionary<string, string>>();
    
    // Event when language changes
    public event Action<string> OnLanguageChanged;
    
    // Track all localizable texts
    private List<LocalizableText> registeredTexts = new List<LocalizableText>();
    
    // Is initialized
    private bool isInitialized = false;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized) return;
        
        // Load saved language or use default
        currentLanguage = PlayerPrefs.GetString("CurrentLanguage", defaultLanguage);
        
        Debug.Log($"[Localizer] ========== INITIALIZING ==========");
        Debug.Log($"[Localizer] Current language: {currentLanguage}");
        Debug.Log($"[Localizer] Default language: {defaultLanguage}");
        
        // Load all language files
        LoadAllLanguages();
        
        Debug.Log($"[Localizer] Total languages loaded: {allLocalizations.Count}");
        foreach (var lang in allLocalizations)
        {
            Debug.Log($"[Localizer]   - {lang.Key}: {lang.Value.Count} keys");
        }
        
        // Set current language data
        if (allLocalizations.ContainsKey(currentLanguage))
        {
            currentLocalization = allLocalizations[currentLanguage];
            Debug.Log($"[Localizer] Set current localization to '{currentLanguage}' with {currentLocalization.Count} keys");
        }
        else
        {
            Debug.LogError($"[Localizer] Language '{currentLanguage}' NOT FOUND! Available: {string.Join(", ", allLocalizations.Keys)}");
            
            // Try to use first available language
            if (allLocalizations.Count > 0)
            {
                foreach (var lang in allLocalizations)
                {
                    currentLanguage = lang.Key;
                    currentLocalization = lang.Value;
                    Debug.Log($"[Localizer] Falling back to '{currentLanguage}'");
                    break;
                }
            }
        }
        
        isInitialized = true;
        Debug.Log($"[Localizer] ========== INIT COMPLETE ==========");
    }

    private void LoadAllLanguages()
    {
        // Load Turkish
        LoadLanguageFile("Turkish", "turkish_localization");
        
        // Load English
        LoadLanguageFile("English", "english_localization");
    }

    private void LoadLanguageFile(string language, string fileName)
    {
        // Try to load from Resources/Localization/
        string path = $"Localization/{fileName}";
        Debug.Log($"[Localizer] Attempting to load: Resources/{path}");
        
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        
        if (textAsset == null)
        {
            Debug.LogError($"[Localizer] FAILED to load: Resources/{path} - File not found!");
            return;
        }
        
        Debug.Log($"[Localizer] SUCCESS loaded: Resources/{path} ({textAsset.text.Length} chars)");
        Debug.Log($"[Localizer] Content preview: {textAsset.text.Substring(0, Mathf.Min(200, textAsset.text.Length))}...");
        
        // Parse JSON manually (simple key-value pairs)
        var data = ParseSimpleJson(textAsset.text);
        
        if (data != null && data.Count > 0)
        {
            allLocalizations[language] = data;
            Debug.Log($"[Localizer] Parsed {data.Count} keys for '{language}'");
            
            // Show first few keys
            int count = 0;
            foreach (var kvp in data)
            {
                Debug.Log($"[Localizer]   {kvp.Key} = {kvp.Value}");
                if (++count >= 3) break;
            }
        }
        else
        {
            Debug.LogError($"[Localizer] FAILED to parse JSON for {language}! Data is null or empty.");
        }
    }

    /// <summary>
    /// Parse JSON using regex (more robust)
    /// </summary>
    private Dictionary<string, string> ParseSimpleJson(string json)
    {
        var result = new Dictionary<string, string>();
        
        try
        {
            // Use regex to find all "key": "value" pairs
            var regex = new System.Text.RegularExpressions.Regex("\"([^\"]+)\"\\s*:\\s*\"([^\"]*)\"|\"([^\"]+)\"\\s*:\\s*\"([^\"]*)\"");
            var matches = regex.Matches(json);
            
            Debug.Log($"[Localizer] Regex found {matches.Count} matches");
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                string key = match.Groups[1].Value;
                string value = match.Groups[2].Value;
                
                if (!string.IsNullOrEmpty(key))
                {
                    // Unescape common escape sequences
                    value = value.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
                    result[key] = value;
                }
            }
            
            // If regex didn't work, try line-by-line parsing
            if (result.Count == 0)
            {
                Debug.Log("[Localizer] Regex failed, trying line-by-line parsing...");
                string[] lines = json.Split('\n');
                foreach (string line in lines)
                {
                    string trimmed = line.Trim().TrimEnd(',');
                    if (trimmed.StartsWith("\"") && trimmed.Contains(":"))
                    {
                        int colonIdx = trimmed.IndexOf(':');
                        string keyPart = trimmed.Substring(0, colonIdx).Trim().Trim('"');
                        string valuePart = trimmed.Substring(colonIdx + 1).Trim().Trim('"');
                        
                        if (!string.IsNullOrEmpty(keyPart) && !keyPart.StartsWith("{") && !keyPart.StartsWith("}"))
                        {
                            result[keyPart] = valuePart;
                        }
                    }
                }
            }
            
            Debug.Log($"[Localizer] Final parse result: {result.Count} entries");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Localizer] JSON parse error: {e.Message}\n{e.StackTrace}");
        }
        
        return result;
    }

    /// <summary>
    /// Get localized text for a key
    /// </summary>
    public string GetText(string key)
    {
        if (!isInitialized)
        {
            Initialize();
        }
        
        if (string.IsNullOrEmpty(key))
        {
            return "";
        }
        
        Log($"GetText('{key}') - Language: {currentLanguage}, Keys in current: {(currentLocalization != null ? currentLocalization.Count : 0)}");
        
        // Try current language
        if (currentLocalization != null && currentLocalization.TryGetValue(key, out string value))
        {
            Log($"  Found: '{key}' -> '{value}'");
            return value;
        }
        
        // Try fallback to any available language
        foreach (var langPair in allLocalizations)
        {
            if (langPair.Value.TryGetValue(key, out string fallback))
            {
                Log($"  Fallback ({langPair.Key}): '{key}' -> '{fallback}'");
                return fallback;
            }
        }
        
        // Return key if not found
        LogWarning($"Key not found: '{key}' (Available keys: {(currentLocalization != null ? string.Join(", ", currentLocalization.Keys) : "none")})");
        return key;
    }

    /// <summary>
    /// Set current language
    /// </summary>
    public void SetLanguage(string language)
    {
        if (currentLanguage == language) return;
        
        Log($"Switching language from {currentLanguage} to {language}");
        
        currentLanguage = language;
        PlayerPrefs.SetString("CurrentLanguage", language);
        PlayerPrefs.Save();
        
        // Update current localization
        if (allLocalizations.ContainsKey(language))
        {
            currentLocalization = allLocalizations[language];
            Log($"Loaded {currentLocalization.Count} keys for {language}");
        }
        else
        {
            LogWarning($"Language '{language}' not found!");
        }
        
        // Refresh all registered texts
        RefreshAllTexts();
        
        // Fire event
        OnLanguageChanged?.Invoke(language);
    }

    /// <summary>
    /// Get current language
    /// </summary>
    public string GetCurrentLanguage()
    {
        return currentLanguage;
    }

    /// <summary>
    /// Register a LocalizableText component
    /// </summary>
    public void Register(LocalizableText text)
    {
        if (!registeredTexts.Contains(text))
        {
            registeredTexts.Add(text);
        }
    }

    /// <summary>
    /// Unregister a LocalizableText component
    /// </summary>
    public void Unregister(LocalizableText text)
    {
        registeredTexts.Remove(text);
    }

    /// <summary>
    /// Refresh all registered texts
    /// </summary>
    private void RefreshAllTexts()
    {
        // Clean up destroyed objects
        registeredTexts.RemoveAll(t => t == null);
        
        foreach (var text in registeredTexts)
        {
            text.UpdateText();
        }
        
        Log($"Refreshed {registeredTexts.Count} texts");
    }

    // Debug logging helpers
    private void Log(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[Localizer] {message}");
        }
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[Localizer] {message}");
    }
}
