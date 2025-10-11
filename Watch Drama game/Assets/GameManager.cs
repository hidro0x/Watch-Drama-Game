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

    [Header("🔧 DEBUG - Inspector Test Controls")]
    [Space(10)]
    [SerializeField] private bool showDebugControls = true;
    [Space(5)]
    [Header("Preset Value Combinations")]
    [SerializeField] private bool usePresetValues = false;
    [SerializeField] private DebugPreset presetToApply = DebugPreset.Normal;
    
    [Space(10)]
    [Header("Manual Value Adjustment")]
    [SerializeField] private int debugTrust = 0;
    [SerializeField] private int debugFaith = 0;
    [SerializeField] private int debugHostility = 0;
    [SerializeField] private bool applyManualValues = false;
    
    [Space(10)]
    [Header("Quick Actions")]
    [SerializeField] private bool resetAllValues = false;
    [SerializeField] private bool maxAllValues = false;

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
        if (mapTypeValuesList != null && mapTypeValuesList.Count > 0)
        {
            foreach (var item in mapTypeValuesList)
            {
                var initVals = new MapValues(item.trust, item.faith, item.hostility);
                mapValuesDict[item.mapType] = ClampMapValues(initVals);
            }
        }
        else
        {
            // Eğer inspector'dan ayarlanmadıysa tüm MapType'lar için default değer ata
            foreach (MapType mapType in Enum.GetValues(typeof(MapType)))
            {
                mapValuesDict[mapType] = new MapValues(0, 0, 0);
            }
        }
        // Varsayılan olarak ilk MapType'ı aktif yap
        currentMapType = (MapType)0;
        SyncMapTypeValuesList();
    }

    private void OnDestroy()
    {
        OnChoiceMade -= ApplyChoiceEffect;
        OnValueReachedZero -= HandleValueReachedZero;
    }

    private void Update()
    {
        if (!showDebugControls) return;
        
        // Preset değerleri uygula
        if (usePresetValues)
        {
            ApplyPresetValues(presetToApply);
            usePresetValues = false; // Tek seferlik kullanım
        }
        
        // Manuel değerleri uygula
        if (applyManualValues)
        {
            ApplyManualValues();
            applyManualValues = false; // Tek seferlik kullanım
        }
        
        // Tüm değerleri sıfırla
        if (resetAllValues)
        {
            ResetAllValues();
            resetAllValues = false; // Tek seferlik kullanım
        }
        
        // Tüm değerleri maksimum yap
        if (maxAllValues)
        {
            MaxAllValues();
            maxAllValues = false; // Tek seferlik kullanım
        }
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
        DialogueManager dialogueManager = UnityEngine.Object.FindObjectOfType<DialogueManager>();
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
        FinishGameWithScenario(EndingScenario.TrustVictory);
    }
    
    [Button("Finish with Faith Victory")]
    public void FinishWithFaithVictory()
    {
        FinishGameWithScenario(EndingScenario.FaithVictory);
    }
    
    [Button("Finish with Hostility Victory")]
    public void FinishWithHostilityVictory()
    {
        FinishGameWithScenario(EndingScenario.HostilityVictory);
    }
    
    [Button("Finish with Balanced Victory")]
    public void FinishWithBalancedVictory()
    {
        FinishGameWithScenario(EndingScenario.BalancedVictory);
    }
    
    [Button("Finish with All Maps Completed")]
    public void FinishWithAllMapsCompleted()
    {
        FinishGameWithScenario(EndingScenario.AllMapsCompleted);
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
    /// Preset değerleri uygula
    /// </summary>
    private void ApplyPresetValues(DebugPreset preset)
    {
        MapValues newValues = GetPresetValues(preset);
        mapValuesDict[currentMapType] = newValues;
        SyncMapTypeValuesList();
        RefreshValues();
        
        Debug.Log($"🔧 DEBUG: Preset '{preset}' uygulandı - Trust: {newValues.Trust}, Faith: {newValues.Faith}, Hostility: {newValues.Hostility}");
    }

    /// <summary>
    /// Manuel değerleri uygula
    /// </summary>
    private void ApplyManualValues()
    {
        MapValues newValues = new MapValues(debugTrust, debugFaith, debugHostility);
        newValues = ClampMapValues(newValues);
        mapValuesDict[currentMapType] = newValues;
        SyncMapTypeValuesList();
        RefreshValues();
        
        Debug.Log($"🔧 DEBUG: Manuel değerler uygulandı - Trust: {newValues.Trust}, Faith: {newValues.Faith}, Hostility: {newValues.Hostility}");
    }

    /// <summary>
    /// Tüm değerleri sıfırla
    /// </summary>
    private void ResetAllValues()
    {
        MapValues zeroValues = new MapValues(0, 0, 0);
        mapValuesDict[currentMapType] = zeroValues;
        SyncMapTypeValuesList();
        RefreshValues();
        
        Debug.Log("🔧 DEBUG: Tüm değerler sıfırlandı");
    }

    /// <summary>
    /// Tüm değerleri maksimum yap
    /// </summary>
    private void MaxAllValues()
    {
        MapValues maxValues = new MapValues(100, 100, 100);
        mapValuesDict[currentMapType] = maxValues;
        SyncMapTypeValuesList();
        RefreshValues();
        
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
        InitializeMapValues();
        Debug.Log("Yeni oyun başlatıldı!");
        SyncMapTypeValuesList();
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