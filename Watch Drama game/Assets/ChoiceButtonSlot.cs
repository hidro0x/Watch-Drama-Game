using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChoiceButtonSlot : MonoBehaviour
{
    private TextMeshProUGUI choiceText;
    private Button choiceButton;

    private object choice; // Hem DialogueChoice hem GlobalDialogueChoice için

    public void SetChoice(DialogueChoice choice)
    {
        this.choice = choice;
        SetChoiceText(choice.text);
    }
    


    void Awake()
    {
        choiceButton = GetComponent<Button>();
        choiceText = GetComponentInChildren<TextMeshProUGUI>();
        choiceButton.onClick.AddListener(OnChoiceButtonClicked);
    }
    
    public void SetChoiceText(string text)
    {
        choiceText.text = text;
    }

    private void OnChoiceButtonClicked()
    {
        if (choice == null) return;
        
        if (choice is DialogueChoice dialogueChoice)
        {
            Debug.Log($"Dialogue choice clicked: {dialogueChoice.text}");
            
            // Global choice kontrolü
            if (dialogueChoice.isGlobalChoice)
            {
                Debug.Log("Global choice seçildi!");
                // Global choice için özel işlemler
                OnGlobalChoiceSelected(dialogueChoice);
                return;
            }
            
            // Normal diyalog: Sadece aktif ülkeyi etkiler
            ChoiceEffect effect = new ChoiceEffect(
                dialogueChoice.trustChange, 
                dialogueChoice.faithChange, 
                dialogueChoice.hostilityChange
            );
            
            // Eğer nextNodeId doluysa, ilgili DialogueNode'u sonraki adımda zorunlu göstermek için MapManager'a (ÖNCE) bildir
            DialogueNode preparedNext = null;
            if (!string.IsNullOrEmpty(dialogueChoice.nextNodeId))
            {
                var db = DialogueManager.Instance.dialogueDatabase;
                if (db != null && db.generalDialogues != null)
                {
                    preparedNext = db.generalDialogues.Find(n => n.id == dialogueChoice.nextNodeId);
                    if (preparedNext != null)
                    {
                        MapManager.Instance.PrepareForcedNextDialogue(preparedNext);
                    }
                }
            }

            // Seçimi uygula (tek olay kaynağı)
            GameManager.MakeChoice(effect);
            
            // UI event tetikleme yok; akış GameManager.OnChoiceMade üzerinden ilerler
            // Eğer forced next yoksa da MapManager normal akışı başlatır
            
            return;
        }

    }
    
    private void OnGlobalChoiceSelected(DialogueChoice globalChoice)
    {
        // Global choice seçimi sonrası işlemler
        Debug.Log($"Global choice seçildi: {globalChoice.text}");
        
        // Global effects'i uygula (şimdilik boş, çünkü DialogueChoice'ta global effects yok)
        // Bu kısım için GlobalDialogueChoice'tan bilgi almamız gerekiyor
        ApplyGlobalEffectsFromDialogueChoice(globalChoice);
        
        // 1. Diyalog ekranını kapat
        var choiceSelectionUI = UnityEngine.Object.FindFirstObjectByType<ChoiceSelectionUI>();
        if (choiceSelectionUI != null)
        {
            choiceSelectionUI.OnPanelClosed();
        }
        
        // 2. Aktif ülkenin butonunu pasif yap
        var currentMap = MapManager.Instance.GetCurrentMap();
        if (currentMap.HasValue)
        {
            DisableMapButton(currentMap.Value);
        }
        
        // 3. Event'i tetikle
        var choiceUI = UnityEngine.Object.FindFirstObjectByType<ChoiceSelectionUI>();
        if (choiceUI != null)
        {
            choiceUI.TriggerDialogueChoiceEvent();
        }
    }
    
    private void ApplyGlobalEffectsFromDialogueChoice(DialogueChoice globalChoice)
    {
        // Global diyalog referansını al
        var currentGlobalDialogue = MapManager.Instance.GetCurrentGlobalDialogue();
        if (currentGlobalDialogue == null)
        {
            Debug.LogWarning("Global diyalog referansı bulunamadı!");
            return;
        }
        
        // Seçilen choice'ın index'ini bul
        int choiceIndex = -1;
        for (int i = 0; i < currentGlobalDialogue.choices.Count; i++)
        {
            if (currentGlobalDialogue.choices[i].text == globalChoice.text)
            {
                choiceIndex = i;
                break;
            }
        }
        
        if (choiceIndex >= 0)
        {
            // Global effects'i uygula
            var originalGlobalChoice = currentGlobalDialogue.choices[choiceIndex];
            ApplyGlobalEffectsFromGlobalChoice(originalGlobalChoice);
        }
        else
        {
            Debug.LogWarning("Global choice bulunamadı!");
        }
    }
    
    private void ApplyGlobalEffectsFromGlobalChoice(GlobalDialogueChoice globalChoice)
    {
        var globalEffect = new GlobalDialogueEffect();
        
        if (globalChoice.globalEffects != null && globalChoice.globalEffects.Count > 0)
        {
            // Özel global effects varsa onları kullan
            globalEffect.countryEffects = new Dictionary<MapType, BarValues>();
            foreach (var countryEffect in globalChoice.globalEffects)
            {
                globalEffect.countryEffects[countryEffect.country] = new BarValues
                {
                    trust = countryEffect.trustChange,
                    faith = countryEffect.faithChange,
                    hostility = countryEffect.hostilityChange
                };
            }
        }
        else
        {
            // Global effects yoksa hiçbir şey yapma
            Debug.LogWarning("GlobalDialogueChoice'ta global effects tanımlanmamış!");
            return;
        }
        
        GameManager.Instance.ApplyGlobalDialogueEffect(globalEffect);
    }
    

    
    private void OnGlobalDialogueChoiceCompleted()
    {
        // 1. Diyalog ekranını kapat
        var choiceSelectionUI = UnityEngine.Object.FindFirstObjectByType<ChoiceSelectionUI>();
        if (choiceSelectionUI != null)
        {
            choiceSelectionUI.gameObject.SetActive(false);
        }
        
        // 2. Aktif ülkenin butonunu pasif yap
        var currentMap = MapManager.Instance.GetCurrentMap();
        if (currentMap.HasValue)
        {
            DisableMapButton(currentMap.Value);
        }
        
        // 3. Event'i tetikle
        var choiceUI = UnityEngine.Object.FindFirstObjectByType<ChoiceSelectionUI>();
        if (choiceUI != null)
        {
            choiceUI.TriggerDialogueChoiceEvent();
        }
    }
    
    private void DisableMapButton(MapType mapType)
    {
        // Tüm MapSelectionButton'ları bul
        var mapButtons = UnityEngine.Object.FindObjectsOfType<MapSelectionButton>();
        foreach (var mapButton in mapButtons)
        {
            if (mapButton.GetMapType() == mapType)
            {
                mapButton.SetButtonInteractable(false);
                Debug.Log($"Map butonu pasif yapıldı: {mapType}");
                break;
            }
        }
    }
    
}
