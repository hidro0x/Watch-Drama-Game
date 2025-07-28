using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Global Effects System Örneği
/// 
/// Bu sistem sayesinde:
/// 1. Normal diyaloglar sadece aktif ülkeyi etkiler
/// 2. Global etkiler tüm ülkeleri aynı anda etkileyebilir
/// 3. Her ülke için farklı değerler ayarlanabilir
/// 
/// Örnek Kullanım:
/// - Varnan +5 +5 -3 (Varnan'ın Trust'ı +5, Faith'i +5, Hostility'si -3)
/// - Astrahil +2 -1 +0 (Astrahil'in Trust'ı +2, Faith'i -1, Hostility'si değişmez)
/// </summary>
public class GlobalEffectsExample : MonoBehaviour
{
    [Header("Örnek Global Effect")]
    public GlobalDialogueEffect exampleGlobalEffect;
    
    private void Start()
    {
        // Örnek global effect oluştur
        CreateExampleGlobalEffect();
    }
    
    private void CreateExampleGlobalEffect()
    {
        exampleGlobalEffect = new GlobalDialogueEffect();
        exampleGlobalEffect.countryEffects = new Dictionary<MapType, BarValues>
        {
            { MapType.Varnan,   new BarValues { trust = 5, faith = 5, hostility = -3 } },
            { MapType.Astrahil, new BarValues { trust = 2, faith = -1, hostility = 0 } },
            { MapType.Solarya,  new BarValues { trust = 0, faith = 3, hostility = 1 } },
            { MapType.Theon,    new BarValues { trust = -2, faith = 1, hostility = 2 } },
            { MapType.Agnari,   new BarValues { trust = 1, faith = -2, hostility = -1 } },
        };
    }
    
    [ContextMenu("Test Global Effect")]
    public void TestGlobalEffect()
    {
        if (exampleGlobalEffect != null)
        {
            GameManager.Instance.ApplyGlobalDialogueEffect(exampleGlobalEffect);
            Debug.Log("Global effect uygulandı!");
        }
    }
} 