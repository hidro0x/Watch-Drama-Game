using UnityEngine;
using System;
using System.Collections.Generic;

public enum ValueType
{
    Trust,
    Faith,
    Hostility
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

    private Dictionary<MapType, MapValues> mapValuesDict = new Dictionary<MapType, MapValues>();
    private MapType currentMapType;

    // Seçim yapıldığında tetiklenecek event
    public static event Action<ChoiceEffect> OnChoiceMade;
    // Değer 0'a düştüğünde tetiklenecek event
    public static event Action<ValueType> OnValueReachedZero;

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