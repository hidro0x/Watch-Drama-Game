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

    [Header("Başlangıç Değerleri")]
    public int startingTrust = 50;
    public int startingFaith = 50;
    public int startingHostility = 50;

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
                mapValuesDict[item.mapType] = new MapValues(item.trust, item.faith, item.hostility);
            }
        }
        else
        {
            // Eğer inspector'dan ayarlanmadıysa tüm MapType'lar için default değer ata
            foreach (MapType mapType in Enum.GetValues(typeof(MapType)))
            {
                mapValuesDict[mapType] = new MapValues(startingTrust, startingFaith, startingHostility);
            }
        }
        // Varsayılan olarak ilk MapType'ı aktif yap
        currentMapType = (MapType)0;
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
        OnChoiceMade?.Invoke(new ChoiceEffect(0, 0, 0)); // Sıfır etkiyle tetiklenir, sadece güncelleme için
    }

    // Seçimden gelen etkiyi uygula (artık sadece aktif map'e uygula)
    private void ApplyChoiceEffect(ChoiceEffect effect)
    {
        var values = mapValuesDict[currentMapType];
        values.Trust += effect.TrustChange;
        values.Faith += effect.FaithChange;
        values.Hostility += effect.HostilityChange;

        // Değerlerin min sınırlarını kontrol et (0'ın altına düşmesin)
        values.Trust = Mathf.Max(0, values.Trust);
        values.Faith = Mathf.Max(0, values.Faith);
        values.Hostility = Mathf.Max(0, values.Hostility);

        mapValuesDict[currentMapType] = values;

        CheckForZeroValues(values);

        // DialogueManager'a seçim yapıldığını bildir
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.OnChoiceMade();
        }
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

    // Yeni oyun başlatma
    public void StartNewGame()
    {
        InitializeMapValues();
        Debug.Log("Yeni oyun başlatıldı!");
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