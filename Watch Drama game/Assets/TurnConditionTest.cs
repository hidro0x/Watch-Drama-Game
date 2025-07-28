using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Yeni Turn Condition Sistemi Test Script'i
/// 
/// Bu script ile turn condition'ları test edebilirsiniz:
/// 1. Trust 0'a düşme testi
/// 2. Faith 0'a düşme testi  
/// 3. Hostility 100'e çıkma testi
/// 4. Maximum turn testi
/// </summary>
public class TurnConditionTest : MonoBehaviour
{
    [Header("Test Diyalogları")]
    public DialogueNode trustZeroTestDialogue;
    public DialogueNode faithZeroTestDialogue;
    public DialogueNode hostilityMaxTestDialogue;
    public DialogueNode maxTurnTestDialogue;
    
    [Header("Test Ayarları")]
    public int testTrustValue = 0;
    public int testFaithValue = 0;
    public int testHostilityValue = 100;
    public int testMaxTurn = 5;
    
    private void Start()
    {
        CreateTestDialogues();
    }
    
    private void CreateTestDialogues()
    {
        // Trust 0 Test Diyalogu
        trustZeroTestDialogue = new DialogueNode
        {
            id = "trust_zero_test",
            text = "Bir ülkenin Trust değeri 0'a düştü! Bu durum oyunu etkileyebilir.",
            choices = new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    text = "Durumu kabul et",
                    trustChange = 0,
                    faithChange = 0,
                    hostilityChange = 0
                },
                new DialogueChoice
                {
                    text = "Çözüm ara",
                    trustChange = 5,
                    faithChange = 3,
                    hostilityChange = -2
                }
            }
        };
        
        // Faith 0 Test Diyalogu
        faithZeroTestDialogue = new DialogueNode
        {
            id = "faith_zero_test",
            text = "Bir ülkenin Faith değeri 0'a düştü! Bu durum oyunu etkileyebilir.",
            choices = new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    text = "Durumu kabul et",
                    trustChange = 0,
                    faithChange = 0,
                    hostilityChange = 0
                },
                new DialogueChoice
                {
                    text = "Çözüm ara",
                    trustChange = 3,
                    faithChange = 5,
                    hostilityChange = -2
                }
            }
        };
        
        // Hostility 100 Test Diyalogu
        hostilityMaxTestDialogue = new DialogueNode
        {
            id = "hostility_max_test",
            text = "Bir ülkenin Hostility değeri 100'e çıktı! Bu durum oyunu etkileyebilir.",
            choices = new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    text = "Durumu kabul et",
                    trustChange = 0,
                    faithChange = 0,
                    hostilityChange = 0
                },
                new DialogueChoice
                {
                    text = "Çözüm ara",
                    trustChange = 2,
                    faithChange = 2,
                    hostilityChange = -5
                }
            }
        };
        
        // Max Turn Test Diyalogu
        maxTurnTestDialogue = new DialogueNode
        {
            id = "max_turn_test",
            text = "Maximum turn sayısına ulaştınız! Oyun sona eriyor.",
            choices = new List<DialogueChoice>
            {
                new DialogueChoice
                {
                    text = "Oyunu bitir",
                    trustChange = 0,
                    faithChange = 0,
                    hostilityChange = 0
                },
                new DialogueChoice
                {
                    text = "Devam et",
                    trustChange = 10,
                    faithChange = 10,
                    hostilityChange = -10
                }
            }
        };
    }
    
    [ContextMenu("Test Trust 0")]
    public void TestTrustZero()
    {
        if (GameManager.Instance != null)
        {
            // Varnan'ın Trust'ını 0'a düşür
            var currentValues = GameManager.Instance.GetBarValuesForCountry(MapType.Varnan);
            currentValues.Trust = testTrustValue;
            GameManager.Instance.SetBarValuesForCountry(MapType.Varnan, currentValues);
            
            Debug.Log($"Varnan'ın Trust değeri {testTrustValue}'a ayarlandı. Turn condition tetiklenmeli!");
        }
    }
    
    [ContextMenu("Test Faith 0")]
    public void TestFaithZero()
    {
        if (GameManager.Instance != null)
        {
            // Astrahil'in Faith'ini 0'a düşür
            var currentValues = GameManager.Instance.GetBarValuesForCountry(MapType.Astrahil);
            currentValues.Faith = testFaithValue;
            GameManager.Instance.SetBarValuesForCountry(MapType.Astrahil, currentValues);
            
            Debug.Log($"Astrahil'in Faith değeri {testFaithValue}'a ayarlandı. Turn condition tetiklenmeli!");
        }
    }
    
    [ContextMenu("Test Hostility 100")]
    public void TestHostilityMax()
    {
        if (GameManager.Instance != null)
        {
            // Solarya'nın Hostility'sini 100'e çıkar
            var currentValues = GameManager.Instance.GetBarValuesForCountry(MapType.Solarya);
            currentValues.Hostility = testHostilityValue;
            GameManager.Instance.SetBarValuesForCountry(MapType.Solarya, currentValues);
            
            Debug.Log($"Solarya'nın Hostility değeri {testHostilityValue}'a ayarlandı. Turn condition tetiklenmeli!");
        }
    }
    
    [ContextMenu("Test Max Turn")]
    public void TestMaxTurn()
    {
        DialogueManager dialogueManager = UnityEngine.Object.FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            // Turn sayısını artır
            for (int i = 0; i < testMaxTurn; i++)
            {
                dialogueManager.IncrementTurn();
            }
            
            Debug.Log($"Turn sayısı {testMaxTurn}'a ayarlandı. Turn condition tetiklenmeli!");
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
    
    [ContextMenu("Turn Sayısını Göster")]
    public void ShowTurnCount()
    {
        DialogueManager dialogueManager = UnityEngine.Object.FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            Debug.Log($"Mevcut Turn: {dialogueManager.GetCurrentTurn()}");
        }
    }
    
    [ContextMenu("Turn Condition'ları Manuel Tetikle")]
    public void ManuallyTriggerTurnConditions()
    {
        TurnConditionSystem turnConditionSystem = UnityEngine.Object.FindObjectOfType<TurnConditionSystem>();
        if (turnConditionSystem != null)
        {
            turnConditionSystem.CheckTurnConditions();
            Debug.Log("Turn condition'ları manuel olarak tetiklendi!");
        }
    }
} 