using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

public enum ValueType
{
    Trust,
    Faith,
    Hostility
}

public enum DebugPreset
{
    Normal,          // Trust: 50, Faith: 50, Hostility: 0
    TrustZero,       // Trust: 0, Faith: 50, Hostility: 50
    FaithZero,       // Trust: 50, Faith: 0, Hostility: 50
    HostilityMax,    // Trust: 0, Faith: 0, Hostility: 100
    AllZero,         // Trust: 0, Faith: 0, Hostility: 0
    AllMax,          // Trust: 100, Faith: 100, Hostility: 100
    TrustFaithZero,  // Trust: 0, Faith: 0, Hostility: 50
    Balanced,        // Trust: 33, Faith: 33, Hostility: 33
}

[Serializable]
public struct MapValues
{
    public int Trust;
    public int Faith;
    public int Hostility;

    public MapValues(int trust, int faith, int hostility)
    {
        Trust = trust;
        Faith = faith;
        Hostility = hostility;
    }
}

[Serializable]
public class MapTypeValues
{
    public MapType mapType;
    public int trust;
    public int faith;
    public int hostility;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    [Header("Map Bazlı Değerler")]
    public List<MapTypeValues> mapTypeValuesList;

    [Title("🔧 DEBUG - Game Controls")]
    [Title("Preset Values")]
    [Button("Apply Normal Preset", ButtonSizes.Large)]
    [GUIColor(0.4f, 0.8f, 1f)]
    public void ApplyNormalPreset() => ApplyPresetValues(DebugPreset.Normal);
    
    [Button("Apply Trust Zero Preset", ButtonSizes.Large)]
    [GUIColor(1f, 0.6f, 0.6f)]
    public void ApplyTrustZeroPreset() => ApplyPresetValues(DebugPreset.TrustZero);
    
    [Button("Apply Faith Zero Preset", ButtonSizes.Large)]
    [GUIColor(1f, 0.8f, 0.4f)]
    public void ApplyFaithZeroPreset() => ApplyPresetValues(DebugPreset.FaithZero);
    
    [Button("Apply Hostility Max Preset", ButtonSizes.Large)]
    [GUIColor(1f, 0.2f, 0.2f)]
    public void ApplyHostilityMaxPreset() => ApplyPresetValues(DebugPreset.HostilityMax);
    
    [Button("Apply All Zero Preset", ButtonSizes.Large)]
    [GUIColor(0.5f, 0.5f, 0.5f)]
    public void ApplyAllZeroPreset() => ApplyPresetValues(DebugPreset.AllZero);
    
    [Button("Apply All Max Preset", ButtonSizes.Large)]
    [GUIColor(0.2f, 1f, 0.2f)]
    public void ApplyAllMaxPreset() => ApplyPresetValues(DebugPreset.AllMax);
    
    [Button("Apply Balanced Preset", ButtonSizes.Large)]
    [GUIColor(1f, 1f, 0.2f)]
    public void ApplyBalancedPreset() => ApplyPresetValues(DebugPreset.Balanced);
    
    [Title("Manual Value Controls")]
    [InfoBox("Set values for maps")]
    [SerializeField] private int manualTrust = 0;
    [SerializeField] private int manualFaith = 0;
    [SerializeField] private int manualHostility = 0;
    [SerializeField] private bool applyToAllMaps = true;
    
    [Button("Apply Manual Values", ButtonSizes.Large)]
    [GUIColor(0.8f, 0.8f, 1f)]
    public void ApplyManualValues() => ApplyManualValues(manualTrust, manualFaith, manualHostility, applyToAllMaps);
    
    [Title("Quick Actions")]
    [Button("Reset All Values", ButtonSizes.Large)]
    [GUIColor(1f, 0.4f, 0.4f)]
    public void ResetAllValues() => ResetAllMapValues();
    
    [Button("Max All Values", ButtonSizes.Large)]
    [GUIColor(0.4f, 1f, 0.4f)]
    public void MaxAllValues() => SetAllValuesToMax();
    
    [Button("Force Update UI", ButtonSizes.Medium)]
    [GUIColor(0.6f, 0.6f, 1f)]
    public void ForceUpdateUI() => ForceUpdateBarUI();
    
    [Button("Print All Map Values", ButtonSizes.Medium)]
    [GUIColor(1f, 1f, 0.6f)]
    public void PrintAllMapValuesDebug() => PrintAllMapValues();
    
    [Title("Game Completion")]
    [Button("Complete Selected Map", ButtonSizes.Large)]
    [GUIColor(1f, 0.8f, 0.2f)]
    public void CompleteSelectedMap() => CompleteCurrentMap();
    
    [Button("Complete All Maps", ButtonSizes.Large)]
    [GUIColor(0.8f, 0.2f, 1f)]
    public void CompleteAllMapsDebug() => CompleteAllMaps();
    
    [Title("Victory Scenarios")]
    [Button("🏆 Trust Victory", ButtonSizes.Large)]
    [GUIColor(0.2f, 0.8f, 0.2f)]
    public void TriggerTrustVictory() => FinishWithTrustVictory();
    
    [Button("🌟 Faith Victory", ButtonSizes.Large)]
    [GUIColor(0.2f, 0.8f, 1f)]
    public void TriggerFaithVictory() => FinishWithFaithVictory();
    
    [Button("⚔️ Hostility Victory", ButtonSizes.Large)]
    [GUIColor(1f, 0.2f, 0.2f)]
    public void TriggerHostilityVictory() => FinishWithHostilityVictory();
    
    [Button("⚖️ Balanced Victory", ButtonSizes.Large)]
    [GUIColor(1f, 1f, 0.2f)]
    public void TriggerBalancedVictory() => FinishWithBalancedVictory();
    
    [Button("🗺️ All Maps Completed", ButtonSizes.Large)]
    [GUIColor(0.8f, 0.8f, 0.8f)]
    public void TriggerAllMapsCompleted() => FinishWithAllMapsCompleted();
    
    [Title("Game Restart")]
    [Button("🔄 Start New Game", ButtonSizes.Large)]
    [GUIColor(0.4f, 0.4f, 1f)]
    public void StartNewGameDebug() => StartNewGame();

    private Dictionary<MapType, MapValues> mapValuesDict = new Dictionary<MapType, MapValues>();
    private MapType currentMapType;

    // Seçim yapıldığında tetiklenecek event
    public static event Action<ChoiceEffect> OnChoiceMade;
    // Değer 0'a düştüğünde tetiklenecek event
    public static event Action<ValueType> OnValueReachedZero;
    // Oyun tamamlandığında tetiklenecek event
    public static event Action<EndingScenario> OnGameFinished;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        OnChoiceMade += ApplyChoiceEffect;
        OnValueReachedZero += HandleValueReachedZero;

        InitializeMapValues();
    }

    private void InitializeMapValues()
    {
        mapValuesDict.Clear();
        
        // Apply normal preset values to all maps
        var normalPresetValues = GetPresetValues(DebugPreset.Normal);
        var allMaps = GetAllMapTypes();
        
        foreach (var mapType in allMaps)
        {
            mapValuesDict[mapType] = normalPresetValues;
        }
        
        // Varsayılan olarak ilk MapType'ı aktif yap
        currentMapType = (MapType)0;
        SyncMapTypeValuesList();
        
        Debug.Log("🎮 All maps initialized with normal preset values (Trust: 50, Faith: 50, Hostility: 0)");
    }

    private void OnDestroy()
    {
        OnChoiceMade -= ApplyChoiceEffect;
        OnValueReachedZero -= HandleValueReachedZero;
    }


    // Aktif haritayı ayarla
    public void SetActiveMap(MapType mapType)
    {
        currentMapType = mapType;
        // BarUIController'ı güncellemek için event tetikle
        RefreshValues();
    }

    // Seçimden gelen etkiyi uygula (artık sadece aktif map'e uygula)
    private void ApplyChoiceEffect(ChoiceEffect effect)
    {
        var values = mapValuesDict[currentMapType];
        values.Trust += effect.TrustChange;
        values.Faith += effect.FaithChange;
        values.Hostility += effect.HostilityChange;
        values = ClampMapValues(values);
        mapValuesDict[currentMapType] = values;

        CheckForZeroValues(values);
        SyncMapTypeValuesList();

        // DialogueManager'a seçim yapıldığını bildir
        DialogueManager dialogueManager = UnityEngine.Object.FindFirstObjectByType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.OnChoiceMade();
        }
    }

    // Global diyalog etkilerini uygula (tüm ülkelere etki eder)
    public void ApplyGlobalDialogueEffect(GlobalDialogueEffect globalEffect)
    {
        if (globalEffect == null || globalEffect.countryEffects == null)
        {
            Debug.LogWarning("GlobalDialogueEffect veya countryEffects null!");
            return;
        }
        
        foreach (var kvp in globalEffect.countryEffects)
        {
            var country = kvp.Key;
            var values = kvp.Value;
            if (mapValuesDict.ContainsKey(country))
            {
                var current = mapValuesDict[country];
                current.Trust += values.trust;
                current.Faith += values.faith;
                current.Hostility += values.hostility;
                current = ClampMapValues(current);
                mapValuesDict[country] = current;
                CheckForZeroValues(current);
            }
        }
        SyncMapTypeValuesList();
        RefreshValues();
    }

    private void RefreshValues(){
        var values = mapValuesDict[currentMapType];
        values = ClampMapValues(values);
        mapValuesDict[currentMapType] = values;
    }

    private void CheckForZeroValues(MapValues values)
    {
        if (values.Trust == 0)
        {
            OnValueReachedZero?.Invoke(ValueType.Trust);
        }
        if (values.Faith == 0)
        {
            OnValueReachedZero?.Invoke(ValueType.Faith);
        }
        if (values.Hostility == 0)
        {
            OnValueReachedZero?.Invoke(ValueType.Hostility);
        }
    }

    // Değer 0'a düştüğünde çağrılacak ana method
    private void HandleValueReachedZero(ValueType valueType)
    {
        Debug.Log($"{valueType} değeri 0'a düştü!");

        switch (valueType)
        {
            case ValueType.Trust:
                OnTrustReachedZero();
                break;
            case ValueType.Faith:
                OnFaithReachedZero();
                break;
            case ValueType.Hostility:
                OnHostilityReachedZero();
                break;
        }
    }

    // Trust 0'a düştüğünde çağrılacak method
    private void OnTrustReachedZero()
    {
        Debug.Log("Trust 0'a düştü - Güven kaybedildi!");
        // Burada Trust'ın 0'a düşmesiyle ilgili özel işlemler yapılabilir
        // Örnek: Oyun sonu, özel cutscene, vs.
    }

    // Faith 0'a düştüğünde çağrılacak method
    private void OnFaithReachedZero()
    {
        Debug.Log("Faith 0'a düştü - İnanç kaybedildi!");
        // Burada Faith'in 0'a düşmesiyle ilgili özel işlemler yapılabilir
    }

    // Hostility 0'a düştüğünde çağrılacak method
    private void OnHostilityReachedZero()
    {
        Debug.Log("Hostility 0'a düştü - Düşmanlık bitti!");
        // Burada Hostility'nin 0'a düşmesiyle ilgili özel işlemler yapılabilir
    }

    // Seçim yapıldığında bu fonksiyon çağrılmalı
    public static void MakeChoice(ChoiceEffect effect)
    {
        OnChoiceMade?.Invoke(effect);
    }

    // BarUIController için getter'lar
    public int GetTrust() => mapValuesDict.ContainsKey(currentMapType) ? mapValuesDict[currentMapType].Trust : 0;
    public int GetFaith() => mapValuesDict.ContainsKey(currentMapType) ? mapValuesDict[currentMapType].Faith : 0;
    public int GetHostility() => mapValuesDict.ContainsKey(currentMapType) ? mapValuesDict[currentMapType].Hostility : 0;

    // Belirli bir ülke için değerleri al
    public MapValues GetMapValues(MapType mapType)
    {
        return mapValuesDict.ContainsKey(mapType) ? mapValuesDict[mapType] : new MapValues(0, 0, 0);
    }
    
    #region GAME COMPLETION METHODS
    /// <summary>
    /// Finish the game with a specific ending scenario
    /// </summary>
    [Title("Game Completion", "Methods for finishing the game with different scenarios")]
    [Button("Finish with Trust Victory")]
    public void FinishWithTrustVictory()
    {
        // Tüm haritaları complete etmiş gibi davran
        CompleteAllMaps();
        
        // Trust victory efekti uygula
        ApplyTrustVictoryEffects();
        
        // Oyun bitti event'ini tetikle
        FinishGameWithScenario(EndingScenario.TrustVictory);
        
        // Bar UI'yi force update et
        ForceUpdateBarUI();
        
        Debug.Log("🏆 Trust Victory - Tüm haritalar complete edildi!");
    }
    
    [Button("Finish with Faith Victory")]
    public void FinishWithFaithVictory()
    {
        // Tüm haritaları complete etmiş gibi davran
        CompleteAllMaps();
        
        // Faith victory efekti uygula
        ApplyFaithVictoryEffects();
        
        // Oyun bitti event'ini tetikle
        FinishGameWithScenario(EndingScenario.FaithVictory);
        
        // Bar UI'yi force update et
        ForceUpdateBarUI();
        
        Debug.Log("🌟 Faith Victory - Tüm haritalar complete edildi!");
    }
    
    [Button("Finish with Hostility Victory")]
    public void FinishWithHostilityVictory()
    {
        // Tüm haritaları complete etmiş gibi davran
        CompleteAllMaps();
        
        // Hostility victory efekti uygula
        ApplyHostilityVictoryEffects();
        
        // Oyun bitti event'ini tetikle
        FinishGameWithScenario(EndingScenario.HostilityVictory);
        
        // Bar UI'yi force update et
        ForceUpdateBarUI();
        
        Debug.Log("⚔️ Hostility Victory - Tüm haritalar complete edildi!");
    }
    
    [Button("Finish with Balanced Victory")]
    public void FinishWithBalancedVictory()
    {
        // Tüm haritaları complete etmiş gibi davran
        CompleteAllMaps();
        
        // Balanced victory efekti uygula
        ApplyBalancedVictoryEffects();
        
        // Oyun bitti event'ini tetikle
        FinishGameWithScenario(EndingScenario.BalancedVictory);
        
        // Bar UI'yi force update et
        ForceUpdateBarUI();
        
        Debug.Log("⚖️ Balanced Victory - Tüm haritalar complete edildi!");
    }
    
    [Button("Finish with All Maps Completed")]
    public void FinishWithAllMapsCompleted()
    {
        FinishGameWithScenario(EndingScenario.AllMapsCompleted);
        
        // Bar UI'yi force update et
        ForceUpdateBarUI();
    }
    
    public void FinishGameWithScenario(EndingScenario scenario)
    {
        Debug.Log($"🎯 Finishing game with scenario: {scenario}");
        
        // Process the ending scenario
        ProcessEndingScenario(scenario);
        
        // Save game completion data
        SaveGameCompletion(scenario);
        
        // Trigger any end-game events
        OnGameFinished?.Invoke(scenario);
    }
    
    /// <summary>
    /// Process the ending scenario and apply any final effects
    /// </summary>
    private void ProcessEndingScenario(EndingScenario scenario)
    {
        switch (scenario)
        {
            case EndingScenario.TrustVictory:
                Debug.Log("🏆 Trust Victory achieved!");
                ApplyTrustVictoryEffects();
                break;
            case EndingScenario.FaithVictory:
                Debug.Log("🌟 Faith Victory achieved!");
                ApplyFaithVictoryEffects();
                break;
            case EndingScenario.HostilityVictory:
                Debug.Log("⚔️ Hostility Victory achieved!");
                ApplyHostilityVictoryEffects();
                break;
            case EndingScenario.BalancedVictory:
                Debug.Log("⚖️ Balanced Victory achieved!");
                ApplyBalancedVictoryEffects();
                break;
            case EndingScenario.AllMapsCompleted:
                Debug.Log("🗺️ All Maps Completed!");
                ApplyAllMapsCompletedEffects();
                break;
            case EndingScenario.TrustDefeat:
                Debug.Log("💔 Trust Defeat - Game Over");
                ApplyTrustDefeatEffects();
                break;
            case EndingScenario.FaithDefeat:
                Debug.Log("😞 Faith Defeat - Game Over");
                ApplyFaithDefeatEffects();
                break;
            case EndingScenario.HostilityDefeat:
                Debug.Log("😤 Hostility Defeat - Game Over");
                ApplyHostilityDefeatEffects();
                break;
            case EndingScenario.Custom:
                Debug.Log("🎨 Custom ending scenario");
                ApplyCustomEndingEffects();
                break;
        }
    }
    
    private void ApplyTrustVictoryEffects()
    {
        // Set all trust values to maximum
        foreach (MapType mapType in System.Enum.GetValues(typeof(MapType)))
        {
            MapValues currentValues = GetMapValues(mapType);
            currentValues.Trust = 100;
            mapValuesDict[mapType] = currentValues;
        }
        SyncMapTypeValuesList();
        RefreshValues();
    }
    
    private void ApplyFaithVictoryEffects()
    {
        // Set all faith values to maximum
        foreach (MapType mapType in System.Enum.GetValues(typeof(MapType)))
        {
            MapValues currentValues = GetMapValues(mapType);
            currentValues.Faith = 100;
            mapValuesDict[mapType] = currentValues;
        }
        SyncMapTypeValuesList();
        RefreshValues();
    }
    
    private void ApplyHostilityVictoryEffects()
    {
        // Set all hostility values to maximum
        foreach (MapType mapType in System.Enum.GetValues(typeof(MapType)))
        {
            MapValues currentValues = GetMapValues(mapType);
            currentValues.Hostility = 100;
            mapValuesDict[mapType] = currentValues;
        }
        SyncMapTypeValuesList();
        RefreshValues();
    }
    
    private void ApplyBalancedVictoryEffects()
    {
        // Set balanced values across all maps
        foreach (MapType mapType in System.Enum.GetValues(typeof(MapType)))
        {
            mapValuesDict[mapType] = new MapValues(75, 75, 25); // Balanced but positive
        }
        SyncMapTypeValuesList();
        RefreshValues();
    }
    
    private void ApplyAllMapsCompletedEffects()
    {
        // Keep current values but mark all maps as completed
        Debug.Log("All maps have been successfully completed!");
    }
    
    private void ApplyTrustDefeatEffects()
    {
        // Set all trust values to zero
        foreach (MapType mapType in System.Enum.GetValues(typeof(MapType)))
        {
            MapValues currentValues = GetMapValues(mapType);
            currentValues.Trust = 0;
            mapValuesDict[mapType] = currentValues;
        }
        SyncMapTypeValuesList();
        RefreshValues();
    }
    
    private void ApplyFaithDefeatEffects()
    {
        // Set all faith values to zero
        foreach (MapType mapType in System.Enum.GetValues(typeof(MapType)))
        {
            MapValues currentValues = GetMapValues(mapType);
            currentValues.Faith = 0;
            mapValuesDict[mapType] = currentValues;
        }
        SyncMapTypeValuesList();
        RefreshValues();
    }
    
    private void ApplyHostilityDefeatEffects()
    {
        // Set all hostility values to zero
        foreach (MapType mapType in System.Enum.GetValues(typeof(MapType)))
        {
            MapValues currentValues = GetMapValues(mapType);
            currentValues.Hostility = 0;
            mapValuesDict[mapType] = currentValues;
        }
        SyncMapTypeValuesList();
        RefreshValues();
    }
    
    private void ApplyCustomEndingEffects()
    {
        // Custom ending effects - can be customized based on specific requirements
        Debug.Log("Custom ending effects applied");
    }
    
    private void SaveGameCompletion(EndingScenario scenario)
    {
        // Save completion data (could be extended to use PlayerPrefs or save system)
        PlayerPrefs.SetString("LastCompletedScenario", scenario.ToString());
        PlayerPrefs.SetString("GameCompletionTime", System.DateTime.Now.ToString());
        PlayerPrefs.Save();
        
        Debug.Log($"Game completion saved: {scenario} at {System.DateTime.Now}");
    }
    
    /// <summary>
    /// Get the last completed scenario
    /// </summary>
    public EndingScenario GetLastCompletedScenario()
    {
        string scenarioString = PlayerPrefs.GetString("LastCompletedScenario", "Custom");
        if (System.Enum.TryParse<EndingScenario>(scenarioString, out EndingScenario scenario))
        {
            return scenario;
        }
        return EndingScenario.Custom;
    }
    
    /// <summary>
    /// Check if game has been completed
    /// </summary>
    public bool HasGameBeenCompleted()
    {
        return PlayerPrefs.HasKey("LastCompletedScenario");
    }
    #endregion

    #region DEBUG METHODS
    /// <summary>
    /// Preset değerleri uygula (tüm şehirlere)
    /// </summary>
    private void ApplyPresetValues(DebugPreset preset)
    {
        MapValues newValues = GetPresetValues(preset);
        var allMaps = GetAllMapTypes();
        
        // Tüm şehirlere preset değerlerini uygula
        foreach (var mapType in allMaps)
        {
            mapValuesDict[mapType] = newValues;
        }
        
        SyncMapTypeValuesList();
        RefreshValues();
        
        // Bar UI'yi force update et
        ForceUpdateBarUI();
        
        Debug.Log($"🔧 DEBUG: Preset '{preset}' tüm şehirlere uygulandı - Trust: {newValues.Trust}, Faith: {newValues.Faith}, Hostility: {newValues.Hostility}");
    }

    /// <summary>
    /// Manuel değerleri uygula
    /// </summary>
    private void ApplyManualValues(int trust, int faith, int hostility, bool applyToAllMaps = false)
    {
        MapValues newValues = new MapValues(trust, faith, hostility);
        newValues = ClampMapValues(newValues);
        
        if (applyToAllMaps)
        {
            // Tüm şehirlere uygula
            var allMaps = GetAllMapTypes();
            foreach (var mapType in allMaps)
            {
                mapValuesDict[mapType] = newValues;
            }
            Debug.Log($"🔧 DEBUG: Manuel değerler TÜM ŞEHİRLERE uygulandı - Trust: {newValues.Trust}, Faith: {newValues.Faith}, Hostility: {newValues.Hostility}");
        }
        else
        {
            // Sadece aktif şehre uygula
            mapValuesDict[currentMapType] = newValues;
            Debug.Log($"🔧 DEBUG: Manuel değerler {currentMapType} şehrine uygulandı - Trust: {newValues.Trust}, Faith: {newValues.Faith}, Hostility: {newValues.Hostility}");
        }
        
        SyncMapTypeValuesList();
        RefreshValues();
        
        // Bar UI'yi force update et
        ForceUpdateBarUI();
    }

    /// <summary>
    /// Tüm map değerlerini sıfırla
    /// </summary>
    private void ResetAllMapValues()
    {
        var zeroValues = new MapValues(0, 0, 0);
        var allMaps = GetAllMapTypes();
        
        foreach (var mapType in allMaps)
        {
            mapValuesDict[mapType] = zeroValues;
        }
        
        SyncMapTypeValuesList();
        RefreshValues();
        ForceUpdateBarUI();
        
        Debug.Log("🔧 DEBUG: Tüm map değerleri sıfırlandı");
    }

    /// <summary>
    /// Tüm değerleri maksimum yap
    /// </summary>
    private void SetAllValuesToMax()
    {
        var maxValues = new MapValues(100, 100, 100);
        var allMaps = GetAllMapTypes();
        
        foreach (var mapType in allMaps)
        {
            mapValuesDict[mapType] = maxValues;
        }
        
        SyncMapTypeValuesList();
        RefreshValues();
        ForceUpdateBarUI();
        
        Debug.Log("🔧 DEBUG: Tüm değerler maksimum yapıldı");
    }

    /// <summary>
    /// Preset değerlerini al
    /// </summary>
    private MapValues GetPresetValues(DebugPreset preset)
    {
        switch (preset)
        {
            case DebugPreset.Normal:
                return new MapValues(50, 50, 0);
            case DebugPreset.TrustZero:
                return new MapValues(0, 50, 50);
            case DebugPreset.FaithZero:
                return new MapValues(50, 0, 50);
            case DebugPreset.HostilityMax:
                return new MapValues(0, 0, 100);
            case DebugPreset.AllZero:
                return new MapValues(0, 0, 0);
            case DebugPreset.AllMax:
                return new MapValues(100, 100, 100);
            case DebugPreset.TrustFaithZero:
                return new MapValues(0, 0, 50);
            case DebugPreset.Balanced:
                return new MapValues(33, 33, 33);
            default:
                return new MapValues(50, 50, 0);
        }
    }

    /// <summary>
    /// Debug için mevcut değerleri yazdır
    /// </summary>
    [ContextMenu("Print Current Values")]
    public void PrintCurrentValues()
    {
        var values = mapValuesDict[currentMapType];
        Debug.Log($"🔧 Current Values for {currentMapType}: Trust: {values.Trust}, Faith: {values.Faith}, Hostility: {values.Hostility}");
    }

    /// <summary>
    /// Debug için tüm map değerlerini yazdır
    /// </summary>
    [ContextMenu("Print All Map Values")]
    public void PrintAllMapValues()
    {
        Debug.Log("🔧 All Map Values:");
        foreach (var kvp in mapValuesDict)
        {
            Debug.Log($"  {kvp.Key}: Trust: {kvp.Value.Trust}, Faith: {kvp.Value.Faith}, Hostility: {kvp.Value.Hostility}");
        }
    }
    #endregion
    public int GetTrustForCountry(MapType country) => mapValuesDict.ContainsKey(country) ? mapValuesDict[country].Trust : 0;
    public int GetFaithForCountry(MapType country) => mapValuesDict.ContainsKey(country) ? mapValuesDict[country].Faith : 0;
    public int GetHostilityForCountry(MapType country) => mapValuesDict.ContainsKey(country) ? mapValuesDict[country].Hostility : 0;
    
    public MapValues GetBarValuesForCountry(MapType country) => mapValuesDict.ContainsKey(country) ? mapValuesDict[country] : new MapValues(0, 0, 0);
    
    public void SetBarValuesForCountry(MapType country, MapValues values)
    {
        mapValuesDict[country] = ClampMapValues(values);
        RefreshValues();
        SyncMapTypeValuesList();
    }

    // Yeni oyun başlatma
    public void StartNewGame()
    {
        // Map değerlerini yeniden initialize et (default değerleri korur)
        InitializeMapValues();
        
        // UI'yi güncelle
        SyncMapTypeValuesList();
        ForceUpdateBarUI();
        
        Debug.Log("🎮 Yeni oyun başlatıldı - Default değerler yüklendi!");
    }
    
    /// <summary>
    /// Tüm map tiplerini al
    /// </summary>
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

    private void SyncMapTypeValuesList()
    {
        if (mapTypeValuesList == null)
            mapTypeValuesList = new List<MapTypeValues>();
        
        // Ensure list has one entry per MapType and mirrors dictionary values (for inspector/debug)
        var byType = new Dictionary<MapType, MapTypeValues>();
        foreach (var entry in mapTypeValuesList)
        {
            if (!byType.ContainsKey(entry.mapType))
                byType[entry.mapType] = entry;
        }
        foreach (MapType mapType in Enum.GetValues(typeof(MapType)))
        {
            MapValues vals = mapValuesDict.ContainsKey(mapType) ? mapValuesDict[mapType] : new MapValues(0,0,0);
            if (!byType.TryGetValue(mapType, out var m))
            {
                m = new MapTypeValues { mapType = mapType };
                mapTypeValuesList.Add(m);
                byType[mapType] = m;
            }
            m.trust = vals.Trust;
            m.faith = vals.Faith;
            m.hostility = vals.Hostility;
        }
    }

    private MapValues ClampMapValues(MapValues values)
    {
        values.Trust = Mathf.Clamp(values.Trust, 0, 100);
        values.Faith = Mathf.Clamp(values.Faith, 0, 100);
        values.Hostility = Mathf.Clamp(values.Hostility, 0, 100);
        return values;
    }
    
    /// <summary>
    /// Bar UI'yi force update et - Debug için
    /// </summary>
    public void ForceUpdateBarUI()
    {
        // BarUIController'ı bul ve force update et
        var barUIController = UnityEngine.Object.FindFirstObjectByType<BarUIController>();
        if (barUIController != null && barUIController.bar != null)
        {
            // Mevcut map'i al
            var currentMap = MapManager.Instance != null ? MapManager.Instance.GetCurrentMap() : (MapType?)null;
            if (currentMap.HasValue)
            {
                barUIController.bar.Initialize(currentMap.Value);
                barUIController.bar.Refresh();
                Debug.Log($"🔧 DEBUG: Bar UI force updated for {currentMap.Value}");
            }
        }
        else
        {
            Debug.LogWarning("🔧 DEBUG: BarUIController veya bar bulunamadı!");
        }
    }
    
    /// <summary>
    /// Tüm haritaları complete etmiş gibi davran (Game completion için)
    /// </summary>
    private void CompleteAllMaps()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogWarning("MapManager bulunamadı!");
            return;
        }
        
        // MapManager'da tüm haritaları complete et
        MapManager.Instance.CompleteAllMaps();
        Debug.Log("🔧 DEBUG: Tüm haritalar complete edildi!");
    }
    
    /// <summary>
    /// Seçili haritayı complete et (Debug için)
    /// </summary>
    private void CompleteCurrentMap()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogWarning("MapManager bulunamadı!");
            return;
        }
        
        var currentMap = MapManager.Instance.GetCurrentMap();
        if (currentMap == null)
        {
            Debug.LogWarning("Aktif harita yok!");
            return;
        }
        
        // Seçili haritayı complete et
        MapManager.Instance.CompleteCurrentMap();
        
        // Map completion panel'ini tetikle
        var finalStats = GetMapValues(currentMap.Value);
        MapCompletionPanelUI.TriggerMapCompletion(currentMap.Value, finalStats);
        
        Debug.Log($"🔧 DEBUG: {currentMap.Value} haritası complete edildi!");
    }
}

// Seçimlerin etkisini tutan yardımcı struct
public struct ChoiceEffect
{
    public int TrustChange;
    public int FaithChange;
    public int HostilityChange;

    public ChoiceEffect(int trust, int faith, int hostility)
    {
        TrustChange = trust;
        FaithChange = faith;
        HostilityChange = hostility;
    }
} 