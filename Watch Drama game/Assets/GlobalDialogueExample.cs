using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Global Dialogue Node Sistemi Örneği
/// 
/// Bu sistem sayesinde:
/// 1. DialogueNode: Sadece aktif ülkeyi etkiler
/// 2. GlobalDialogueNode: Tüm dünyayı etkiler
/// 3. List<CountryBarEffect> ile serialize edilebilir
/// </summary>
public class GlobalDialogueExample : MonoBehaviour
{
    [Header("Test Diyalogları")]
    public DialogueNode localDialogue;
    public GlobalDialogueNode globalDialogue;
    
    private void Start()
    {
        CreateTestDialogues();
    }
    
    private void CreateTestDialogues()
    {
        // Test 1: Yerel Diyalog (Sadece aktif ülkeyi etkiler)
        localDialogue = new DialogueNode
        {
            id = "local_festival",
            text = "Varnan'da büyük bir festival düzenleniyor. Katılmak ister misin?",
            choices = new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    text = "Festivale katıl",
                    trustChange = 5,
                    faithChange = 3,
                    hostilityChange = -1
                },
                new DialogueChoice
                {
                    text = "Katılma",
                    trustChange = -2,
                    faithChange = -1,
                    hostilityChange = 1
                }
            }
        };
        
        // Test 2: Global Diyalog (Tüm dünyayı etkiler)
        globalDialogue = new GlobalDialogueNode
        {
            id = "world_war",
            text = "Büyük bir savaş tüm dünyayı etkiliyor! Ne yapacaksın?",
            choices = new List<GlobalDialogueChoice>
            {
                new GlobalDialogueChoice
                {
                    text = "Savaşa katıl",
                    globalEffects = new List<CountryBarEffect>
                    {
                        new CountryBarEffect { country = MapType.Varnan, trustChange = -3, faithChange = -2, hostilityChange = 5 },
                        new CountryBarEffect { country = MapType.Astrahil, trustChange = -2, faithChange = -1, hostilityChange = 3 },
                        new CountryBarEffect { country = MapType.Solarya, trustChange = -4, faithChange = -3, hostilityChange = 7 },
                        new CountryBarEffect { country = MapType.Theon, trustChange = -1, faithChange = -2, hostilityChange = 4 },
                        new CountryBarEffect { country = MapType.Agnari, trustChange = -3, faithChange = -1, hostilityChange = 6 }
                    }
                },
                new GlobalDialogueChoice
                {
                    text = "Tarafsız kal",
                    globalEffects = new List<CountryBarEffect>
                    {
                        new CountryBarEffect { country = MapType.Varnan, trustChange = 1, faithChange = 1, hostilityChange = -1 },
                        new CountryBarEffect { country = MapType.Astrahil, trustChange = 2, faithChange = 1, hostilityChange = -2 },
                        new CountryBarEffect { country = MapType.Solarya, trustChange = 1, faithChange = 2, hostilityChange = -1 },
                        new CountryBarEffect { country = MapType.Theon, trustChange = 2, faithChange = 1, hostilityChange = -2 },
                        new CountryBarEffect { country = MapType.Agnari, trustChange = 1, faithChange = 1, hostilityChange = -1 }
                    }
                }
            }
        };
    }
    
    [ContextMenu("Test Yerel Diyalog")]
    public void TestLocalDialogue()
    {
        if (localDialogue != null)
        {
            DialogueManager dialogueManager = UnityEngine.Object.FindObjectOfType<DialogueManager>();
            if (dialogueManager != null)
            {
                dialogueManager.ShowSpecificDialogue(localDialogue);
                Debug.Log("Yerel diyalog test ediliyor - Sadece aktif ülkeyi etkilemeli!");
            }
        }
    }
    
    [ContextMenu("Test Global Diyalog")]
    public void TestGlobalDialogue()
    {
        if (globalDialogue != null)
        {
            DialogueManager dialogueManager = UnityEngine.Object.FindObjectOfType<DialogueManager>();
            if (dialogueManager != null)
            {
                dialogueManager.ShowSpecificDialogue(globalDialogue);
                Debug.Log("Global diyalog test ediliyor - Tüm dünyayı etkilemeli!");
            }
        }
    }
    
    [ContextMenu("Bar Değerlerini Göster")]
    public void ShowBarValues()
    {
        if (GameManager.Instance != null)
        {
            Debug.Log("=== BAR DEĞERLERİ ===");
            Debug.Log($"Varnan: Trust={GameManager.Instance.GetTrustForCountry(MapType.Varnan)}, Faith={GameManager.Instance.GetFaithForCountry(MapType.Varnan)}, Hostility={GameManager.Instance.GetHostilityForCountry(MapType.Varnan)}");
            Debug.Log($"Astrahil: Trust={GameManager.Instance.GetTrustForCountry(MapType.Astrahil)}, Faith={GameManager.Instance.GetFaithForCountry(MapType.Astrahil)}, Hostility={GameManager.Instance.GetHostilityForCountry(MapType.Astrahil)}");
            Debug.Log($"Solarya: Trust={GameManager.Instance.GetTrustForCountry(MapType.Solarya)}, Faith={GameManager.Instance.GetFaithForCountry(MapType.Solarya)}, Hostility={GameManager.Instance.GetHostilityForCountry(MapType.Solarya)}");
            Debug.Log($"Theon: Trust={GameManager.Instance.GetTrustForCountry(MapType.Theon)}, Faith={GameManager.Instance.GetFaithForCountry(MapType.Theon)}, Hostility={GameManager.Instance.GetHostilityForCountry(MapType.Theon)}");
            Debug.Log($"Agnari: Trust={GameManager.Instance.GetTrustForCountry(MapType.Agnari)}, Faith={GameManager.Instance.GetFaithForCountry(MapType.Agnari)}, Hostility={GameManager.Instance.GetHostilityForCountry(MapType.Agnari)}");
        }
    }
} 