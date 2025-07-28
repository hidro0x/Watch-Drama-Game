using UnityEngine;

/// <summary>
/// Turn sistemini test etmek için debug script'i
/// </summary>
public class TurnSystemDebug : MonoBehaviour
{
    [Header("Test Ayarları")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private void Start()
    {
        if (enableDebugLogs)
        {
            Debug.Log("TurnSystemDebug başlatıldı. Turn sistemini izliyor...");
        }
    }
    
    [ContextMenu("Turn Bilgilerini Göster")]
    public void ShowTurnInfo()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogError("MapManager bulunamadı!");
            return;
        }
        
        if (DialogueManager.Instance == null)
        {
            Debug.LogError("DialogueManager bulunamadı!");
            return;
        }
        
        var currentMap = MapManager.Instance.GetCurrentMap();
        var currentTurn = DialogueManager.Instance.GetCurrentTurn();
        
        Debug.Log("=== TURN BİLGİLERİ ===");
        Debug.Log($"Mevcut Harita: {currentMap}");
        Debug.Log($"Genel Turn: {currentTurn}");
        
        if (currentMap.HasValue)
        {
            var mapTurns = MapManager.Instance.GetMapTurns();
            if (mapTurns.ContainsKey(currentMap.Value))
            {
                Debug.Log($"Harita Turn: {mapTurns[currentMap.Value]}");
            }
        }
        
        // Database bilgileri
        if (MapManager.Instance.dialogueDatabase != null)
        {
            var db = MapManager.Instance.dialogueDatabase;
            Debug.Log($"Genel Diyalog Sayısı: {db.generalDialogues.Count}");
            Debug.Log($"Global Diyalog Sayısı: {db.globalDialogueEffects.Count}");
            
            if (currentMap.HasValue)
            {
                if (db.specialGeneralDialoguesByMap.ContainsKey(currentMap.Value))
                {
                    Debug.Log($"Map'e Özel Diyalog Sayısı: {db.specialGeneralDialoguesByMap[currentMap.Value].Count}");
                }
                else
                {
                    Debug.Log("Bu harita için özel diyalog yok!");
                }
            }
        }
        
        Debug.Log("=== TURN ARALIKLARI ===");
        Debug.Log($"Map'e Özel Aralık: {MapManager.Instance.dialogueDatabase.mapSpecificInterval}");
        Debug.Log($"Global Aralık: {MapManager.Instance.dialogueDatabase.globalDialogueInterval}");
    }
    
    [ContextMenu("Sonraki Turn'ü Test Et")]
    public void TestNextTurn()
    {
        if (MapManager.Instance != null)
        {
            Debug.Log("Manuel turn testi başlatılıyor...");
            MapManager.Instance.StartNextDialogue();
        }
    }
    
    [ContextMenu("Turn Sistemini Sıfırla")]
    public void ResetTurnSystem()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.ResetAllMaps();
            Debug.Log("Turn sistemi sıfırlandı!");
        }
    }
} 