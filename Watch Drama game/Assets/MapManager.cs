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
        
        // DialogueManager eventlerini dinle
        ChoiceSelectionUI.OnDialogueChoiceMade += OnDialogueChoiceCompleted;
    }
    
    private void OnDestroy()
    {
        ChoiceSelectionUI.OnDialogueChoiceMade -= OnDialogueChoiceCompleted;
    }
    
    // DialogueManager'dan seçim yapıldığı bilgisi gelince çağrılır
    private void OnDialogueChoiceCompleted()
    {
        if (currentMap != null)
        {
            StartNextDialogue();
        }
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