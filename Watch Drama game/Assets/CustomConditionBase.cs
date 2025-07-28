using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Özel condition'lar için base class
/// Bu sınıfı extend ederek özel condition'lar oluşturabilirsiniz
/// </summary>
public abstract class CustomConditionBase : MonoBehaviour
{
    [Header("Özel Condition Ayarları")]
    public string conditionDescription = "Özel Condition";
    public bool isEnabled = true;
    
    /// <summary>
    /// Condition'ı kontrol eder
    /// </summary>
    /// <param name="currentMap">Aktif harita</param>
    /// <returns>Condition sağlanıyorsa true</returns>
    public abstract bool CheckCustomCondition(MapType currentMap);
    
    /// <summary>
    /// Condition sağlandığında çalışacak aksiyon
    /// </summary>
    /// <param name="currentMap">Aktif harita</param>
    public abstract void ExecuteCustomAction(MapType currentMap);
    
    /// <summary>
    /// Condition'ın açıklamasını döndürür
    /// </summary>
    public virtual string GetConditionDescription()
    {
        return conditionDescription;
    }
}

/// <summary>
/// Örnek özel condition: Belirli bir ülkenin tüm bar'ları belirli değerin üstündeyse
/// </summary>
public class AllBarsAboveThresholdCondition : CustomConditionBase
{
    [Header("Koşul Ayarları")]
    public MapType targetCountry = MapType.Varnan;
    public int thresholdValue = 70;
    
    public override bool CheckCustomCondition(MapType currentMap)
    {
        int trust = GameManager.Instance.GetTrustForCountry(targetCountry);
        int faith = GameManager.Instance.GetFaithForCountry(targetCountry);
        int hostility = GameManager.Instance.GetHostilityForCountry(targetCountry);
        
        return trust > thresholdValue && faith > thresholdValue && hostility > thresholdValue;
    }
    
    public override void ExecuteCustomAction(MapType currentMap)
    {
        Debug.Log($"{targetCountry} ülkesinin tüm bar'ları {thresholdValue}'nin üstünde!");
        
        // Özel aksiyon: Global effect uygula
        var globalEffect = new GlobalDialogueEffect();
        globalEffect.countryEffects = new Dictionary<MapType, BarValues>
        {
            { targetCountry, new BarValues { trust = 5, faith = 5, hostility = 5 } } // Bonus ver
        };
        
        GameManager.Instance.ApplyGlobalDialogueEffect(globalEffect);
    }
    
    public override string GetConditionDescription()
    {
        return $"{targetCountry} ülkesinin tüm bar'ları {thresholdValue}'nin üstünde";
    }
}

/// <summary>
/// Örnek özel condition: İki ülke arasında bar farkı kontrolü
/// </summary>
public class BarDifferenceCondition : CustomConditionBase
{
    [Header("Koşul Ayarları")]
    public MapType country1 = MapType.Varnan;
    public MapType country2 = MapType.Astrahil;
    public ValueType valueType = ValueType.Trust;
    public int differenceThreshold = 20;
    
    public override bool CheckCustomCondition(MapType currentMap)
    {
        int value1 = GetBarValue(country1, valueType);
        int value2 = GetBarValue(country2, valueType);
        
        int difference = Mathf.Abs(value1 - value2);
        return difference > differenceThreshold;
    }
    
    public override void ExecuteCustomAction(MapType currentMap)
    {
        Debug.Log($"{country1} ve {country2} arasında {valueType} farkı {differenceThreshold}'den büyük!");
        
        // Özel aksiyon: Mesaj göster
        Debug.Log($"Dikkat: {country1} ve {country2} arasında büyük {valueType} farkı var!");
    }
    
    private int GetBarValue(MapType country, ValueType valueType)
    {
        switch (valueType)
        {
            case ValueType.Trust:
                return GameManager.Instance.GetTrustForCountry(country);
            case ValueType.Faith:
                return GameManager.Instance.GetFaithForCountry(country);
            case ValueType.Hostility:
                return GameManager.Instance.GetHostilityForCountry(country);
            default:
                return 0;
        }
    }
    
    public override string GetConditionDescription()
    {
        return $"{country1} ve {country2} arasında {valueType} farkı {differenceThreshold}'den büyük";
    }
} 