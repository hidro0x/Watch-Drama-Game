using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

/// <summary>
/// Stores localized text for a single dialogue
/// </summary>
[System.Serializable]
public class LocalizedDialogueText
{
    public string name;
    public string text;
    public List<string> choiceTexts = new List<string>();
}

/// <summary>
/// Stores all localizations for dialogues
/// Key: DialogueID, Value: Dictionary<Language, LocalizedText>
/// </summary>
[CreateAssetMenu(menuName = "Kahin/Dialogue Localization Data")]
public class DialogueLocalizationData : SerializedScriptableObject
{
    [Title("Dialogue Localizations")]
    [DictionaryDrawerSettings(KeyLabel = "Dialogue ID", ValueLabel = "Translations")]
    public Dictionary<string, Dictionary<string, LocalizedDialogueText>> dialogueLocalizations = new Dictionary<string, Dictionary<string, LocalizedDialogueText>>();
    
    [Title("Global Dialogue Localizations")]
    [DictionaryDrawerSettings(KeyLabel = "Global Dialogue ID", ValueLabel = "Translations")]
    public Dictionary<string, Dictionary<string, LocalizedDialogueText>> globalDialogueLocalizations = new Dictionary<string, Dictionary<string, LocalizedDialogueText>>();
    
    /// <summary>
    /// Get localized text for a dialogue
    /// </summary>
    public LocalizedDialogueText GetLocalizedDialogue(string dialogueId, string language)
    {
        if (dialogueLocalizations.TryGetValue(dialogueId, out var translations))
        {
            if (translations.TryGetValue(language, out var localizedText))
            {
                return localizedText;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Get localized text for a global dialogue
    /// </summary>
    public LocalizedDialogueText GetLocalizedGlobalDialogue(string dialogueId, string language)
    {
        if (globalDialogueLocalizations.TryGetValue(dialogueId, out var translations))
        {
            if (translations.TryGetValue(language, out var localizedText))
            {
                return localizedText;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Add or update a localization
    /// </summary>
    public void SetLocalizedDialogue(string dialogueId, string language, LocalizedDialogueText localizedText)
    {
        if (!dialogueLocalizations.ContainsKey(dialogueId))
        {
            dialogueLocalizations[dialogueId] = new Dictionary<string, LocalizedDialogueText>();
        }
        dialogueLocalizations[dialogueId][language] = localizedText;
    }
    
    /// <summary>
    /// Add or update a global dialogue localization
    /// </summary>
    public void SetLocalizedGlobalDialogue(string dialogueId, string language, LocalizedDialogueText localizedText)
    {
        if (!globalDialogueLocalizations.ContainsKey(dialogueId))
        {
            globalDialogueLocalizations[dialogueId] = new Dictionary<string, LocalizedDialogueText>();
        }
        globalDialogueLocalizations[dialogueId][language] = localizedText;
    }
    
    /// <summary>
    /// Get all available languages
    /// </summary>
    public List<string> GetAvailableLanguages()
    {
        HashSet<string> languages = new HashSet<string>();
        
        foreach (var dialogueTranslations in dialogueLocalizations.Values)
        {
            foreach (var lang in dialogueTranslations.Keys)
            {
                languages.Add(lang);
            }
        }
        
        foreach (var dialogueTranslations in globalDialogueLocalizations.Values)
        {
            foreach (var lang in dialogueTranslations.Keys)
            {
                languages.Add(lang);
            }
        }
        
        return new List<string>(languages);
    }
    
    /// <summary>
    /// Clear all localizations
    /// </summary>
    public void ClearAll()
    {
        dialogueLocalizations.Clear();
        globalDialogueLocalizations.Clear();
    }
    
    /// <summary>
    /// Get dialogue count
    /// </summary>
    public int GetDialogueCount() => dialogueLocalizations.Count;
    
    /// <summary>
    /// Get global dialogue count
    /// </summary>
    public int GetGlobalDialogueCount() => globalDialogueLocalizations.Count;
}

