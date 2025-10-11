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
        
        // Check if this was the last turn and a global decision was made
        CheckForMapCompletion();
        
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
        
        // Harita tamamlandı mı kontrolü gerekiyorsa burada yapılabilir
        // Hangi havuzdan diyalog seçileceğini belirle
        DialogueNode selectedDialogue = SelectDialogueFromPool();
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
        Debug.Log($"Global dialogue count: {dialogueDatabase.globalDialogueEffects.Count}, Interval: {dialogueDatabase.globalDialogueInterval}");
        
        // 1. Global diyalog kontrolü
        if (currentTurn % dialogueDatabase.globalDialogueInterval == 0 && currentTurn > 0 && dialogueDatabase.globalDialogueEffects.Count > 0)
        {
            Debug.Log($"Global diyalog seçiliyor... Turn: {currentTurn}, Interval: {dialogueDatabase.globalDialogueInterval}");
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
    
    /// <summary>
    /// Check if the current map should be completed (last turn + global decision made)
    /// </summary>
    private void CheckForMapCompletion()
    {
        if (currentMap == null || currentGlobalDialogue == null) return;
        
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (dialogueManager == null) return;
        
        int currentTurn = dialogueManager.GetCurrentTurn();
        int maxTurns = dialogueManager.GetMaxTurnCount();
        
        // Check if this is the last turn
        if (currentTurn >= maxTurns)
        {
            Debug.Log($"Last turn reached ({currentTurn}/{maxTurns}). Map completion triggered for {currentMap}!");
            
            // Get final stats for the current map
            MapValues finalStats = GameManager.Instance.GetMapValues(currentMap.Value);
            
            // Trigger map completion panel
            MapCompletionPanelUI.TriggerMapCompletion(currentMap.Value, finalStats);
            
            // Mark map as completed
            OnMapCompleted?.Invoke(currentMap.Value);
            
            // Clear current global dialogue reference
            currentGlobalDialogue = null;
        }
    }

    /// <summary>
    /// Manually trigger map completion (for testing purposes)
    /// </summary>
    [ContextMenu("Trigger Map Completion (Debug)")]
    public void TriggerMapCompletionDebug()
    {
        if (currentMap == null)
        {
            Debug.LogWarning("No current map selected!");
            return;
        }
        
        MapValues finalStats = GameManager.Instance.GetMapValues(currentMap.Value);
        MapCompletionPanelUI.TriggerMapCompletion(currentMap.Value, finalStats);
    }

    public DialogueNode ConvertGlobalDialogueToDialogue(GlobalDialogueNode globalDialogue)
    {
        // GlobalDialogueNode'u DialogueNode'a dönüştür
        DialogueNode dialogueNode = new DialogueNode
        {
            id = globalDialogue.id,
            name = globalDialogue.name,
            text = globalDialogue.text,
            sprite = globalDialogue.sprite,
            backgroundSprite = globalDialogue.sprite, // Use the sprite as background for global dialogues
            choices = new List<DialogueChoice>(),
            isGlobalDialogue = true // Global diyalog olduğunu işaretle
        };
        
        Debug.Log($"ConvertGlobalDialogueToDialogue: Set backgroundSprite to {(globalDialogue.sprite != null ? globalDialogue.sprite.name : "NULL")} for dialogue {globalDialogue.id}");
        
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