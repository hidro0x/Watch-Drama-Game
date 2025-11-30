using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// Language enum for supported languages
/// </summary>
public enum Language
{
    Turkish = 0,
    English = 1,
    // Add more languages as needed
    // Spanish = 2,
    // French = 3,
    // German = 4,
}

/// <summary>
/// Localization Manager - Singleton that handles language switching and text retrieval
/// </summary>
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }
    
    [Header("Language Settings")]
    [SerializeField] private Language currentLanguage = Language.Turkish;
    
    [Header("Localization Data")]
    [SerializeField] private Dictionary<string, LocalizedDialogueData> localizedDialogues = new Dictionary<string, LocalizedDialogueData>();
    
    // Events
    public System.Action<Language> OnLanguageChanged;
    
    private const string LANGUAGE_PREF_KEY = "SelectedLanguage";
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Load saved language preference
        LoadLanguagePreference();
        
        // Load current language on start
        LoadLocalizedDialogues(currentLanguage);
    }
    
    /// <summary>
    /// Load language preference from PlayerPrefs
    /// </summary>
    private void LoadLanguagePreference()
    {
        if (PlayerPrefs.HasKey(LANGUAGE_PREF_KEY))
        {
            int langValue = PlayerPrefs.GetInt(LANGUAGE_PREF_KEY);
            if (System.Enum.IsDefined(typeof(Language), langValue))
            {
                currentLanguage = (Language)langValue;
            }
        }
    }
    
    /// <summary>
    /// Save language preference to PlayerPrefs
    /// </summary>
    private void SaveLanguagePreference()
    {
        PlayerPrefs.SetInt(LANGUAGE_PREF_KEY, (int)currentLanguage);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Get the current language
    /// </summary>
    public Language GetCurrentLanguage()
    {
        return currentLanguage;
    }
    
    /// <summary>
    /// Set the current language and reload dialogues
    /// </summary>
    public void SetLanguage(Language language)
    {
        if (currentLanguage == language) return;
        
        currentLanguage = language;
        SaveLanguagePreference();
        LoadLocalizedDialogues(language);
        
        OnLanguageChanged?.Invoke(language);
        Debug.Log($"Language changed to: {language}");
    }
    
    /// <summary>
    /// Load all localized dialogue JSON files for the specified language
    /// </summary>
    public void LoadLocalizedDialogues(Language language)
    {
        localizedDialogues.Clear();
        
        string languageFolder = GetLanguageFolder(language);
        TextAsset[] jsonFiles = null;
        
        // For Turkish, try root folder first (backward compatibility)
        if (language == Language.Turkish)
        {
            // Try root folder first
            jsonFiles = Resources.LoadAll<TextAsset>("NewDialogues");
            
            // If no files in root, try tr folder
            if (jsonFiles == null || jsonFiles.Length == 0)
            {
                jsonFiles = Resources.LoadAll<TextAsset>($"NewDialogues/{languageFolder}");
            }
        }
        else
        {
            // For other languages, use language-specific folder
            jsonFiles = Resources.LoadAll<TextAsset>($"NewDialogues/{languageFolder}");
            
            // Fallback to Turkish if no files found
            if (jsonFiles == null || jsonFiles.Length == 0)
            {
                Debug.LogWarning($"No localized files found for language: {language} in Resources/NewDialogues/{languageFolder}");
                Debug.LogWarning($"Falling back to Turkish (default language)");
                LoadLocalizedDialogues(Language.Turkish);
                return;
            }
        }
        
        if (jsonFiles == null || jsonFiles.Length == 0)
        {
            Debug.LogWarning($"No dialogue files found for language: {language}");
            return;
        }
        
        foreach (var jsonFile in jsonFiles)
        {
            // Skip files in subfolders when loading from root (for Turkish)
            if (language == Language.Turkish && jsonFile.name.Contains("/"))
            {
                continue;
            }
            
            if (jsonFile.name.EndsWith(".json") || jsonFile.name.EndsWith("_full") || jsonFile.name.Contains("dialogues"))
            {
                LoadDialogueFile(jsonFile.text, jsonFile.name);
            }
        }
        
        Debug.Log($"Loaded {localizedDialogues.Count} localized dialogues for language: {language}");
    }
    
    /// <summary>
    /// Load a single dialogue JSON file
    /// </summary>
    private void LoadDialogueFile(string jsonContent, string fileName)
    {
        try
        {
            // Check if it's an array format
            if (jsonContent.Trim().StartsWith("["))
            {
                var dialogueArray = JsonConvert.DeserializeObject<List<JsonDialogueData>>(jsonContent);
                foreach (var dialogue in dialogueArray)
                {
                    StoreLocalizedDialogue(dialogue);
                }
            }
            // Check if it's a map-specific format
            else if (jsonContent.Contains("\"mapType\""))
            {
                var mapDialogueData = JsonConvert.DeserializeObject<MapSpecificDialogueData>(jsonContent);
                foreach (var dialogue in mapDialogueData.dialogues)
                {
                    StoreLocalizedDialogue(dialogue);
                }
            }
            // Single dialogue format
            else
            {
                var dialogue = JsonConvert.DeserializeObject<JsonDialogueData>(jsonContent);
                StoreLocalizedDialogue(dialogue);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading dialogue file {fileName}: {e.Message}");
        }
    }
    
    /// <summary>
    /// Store a localized dialogue in the dictionary
    /// </summary>
    private void StoreLocalizedDialogue(JsonDialogueData dialogue)
    {
        if (string.IsNullOrEmpty(dialogue.id))
        {
            Debug.LogWarning("Dialogue has no ID, skipping...");
            return;
        }
        
        localizedDialogues[dialogue.id] = new LocalizedDialogueData
        {
            id = dialogue.id,
            name = dialogue.name ?? dialogue.speaker ?? "",
            speaker = dialogue.speaker ?? dialogue.name ?? "",
            text = dialogue.text ?? "",
            options = dialogue.options ?? new List<JsonDialogueOption>()
        };
    }
    
    /// <summary>
    /// Get localized text for a dialogue ID
    /// </summary>
    public string GetLocalizedText(string dialogueId)
    {
        if (localizedDialogues.TryGetValue(dialogueId, out var dialogue))
        {
            return dialogue.text;
        }
        
        Debug.LogWarning($"Localized text not found for dialogue ID: {dialogueId}");
        return $"<Missing: {dialogueId}>";
    }
    
    /// <summary>
    /// Get localized name/speaker for a dialogue ID
    /// </summary>
    public string GetLocalizedName(string dialogueId)
    {
        if (localizedDialogues.TryGetValue(dialogueId, out var dialogue))
        {
            return dialogue.name;
        }
        
        Debug.LogWarning($"Localized name not found for dialogue ID: {dialogueId}");
        return $"<Missing: {dialogueId}>";
    }
    
    /// <summary>
    /// Get localized options for a dialogue ID
    /// </summary>
    public List<JsonDialogueOption> GetLocalizedOptions(string dialogueId)
    {
        if (localizedDialogues.TryGetValue(dialogueId, out var dialogue))
        {
            return dialogue.options;
        }
        
        Debug.LogWarning($"Localized options not found for dialogue ID: {dialogueId}");
        return new List<JsonDialogueOption>();
    }
    
    /// <summary>
    /// Get complete localized dialogue data
    /// </summary>
    public LocalizedDialogueData GetLocalizedDialogue(string dialogueId)
    {
        if (localizedDialogues.TryGetValue(dialogueId, out var dialogue))
        {
            return dialogue;
        }
        
        Debug.LogWarning($"Localized dialogue not found for dialogue ID: {dialogueId}");
        return null;
    }
    
    /// <summary>
    /// Apply localization to a DialogueNode
    /// </summary>
    public void ApplyLocalizationToDialogueNode(DialogueNode node)
    {
        if (node == null || string.IsNullOrEmpty(node.id))
        {
            return;
        }
        
        var localizedData = GetLocalizedDialogue(node.id);
        if (localizedData != null)
        {
            node.name = localizedData.name;
            node.text = localizedData.text;
            
            // Update choices
            if (node.choices != null && localizedData.options != null)
            {
                for (int i = 0; i < node.choices.Count && i < localizedData.options.Count; i++)
                {
                    node.choices[i].text = localizedData.options[i].text;
                }
            }
        }
    }
    
    /// <summary>
    /// Get the folder name for a language
    /// </summary>
    private string GetLanguageFolder(Language language)
    {
        switch (language)
        {
            case Language.Turkish:
                return "tr"; // Turkish
            case Language.English:
                return "en"; // English
            default:
                return "tr"; // Default to Turkish
        }
    }
    
    /// <summary>
    /// Check if a dialogue ID exists in the current language
    /// </summary>
    public bool HasLocalization(string dialogueId)
    {
        return localizedDialogues.ContainsKey(dialogueId);
    }
}

/// <summary>
/// Localized dialogue data structure
/// </summary>
[System.Serializable]
public class LocalizedDialogueData
{
    public string id;
    public string name;
    public string speaker;
    public string text;
    public List<JsonDialogueOption> options;
}

/// <summary>
/// Map-specific dialogue data structure for JSON parsing
/// </summary>
[System.Serializable]
public class MapSpecificDialogueData
{
    public string mapType;
    public List<JsonDialogueData> dialogues;
}

