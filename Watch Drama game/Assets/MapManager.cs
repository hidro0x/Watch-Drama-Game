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
    private DialogueNode lastDialogue = null;
    private Dictionary<MapType, DialogueNode> lastDialoguePerMap = new Dictionary<MapType, DialogueNode>();
    
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
        // Eğer kayıtlı diyalog varsa turn atlamadan onu göster
        if (lastDialoguePerMap.TryGetValue(mapType, out var savedNode) && savedNode != null)
        {
            DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
            if (dialogueManager != null)
                dialogueManager.ShowSpecificDialogue(savedNode);
        }
        else
        {
            lastDialogue = null;
            StartNextDialogue();
        }
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
        // Return butonunu tur sayısına göre aktif/pasif yap
        var choiceUI = UnityEngine.Object.FindFirstObjectByType<ChoiceSelectionUI>(FindObjectsInactive.Include);
        if (choiceUI != null)
        {
            bool isActive = mapTurns[currentMap.Value] >= 15;
            choiceUI.SetReturnButtonActive(isActive);
        }
        
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
        List<DialogueNode> pool = dialogueDatabase.generalDialogues;
        if (pool.Count == 0) return null;
        // Son kullanılan diyalogu havuzdan çıkar
        List<DialogueNode> filtered = new List<DialogueNode>(pool);
        if (lastDialogue != null && filtered.Count > 1)
            filtered.Remove(lastDialogue);
        DialogueNode selected = filtered[UnityEngine.Random.Range(0, filtered.Count)];
        lastDialogue = selected;
        return selected;
    }
    
    // Artık kullanılmıyor: GetAvailableDialogues
    
    // Diyalog uygunluk kontrolü kaldırıldı (min/max yok)
    
    // Harita tamamlama fonksiyonu kaldırıldı (isCompleted yok)
    
    // Getter methodları
    public MapType? GetCurrentMap() => currentMap;
    public List<MapType> GetAllMaps() => dialogueDatabase.maps;
    
    // Yeni oyun başlatma için haritaları sıfırla
    public void ResetAllMaps()
    {
        mapTurns.Clear();
        currentMap = null;
        lastDialogue = null;
    }

    public void SaveDialogueForCurrentMap(DialogueNode node)
    {
        if (currentMap != null)
            lastDialoguePerMap[currentMap.Value] = node;
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