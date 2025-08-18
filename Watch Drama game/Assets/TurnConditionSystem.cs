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
    [Title("Bar Sıfırlanma Diyalogları")]
    [LabelWidth(120)]
    public Sprite trustZeroDialogueSprite;
    public string trustZeroDialogueText;
    
    [LabelWidth(120)]
    public Sprite faithZeroDialogueSprite;
    public string faithZeroDialogueText;
    
    [LabelWidth(120)]
    public Sprite hostilityMaxDialogueSprite;
    public string hostilityMaxDialogueText;
    
    [Title("Tüm Haritalar Tamamlandı Diyalogu")]
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
                Debug.Log($"{country} ülkesinin Trust değeri 0'a düştü! Özel diyalog tetikleniyor.");
                FindFirstObjectByType<EndingPanelUI>(FindObjectsInactive.Include).SetEnding(trustZeroDialogueSprite, trustZeroDialogueText);
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
                Debug.Log($"{country} ülkesinin Faith değeri 0'a düştü! Özel diyalog tetikleniyor.");
                FindFirstObjectByType<EndingPanelUI>(FindObjectsInactive.Include).SetEnding(faithZeroDialogueSprite, faithZeroDialogueText);
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
                Debug.Log($"{country} ülkesinin Hostility değeri 100'e çıktı! Özel diyalog tetikleniyor.");
                FindFirstObjectByType<EndingPanelUI>(FindObjectsInactive.Include).SetEnding(hostilityMaxDialogueSprite, hostilityMaxDialogueText);
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
            Debug.Log("Tüm haritalar tamamlandı! Özel diyalog tetikleniyor.");
            FindFirstObjectByType<EndingPanelUI>(FindObjectsInactive.Include).SetEnding(allMapsCompletedDialogueSprite, allMapsCompletedDialogueText);
        }
    }
    
    private void ShowSpecialDialogue(DialogueNode dialogue, MapType country)
    {
        DialogueManager dialogueManager = UnityEngine.Object.FindObjectOfType<DialogueManager>();
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