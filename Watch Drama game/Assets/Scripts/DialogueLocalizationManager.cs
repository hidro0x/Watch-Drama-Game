using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Runtime manager for dialogue localization
/// Handles language switching and provides localized dialogue text
/// </summary>
public class DialogueLocalizationManager : MonoBehaviour
{
    private static DialogueLocalizationManager _instance;
    public static DialogueLocalizationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DialogueLocalizationManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("DialogueLocalizationManager");
                    _instance = go.AddComponent<DialogueLocalizationManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    
    [Header("Localization Data")]
    [SerializeField] private DialogueLocalizationData localizationData;
    
    [Header("Settings")]
    [SerializeField] private string defaultLanguage = "English";
    [SerializeField] private string currentLanguage = "English";
    
    /// <summary>
    /// Event fired when language changes
    /// </summary>
    public event Action<string> OnLanguageChanged;
    
    /// <summary>
    /// Current active language
    /// </summary>
    public string CurrentLanguage => currentLanguage;
    
    /// <summary>
    /// Default/fallback language
    /// </summary>
    public string DefaultLanguage => defaultLanguage;
    
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Load saved language preference (use same key as MainMenuController)
        currentLanguage = PlayerPrefs.GetString("CurrentLanguage", defaultLanguage);
    }
    
    /// <summary>
    /// Set the localization data asset
    /// </summary>
    public void SetLocalizationData(DialogueLocalizationData data)
    {
        localizationData = data;
    }
    
    /// <summary>
    /// Change the current language
    /// </summary>
    public void SetLanguage(string language)
    {
        if (currentLanguage == language) return;
        
        currentLanguage = language;
        PlayerPrefs.SetString("CurrentLanguage", language); // Use same key as MainMenuController
        PlayerPrefs.Save();
        
        Debug.Log($"🌍 DialogueLocalizationManager: Language changed to: {language}");
        OnLanguageChanged?.Invoke(language);
    }
    
    /// <summary>
    /// Get available languages from the localization data
    /// </summary>
    public List<string> GetAvailableLanguages()
    {
        if (localizationData == null) return new List<string> { defaultLanguage };
        return localizationData.GetAvailableLanguages();
    }
    
    /// <summary>
    /// Get localized dialogue node (modifies the node with localized text)
    /// </summary>
    public DialogueNode GetLocalizedDialogue(DialogueNode originalNode)
    {
        if (localizationData == null || originalNode == null) return originalNode;
        if (string.IsNullOrEmpty(originalNode.id)) return originalNode;
        
        // Try to get localization for current language
        var localized = localizationData.GetLocalizedDialogue(originalNode.id, currentLanguage);
        
        // If not found and not default language, try default
        if (localized == null && currentLanguage != defaultLanguage)
        {
            localized = localizationData.GetLocalizedDialogue(originalNode.id, defaultLanguage);
        }
        
        // If still not found, return original
        if (localized == null) return originalNode;
        
        // Create a copy with localized text
        DialogueNode localizedNode = new DialogueNode
        {
            id = originalNode.id,
            name = !string.IsNullOrEmpty(localized.name) ? localized.name : originalNode.name,
            text = !string.IsNullOrEmpty(localized.text) ? localized.text : originalNode.text,
            sprite = originalNode.sprite,
            backgroundSprite = originalNode.backgroundSprite,
            isGlobalDialogue = originalNode.isGlobalDialogue,
            choices = new List<DialogueChoice>()
        };
        
        // Localize choices
        if (originalNode.choices != null)
        {
            for (int i = 0; i < originalNode.choices.Count; i++)
            {
                var originalChoice = originalNode.choices[i];
                DialogueChoice localizedChoice = new DialogueChoice
                {
                    text = (localized.choiceTexts != null && i < localized.choiceTexts.Count && !string.IsNullOrEmpty(localized.choiceTexts[i]))
                        ? localized.choiceTexts[i]
                        : originalChoice.text,
                    trustChange = originalChoice.trustChange,
                    faithChange = originalChoice.faithChange,
                    hostilityChange = originalChoice.hostilityChange,
                    nextNodeId = originalChoice.nextNodeId,
                    isGlobalChoice = originalChoice.isGlobalChoice
                };
                localizedNode.choices.Add(localizedChoice);
            }
        }
        
        return localizedNode;
    }
    
    /// <summary>
    /// Get localized global dialogue node
    /// </summary>
    public GlobalDialogueNode GetLocalizedGlobalDialogue(GlobalDialogueNode originalNode)
    {
        if (localizationData == null || originalNode == null) return originalNode;
        if (string.IsNullOrEmpty(originalNode.id)) return originalNode;
        
        // Try to get localization for current language
        var localized = localizationData.GetLocalizedGlobalDialogue(originalNode.id, currentLanguage);
        
        // If not found and not default language, try default
        if (localized == null && currentLanguage != defaultLanguage)
        {
            localized = localizationData.GetLocalizedGlobalDialogue(originalNode.id, defaultLanguage);
        }
        
        // If still not found, return original
        if (localized == null) return originalNode;
        
        // Create a copy with localized text
        GlobalDialogueNode localizedNode = new GlobalDialogueNode
        {
            id = originalNode.id,
            name = !string.IsNullOrEmpty(localized.name) ? localized.name : originalNode.name,
            text = !string.IsNullOrEmpty(localized.text) ? localized.text : originalNode.text,
            sprite = originalNode.sprite,
            choices = new List<GlobalDialogueChoice>()
        };
        
        // Localize choices (keep effects, just change text)
        if (originalNode.choices != null)
        {
            for (int i = 0; i < originalNode.choices.Count; i++)
            {
                var originalChoice = originalNode.choices[i];
                GlobalDialogueChoice localizedChoice = new GlobalDialogueChoice
                {
                    text = (localized.choiceTexts != null && i < localized.choiceTexts.Count && !string.IsNullOrEmpty(localized.choiceTexts[i]))
                        ? localized.choiceTexts[i]
                        : originalChoice.text,
                    globalEffects = originalChoice.globalEffects // Keep original effects
                };
                localizedNode.choices.Add(localizedChoice);
            }
        }
        
        return localizedNode;
    }
    
    /// <summary>
    /// Check if localization exists for a dialogue
    /// </summary>
    public bool HasLocalization(string dialogueId, string language = null)
    {
        if (localizationData == null) return false;
        language = language ?? currentLanguage;
        return localizationData.GetLocalizedDialogue(dialogueId, language) != null;
    }
}

