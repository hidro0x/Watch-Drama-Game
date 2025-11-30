using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

/// <summary>
/// Turn sonunda kontrol edilecek condition'ları yöneten sistem
/// </summary>
public class TurnConditionSystem : MonoBehaviour
{
    [Title("Ending Scenario Mappings", "Map specific conditions to ending scenarios")]
    [InfoBox("These conditions will trigger specific ending scenarios when met")]
    
    [Title("Defeat Conditions")]
    [LabelWidth(120)]
    public EndingScenario trustZeroEnding = EndingScenario.TrustDefeat;
    [LabelWidth(120)]
    public EndingScenario faithZeroEnding = EndingScenario.FaithDefeat;
    
    [Title("Victory Conditions")]
    [LabelWidth(120)]
    public EndingScenario hostilityMaxEnding = EndingScenario.HostilityVictory;
    [LabelWidth(120)]
    public EndingScenario allMapsCompletedEnding = EndingScenario.AllMapsCompleted;
    
    [Title("Legacy Support", "Old sprite/text system (optional - for custom endings)")]
    [LabelWidth(120)]
    public Sprite trustZeroDialogueSprite;
    public string trustZeroDialogueText;
    
    [LabelWidth(120)]
    public Sprite faithZeroDialogueSprite;
    public string faithZeroDialogueText;
    
    [LabelWidth(120)]
    public Sprite hostilityMaxDialogueSprite;
    public string hostilityMaxDialogueText;
    
    [LabelWidth(120)]
    public Sprite allMapsCompletedDialogueSprite;
    public string allMapsCompletedDialogueText;
    
    
    private void Start()
    {
        // MapManager'dan turn event'ini dinle
        MapManager.OnMapSelected += OnMapSelected;
    }
    
    private void OnDestroy()
    {
        MapManager.OnMapSelected -= OnMapSelected;
    }
    
    private void OnMapSelected(MapType mapType)
    {
        // Harita değiştiğinde condition'ları güncelle
        UpdateConditionsForMap(mapType);
    }
    
    /// <summary>
    /// Turn sonunda çağrılır - tüm condition'ları kontrol eder
    /// </summary>
    public void CheckTurnConditions()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogWarning("MapManager.Instance null!");
            return;
        }
        
        // 1. Trust 0'a düşen ülkeleri kontrol et
        CheckTrustZeroConditions();
        
        // 2. Faith 0'a düşen ülkeleri kontrol et
        CheckFaithZeroConditions();
        
        // 3. Hostility 100'e çıkan ülkeleri kontrol et
        CheckHostilityMaxConditions();
    }
    
    /// <summary>
    /// Turn değiştiğinde çağrılır - sadece turn condition'larını kontrol eder
    /// </summary>
    public void CheckTurnConditionsOnTurnChange()
    {
        // Tüm haritaların tamamlanması kontrolü
        CheckAllMapsCompletedCondition();
    }
    
    private void CheckTrustZeroConditions()
    {
        
        foreach (MapType country in System.Enum.GetValues(typeof(MapType)))
        {
            int trustValue = GameManager.Instance.GetTrustForCountry(country);
            if (trustValue <= 0)
            {
                Debug.Log($"{country} ülkesinin Trust değeri 0'a düştü! Trust defeat ending tetikleniyor.");
                TriggerEndingScenario(trustZeroEnding, trustZeroDialogueSprite, trustZeroDialogueText);
                return; // Sadece ilk bulunanı tetikle
            }
        }
    }
    
    private void CheckFaithZeroConditions()
    {
        
        foreach (MapType country in System.Enum.GetValues(typeof(MapType)))
        {
            int faithValue = GameManager.Instance.GetFaithForCountry(country);
            if (faithValue <= 0)
            {
                Debug.Log($"{country} ülkesinin Faith değeri 0'a düştü! Faith defeat ending tetikleniyor.");
                TriggerEndingScenario(faithZeroEnding, faithZeroDialogueSprite, faithZeroDialogueText);
                return; // Sadece ilk bulunanı tetikle
            }
        }
    }
    
    private void CheckHostilityMaxConditions()
    {
        
        foreach (MapType country in System.Enum.GetValues(typeof(MapType)))
        {
            int hostilityValue = GameManager.Instance.GetHostilityForCountry(country);
            if (hostilityValue >= 100)
            {
                Debug.Log($"{country} ülkesinin Hostility değeri 100'e çıktı! Hostility victory ending tetikleniyor.");
                TriggerEndingScenario(hostilityMaxEnding, hostilityMaxDialogueSprite, hostilityMaxDialogueText);
                return; // Sadece ilk bulunanı tetikle
            }
        }
    }
    
    private void CheckAllMapsCompletedCondition()
    {
        // MapManager'dan harita bilgilerini al
        if (MapManager.Instance == null)
        {
            Debug.LogWarning("MapManager.Instance null!");
            return;
        }
        
        // Tüm haritaları al
        List<MapType> allMaps = MapManager.Instance.GetAllMaps();
        Dictionary<MapType, int> mapTurns = MapManager.Instance.GetMapTurns();
        
        // Her haritanın en az 1 turn oynanıp oynanmadığını kontrol et
        bool allMapsCompleted = true;
        foreach (MapType mapType in allMaps)
        {
            if (!mapTurns.ContainsKey(mapType) || mapTurns[mapType] < 1)
            {
                allMapsCompleted = false;
                break;
            }
        }
        
        if (allMapsCompleted)
        {
            Debug.Log("Tüm haritalar tamamlandı! All maps completed ending tetikleniyor.");
            TriggerEndingScenario(allMapsCompletedEnding, allMapsCompletedDialogueSprite, allMapsCompletedDialogueText);
        }
    }
    
    /// <summary>
    /// Trigger ending scenario with support for both new system and legacy sprite/text
    /// </summary>
    private void TriggerEndingScenario(EndingScenario scenario, Sprite legacySprite = null, string legacyText = null)
    {
        var endingPanelUI = FindFirstObjectByType<EndingPanelUI>(FindObjectsInactive.Include);
        if (endingPanelUI == null)
        {
            Debug.LogError("EndingPanelUI not found! Cannot trigger ending scenario.");
            return;
        }

        // If legacy sprite and text are provided, create custom ending data
        if (legacySprite != null && !string.IsNullOrEmpty(legacyText))
        {
            EndingData customEnding = new EndingData
            {
                scenario = scenario,
                title = GetScenarioTitle(scenario),
                description = legacyText,
                endingImage = legacySprite,
                backgroundColor = GetScenarioBackgroundColor(scenario),
                textColor = GetScenarioTextColor(scenario),
                titleColor = GetScenarioTitleColor(scenario)
            };
            
            Debug.Log($"Triggering custom ending for scenario: {scenario}");
            endingPanelUI.ShowEnding(customEnding);
        }
        else
        {
            // Use default ending data from EndingPanelUI
            Debug.Log($"Triggering default ending for scenario: {scenario}");
            endingPanelUI.ShowEnding(scenario);
        }
        
        // Also trigger GameManager completion
        if (GameManager.Instance != null)
        {
            GameManager.Instance.FinishGameWithScenario(scenario);
        }
    }
    
    private string GetScenarioTitle(EndingScenario scenario)
    {
        switch (scenario)
        {
            case EndingScenario.TrustDefeat: return "Trust Lost";
            case EndingScenario.FaithDefeat: return "Faith Broken";
            case EndingScenario.HostilityDefeat: return "Power Faded";
            case EndingScenario.HostilityVictory: return "Power Through Strength";
            case EndingScenario.AllMapsCompleted: return "Journey Complete";
            default: return "Game Complete";
        }
    }
    
    private Color GetScenarioBackgroundColor(EndingScenario scenario)
    {
        switch (scenario)
        {
            case EndingScenario.TrustDefeat: return new Color(0.8f, 0.2f, 0.2f, 1.0f); // Red
            case EndingScenario.FaithDefeat: return new Color(0.6f, 0.3f, 0.8f, 1.0f); // Purple
            case EndingScenario.HostilityDefeat: return new Color(0.4f, 0.4f, 0.4f, 1.0f); // Dark Gray
            case EndingScenario.HostilityVictory: return new Color(0.8f, 0.2f, 0.2f, 1.0f); // Red
            case EndingScenario.AllMapsCompleted: return new Color(0.3f, 0.7f, 0.3f, 1.0f); // Green
            default: return Color.black;
        }
    }
    
    private Color GetScenarioTextColor(EndingScenario scenario)
    {
        switch (scenario)
        {
            case EndingScenario.FaithDefeat: return Color.white;
            case EndingScenario.HostilityDefeat: return Color.white;
            default: return Color.white;
        }
    }
    
    private Color GetScenarioTitleColor(EndingScenario scenario)
    {
        switch (scenario)
        {
            case EndingScenario.TrustDefeat: return new Color(1.0f, 0.6f, 0.6f, 1.0f); // Light Red
            case EndingScenario.FaithDefeat: return new Color(0.9f, 0.7f, 1.0f, 1.0f); // Light Purple
            case EndingScenario.HostilityDefeat: return new Color(0.7f, 0.7f, 0.7f, 1.0f); // Light Gray
            case EndingScenario.HostilityVictory: return new Color(1.0f, 0.4f, 0.4f, 1.0f); // Light Red
            case EndingScenario.AllMapsCompleted: return new Color(0.6f, 1.0f, 0.6f, 1.0f); // Light Green
            default: return Color.white;
        }
    }

    private void ShowSpecialDialogue(DialogueNode dialogue, MapType country)
    {
        DialogueManager dialogueManager = UnityEngine.Object.FindFirstObjectByType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.ShowSpecificDialogue(dialogue);
        }
    }
    
    private void UpdateConditionsForMap(MapType mapType)
    {
        // Harita değiştiğinde condition'ları güncelle
        // Bu sistemde harita değişikliği önemli değil
    }
    
    #region DEBUG AND TESTING
    [Title("Debug and Testing", "Test ending scenarios")]
    [Button("Test Trust Defeat")]
    private void TestTrustDefeat()
    {
        TriggerEndingScenario(EndingScenario.TrustDefeat, trustZeroDialogueSprite, trustZeroDialogueText);
    }
    
    [Button("Test Faith Defeat")]
    private void TestFaithDefeat()
    {
        TriggerEndingScenario(EndingScenario.FaithDefeat, faithZeroDialogueSprite, faithZeroDialogueText);
    }
    
    [Button("Test Hostility Victory")]
    private void TestHostilityVictory()
    {
        TriggerEndingScenario(EndingScenario.HostilityVictory, hostilityMaxDialogueSprite, hostilityMaxDialogueText);
    }
    
    [Button("Test All Maps Completed")]
    private void TestAllMapsCompleted()
    {
        TriggerEndingScenario(EndingScenario.AllMapsCompleted, allMapsCompletedDialogueSprite, allMapsCompletedDialogueText);
    }
    
    [Button("Test All Conditions")]
    private void TestAllConditions()
    {
        Debug.Log("Testing all condition checks...");
        CheckTrustZeroConditions();
        CheckFaithZeroConditions();
        CheckHostilityMaxConditions();
        CheckAllMapsCompletedCondition();
    }
    #endregion
}

/// <summary>
/// Yeni Turn Condition Sistemi
/// 
/// Bu sistem şu durumları kontrol eder:
/// 1. Herhangi bir ülkenin Trust'ı 0'a düşerse
/// 2. Herhangi bir ülkenin Faith'i 0'a düşerse  
/// 3. Herhangi bir ülkenin Hostility'si 100'e çıkarsa
/// 4. Tüm haritalar tamamlandığında
/// </summary> 