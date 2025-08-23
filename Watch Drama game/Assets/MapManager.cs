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
    
    // Zorunlu gösterilecek (nextNodeId ile gelen) bir sonraki diyalog
    private DialogueNode pendingForcedNode = null;
    
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
        
        // Eğer nextNodeId ile zorunlu bir diyalog planlandıysa onu göster
        if (pendingForcedNode != null)
        {
            var node = pendingForcedNode;
            pendingForcedNode = null;
            ForceShowSpecificDialogueAndAdvanceTurn(node);
            return;
        }
        
        // Normal akış
        StartNextDialogue();
    }
    
    // nextNodeId akışı için önceden haber ver
    public void PrepareForcedNextDialogue(DialogueNode node)
    {
        pendingForcedNode = node;
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
        
        // Şans bazlı rakip karşılaşması kontrolü
        DialogueNode selectedDialogue = TrySelectRivalEncounter();
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
    
    // Zorunlu node gösterme ve turn ilerletme
    public void ForceShowSpecificDialogueAndAdvanceTurn(DialogueNode forcedNode)
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
        
        // Zorunlu node'u göster
        if (dialogueManager != null)
        {
            dialogueManager.ShowSpecificDialogue(forcedNode);
        }
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
        
        return dialogueNode;
    }

    // Basit: Şans bazlı rakip karşılaşması
    // Örnek oran: %15, rakip ülke havuzdan rastgele seçilir (aktif ülke hariç)
    private DialogueNode TrySelectRivalEncounter()
    {
        var db = dialogueDatabase;
        if (db == null || !db.enableRivalEncounters) return null;
        // Turn aralığı kontrolü
        DialogueManager dialogueManager = UnityEngine.Object.FindFirstObjectByType<DialogueManager>();
        int currentTurn = dialogueManager?.GetCurrentTurn() ?? 0;
        if (db.rivalEncounterInterval > 0 && currentTurn % db.rivalEncounterInterval != 0) return null;
        // Şans kontrolü
        float chance = Mathf.Clamp01(db.rivalEncounterChance);
        if (UnityEngine.Random.value > chance) return null;
        if (currentMap == null) return null;
        if (db == null || db.generalDialogues == null || db.generalDialogues.Count == 0) return null;

        // General havuzdan bir diyalog seç, flag'ini set et ve rakibi ata
        DialogueNode candidate = null;
        // Genel havuzda kullanılabilir bir node bul
        for (int i = 0; i < 5; i++)
        {
            var tmp = db.generalDialogues[UnityEngine.Random.Range(0, db.generalDialogues.Count)];
            if (tmp != null && tmp.choices != null && tmp.choices.Count >= 1)
            {
                candidate = tmp;
                break;
            }
        }
        if (candidate == null) return null;

        // Kopya oluşturup işaretle (orijinal asseti kirletmemek için)
        var node = new DialogueNode
        {
            id = candidate.id,
            name = string.IsNullOrEmpty(candidate.name) ? "Rival" : candidate.name,
            sprite = candidate.sprite,
            text = candidate.text,
            backgroundSprite = candidate.backgroundSprite,
            isGlobalDialogue = false,
            isRivalEncounter = true,
            rivalOpponent = PickRandomOpponent(currentMap.Value)
        };
        node.choices = new List<DialogueChoice>();
        foreach (var c in candidate.choices)
        {
            node.choices.Add(new DialogueChoice
            {
                text = c.text,
                trustChange = c.trustChange,
                faithChange = c.faithChange,
                hostilityChange = c.hostilityChange,
                isGlobalChoice = c.isGlobalChoice,
                nextNodeId = c.nextNodeId,
                opponentTrustChange = 0,
                opponentFaithChange = 0,
                opponentHostilityChange = 0
            });
        }
        return node;
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