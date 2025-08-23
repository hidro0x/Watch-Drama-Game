using UnityEngine;
using System.Collections.Generic;
using System;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    
    [Header("Harita Sistemi")]
    public DialogueDatabase dialogueDatabase;
    
    private MapType? currentMap = null;
    private Dictionary<MapType, int> mapTurns = new Dictionary<MapType, int>();
    
    // Global diyalog referansı
    private GlobalDialogueNode currentGlobalDialogue = null;
    

    
    // Her ülkede savaşçı karşılaşması sadece bir kere olsun
    private HashSet<MapType> warriorEncounteredCountries = new HashSet<MapType>();
    
    // Events
    public static event Action<MapType> OnMapSelected;
    public static event Action<MapType> OnMapCompleted;
    public static event Action OnAllMapsCompleted;
    

    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Tek olay kaynağı: GameManager.OnChoiceMade
        GameManager.OnChoiceMade += OnGameChoiceMade;
    }
    
    private void OnDestroy()
    {
        GameManager.OnChoiceMade -= OnGameChoiceMade;
    }
    
    // Seçim uygulandığında (tek olay)
    private void OnGameChoiceMade(ChoiceEffect _)
    {
        if (currentMap == null) return;
        
        // Normal akış
        StartNextDialogue();
    }
    
    public void SelectMap(MapType mapType)
    {
        currentMap = mapType;
        if (!mapTurns.ContainsKey(mapType))
            mapTurns[mapType] = 0;
        Debug.Log($"Harita seçildi: {mapType}");
        OnMapSelected?.Invoke(mapType);
        // Aktif map'i GameManager'a bildir
        GameManager.Instance.SetActiveMap(mapType);
        
        // Harita adını DialogueManager'a gönder
        DialogueManager.Instance.UpdateMapName(mapType.ToString());
        
        StartNextDialogue();
    }
    
    public void StartNextDialogue()
    {
        if (currentMap == null)
        {
            Debug.LogError("Aktif harita yok!");
            return;
        }
        
        DialogueManager dialogueManager = UnityEngine.Object.FindFirstObjectByType<DialogueManager>();
        dialogueManager.NextTurn();

        mapTurns[currentMap.Value]++;
        
        // Turn sonunda condition'ları kontrol et
        CheckTurnConditions();
        
        // Şans bazlı savaşçı karşılaşması kontrolü (sadece genel diyaloglar sırasında)
        DialogueNode selectedDialogue = TrySelectWarriorEncounter();
        if (selectedDialogue == null)
        {
            // Harita tamamlandı mı kontrolü gerekiyorsa burada yapılabilir
            // Hangi havuzdan diyalog seçileceğini belirle
            selectedDialogue = SelectDialogueFromPool();
        }
        if (selectedDialogue != null)
        {
            
            if (dialogueManager != null)
            {
                dialogueManager.ShowSpecificDialogue(selectedDialogue);
            }
        }
        else
        {
            Debug.LogWarning("Uygun diyalog bulunamadı!");
        }
    }
    

    
    /// <summary>
    /// Ülke tamamlanma kontrolü - Global dialogue sonrasında ülke tamamlanır
    /// </summary>
    private void CheckCountryCompletion()
    {
        if (currentMap == null) return;
        
        // Global dialogue sonrasında ülke tamamlanmış sayılır
        // Turn sayısına bakmaksızın global dialogue geldiğinde ülke biter
        
        // Ülke tamamlandı event'ini tetikle
        OnMapCompleted?.Invoke(currentMap.Value);
        
        // CountryCompletionPanel'i bul ve göster
        var completionPanel = UnityEngine.Object.FindFirstObjectByType<CountryCompletionPanel>();
        
        if (completionPanel != null)
        {
            var finalValues = GameManager.Instance.GetMapValues(currentMap.Value);
            
            completionPanel.ShowCountryCompletion(currentMap.Value, finalValues, () => {
                // Tamamlandığında yapılacak işlemler
                Debug.Log($"{currentMap.Value} ülkesi tamamlandı!");
                
                // Complete panel kapandıktan sonra global diyalog göster ve ülkeden çık
                ShowGlobalDialogueAndExitCountry();
            });
        }
        else
        {
            Debug.LogError("CountryCompletionPanel bulunamadı!");
        }
    }
    
    /// <summary>
    /// Complete panel kapandıktan sonra global diyalog göster ve ülkeden çık
    /// </summary>
    private void ShowGlobalDialogueAndExitCountry()
    {
        // Global diyalog havuzundan rastgele bir diyalog seç
        if (dialogueDatabase.globalDialogueEffects.Count > 0)
        {
            var globalDialogue = dialogueDatabase.globalDialogueEffects[UnityEngine.Random.Range(0, dialogueDatabase.globalDialogueEffects.Count)];
            currentGlobalDialogue = globalDialogue;
            
            // Global diyalogu DialogueNode'a çevir ve göster
            var dialogueNode = ConvertGlobalDialogueToDialogue(globalDialogue);
            
            if (dialogueNode != null)
            {
                // ChoiceSelectionUI'ya direkt gönder
                var choiceUI = UnityEngine.Object.FindFirstObjectByType<ChoiceSelectionUI>();
                if (choiceUI != null)
                {
                    choiceUI.ShowUI(dialogueNode);
                }
            }
        }
        
        // Global diyalog sonrasında ülkeden çık
        ExitCurrentCountry();
    }
    
    /// <summary>
    /// Mevcut ülkeden çık ve harita seçim ekranına dön
    /// </summary>
    private void ExitCurrentCountry()
    {
        if (currentMap == null) return;
        
        Debug.Log($"{currentMap.Value} ülkesinden çıkılıyor...");
        
        // Mevcut ülkeyi null yap
        currentMap = null;
        
        // Harita seçim ekranına dön (bu kısmı daha sonra implement edebiliriz)
        // Örneğin: MapSelectionUI.SetActive(true);
        
        // Başka ülke var mı kontrol et
        CheckAllMapsCompletion();
    }
    
    /// <summary>
    /// Tüm ülkelerin tamamlanıp tamamlanmadığını kontrol eder
    /// Artık global dialogue sonrasında ülkeler tamamlandığı için
    /// bu kontrol sadece tüm ülkelerin global dialogue'larını görmüş olup olmadığını kontrol eder
    /// </summary>
    private void CheckAllMapsCompletion()
    {
        // Bu metod artık global dialogue sistemi ile çalıştığı için
        // farklı bir mantık gerekebilir. Şimdilik basit tutuyoruz.
        // İleride global dialogue sayısına göre kontrol eklenebilir.
        
        Debug.Log("Tüm ülkeler tamamlandı kontrolü yapılıyor...");
        // Bu kısmı daha sonra global dialogue sayısına göre güncelleyebiliriz
    }
    
    /// <summary>
    /// Turn sonunda condition'ları kontrol eder
    /// </summary>
    private void CheckTurnConditions()
    {
        var turnConditionSystem = UnityEngine.Object.FindFirstObjectByType<TurnConditionSystem>();
        if (turnConditionSystem != null)
        {
            turnConditionSystem.CheckTurnConditions();
        }
    }
    
    private DialogueNode SelectDialogueFromPool()
    {
        DialogueManager dialogueManager = UnityEngine.Object.FindFirstObjectByType<DialogueManager>();
        int currentTurn = dialogueManager?.GetCurrentTurn() ?? 0;
        int mapTurn = mapTurns[currentMap.Value];
        
        Debug.Log($"Turn: {currentTurn}, Map Turn: {mapTurn}, Current Map: {currentMap}");
        
        // 1. Global diyalog kontrolü
        if (currentTurn % dialogueDatabase.globalDialogueInterval == 0 && dialogueDatabase.globalDialogueEffects.Count > 0)
        {
            Debug.Log("Global diyalog seçiliyor...");
            var globalDialogue = dialogueDatabase.globalDialogueEffects[UnityEngine.Random.Range(0, dialogueDatabase.globalDialogueEffects.Count)];
            currentGlobalDialogue = globalDialogue; // Global diyalog referansını sakla
            return ConvertGlobalDialogueToDialogue(globalDialogue);
        }
        
        // 2. Map'e özel diyalog kontrolü
        if (mapTurn % dialogueDatabase.mapSpecificInterval == 0 && currentMap != null)
        {
            if (dialogueDatabase.specialGeneralDialoguesByMap.ContainsKey(currentMap.Value) && 
                dialogueDatabase.specialGeneralDialoguesByMap[currentMap.Value].Count > 0)
            {
                Debug.Log($"Map'e özel diyalog seçiliyor... ({currentMap})");
                var mapDialogues = dialogueDatabase.specialGeneralDialoguesByMap[currentMap.Value];
                var selectedDialogue = mapDialogues[UnityEngine.Random.Range(0, mapDialogues.Count)];
                return selectedDialogue;
            }
        }
        
        // 3. Genel diyalog (varsayılan)
        List<DialogueNode> pool = dialogueDatabase.generalDialogues;
        if (pool.Count == 0) return null;
        
        DialogueNode selected = pool[UnityEngine.Random.Range(0, pool.Count)];
        return selected;
    }
    
    public DialogueNode ConvertGlobalDialogueToDialogue(GlobalDialogueNode globalDialogue)
    {
        // GlobalDialogueNode'u DialogueNode'a dönüştür
        DialogueNode dialogueNode = new DialogueNode
        {
            id = globalDialogue.id,
            text = globalDialogue.text,
            choices = new List<DialogueChoice>(),
            isGlobalDialogue = true // Global diyalog olduğunu işaretle
        };
        
        // GlobalDialogueChoice'ları DialogueChoice'a dönüştür
        foreach (var globalChoice in globalDialogue.choices)
        {
            DialogueChoice choice = new DialogueChoice
            {
                text = globalChoice.text,
                trustChange = 0, // Global diyaloglarda yerel değişiklik yok
                faithChange = 0,
                hostilityChange = 0,
                isGlobalChoice = true // Global choice olduğunu işaretle
            };
            
            dialogueNode.choices.Add(choice);
        }
        
        // Global diyalog sonrasında ülke tamamlama kontrolü
        CheckCountryCompletion();
        
        return dialogueNode;
    }

    // Savaşçı karşılaşması - sadece genel diyaloglar sırasında ve her ülkede bir kere
    private DialogueNode TrySelectWarriorEncounter()
    {
        var db = dialogueDatabase;
        if (db == null || !db.enableWarriorEncounters) return null;
        if (currentMap == null) return null;
        
        // Bu ülkede daha önce savaşçı karşılaşması oldu mu kontrol et
        if (warriorEncounteredCountries.Contains(currentMap.Value)) return null;
        
        // Şans kontrolü
        float chance = Mathf.Clamp01(db.warriorEncounterChance);
        if (UnityEngine.Random.value > chance) return null;
        
        // Rastgele bir rakip ülke savaşçısı seç
        var opponentCountry = PickRandomOpponent(currentMap.Value);
        var warrior = GetWarriorForCountry(opponentCountry);
        if (warrior == null) return null;
        
        // Bu ülkede savaşçı karşılaşması olduğunu işaretle
        warriorEncounteredCountries.Add(currentMap.Value);
        
        // Dinamik taraf seçme diyalogu oluştur
        var node = new DialogueNode
        {
            id = $"warrior_{opponentCountry}",
            name = warrior.warriorName,
            sprite = warrior.warriorSprite,
            text = warrior.allianceProposalText,
            backgroundSprite = null,
            isGlobalDialogue = false
        };
        
        // İki seçenek: Savaşçının tarafına katıl veya mevcut ülkeye yardım et
        node.choices = new List<DialogueChoice>();
        
        // Savaşçının tarafına katıl
        node.choices.Add(new DialogueChoice
        {
            text = warrior.joinWarriorSideText,
            trustChange = warrior.joinWarriorTrust,
            faithChange = warrior.joinWarriorFaith,
            hostilityChange = warrior.joinWarriorHostility,
            isGlobalChoice = false,
            opponentTrustChange = 3,
            opponentFaithChange = 2,
            opponentHostilityChange = 5
        });
        
        // Mevcut ülkeye yardım et
        node.choices.Add(new DialogueChoice
        {
            text = warrior.helpCurrentCountryText,
            trustChange = warrior.helpCurrentTrust,
            faithChange = warrior.helpCurrentFaith,
            hostilityChange = warrior.helpCurrentHostility,
            isGlobalChoice = false,
            opponentTrustChange = -2,
            opponentFaithChange = -1,
            opponentHostilityChange = 1
        });
        
        return node;
    }
    
    private CountryWarrior GetWarriorForCountry(MapType country)
    {
        var db = dialogueDatabase;
        if (db == null || db.countryWarriors == null) return null;
        
        foreach (var warrior in db.countryWarriors)
        {
            if (warrior.country == country) return warrior;
        }
        return null;
    }

    private MapType PickRandomOpponent(MapType exclude)
    {
        var all = GetAllMaps();
        var candidates = new List<MapType>();
        foreach (var m in all)
        {
            if (m != exclude) candidates.Add(m);
        }
        if (candidates.Count == 0) return exclude;
        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }
    
    // Artık kullanılmıyor: GetAvailableDialogues
    
    // Diyalog uygunluk kontrolü kaldırıldı (min/max yok)
    
    // Harita tamamlama fonksiyonu kaldırıldı (isCompleted yok)
    
    // Getter methodları
    public MapType? GetCurrentMap() => currentMap;
    public List<MapType> GetAllMaps() => dialogueDatabase.maps;
    public Dictionary<MapType, int> GetMapTurns() => mapTurns;
    public GlobalDialogueNode GetCurrentGlobalDialogue() => currentGlobalDialogue;
    
    // Yeni oyun başlatma için haritaları sıfırla
    public void ResetAllMaps()
    {
        mapTurns.Clear();
        currentMap = null;
        currentGlobalDialogue = null; // Global diyalog referansını da temizle
        warriorEncounteredCountries.Clear(); // Savaşçı karşılaşma geçmişini temizle
    }


} 

[System.Serializable]
public enum MapType{
    Astrahil,
    Agnari,
    Solarya,
    Theon,
    Varnan,
}