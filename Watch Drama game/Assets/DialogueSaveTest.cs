using UnityEngine;

/// <summary>
/// Diyalog kaydetme sisteminin kaldırıldığını test etmek için debug script'i
/// </summary>
public class DialogueSaveTest : MonoBehaviour
{
    [Header("Test Ayarları")]
    [SerializeField] private bool enableDebugLogs = true;
    
    private void Start()
    {
        if (enableDebugLogs)
        {
            Debug.Log("DialogueSaveTest başlatıldı. Diyalog kaydetme sisteminin kaldırıldığını test ediyor...");
        }
    }
    
    [ContextMenu("Diyalog Kaydetme Sistemini Kontrol Et")]
    public void CheckDialogueSaveSystem()
    {
        if (MapManager.Instance == null)
        {
            Debug.LogError("MapManager bulunamadı!");
            return;
        }
        
        Debug.Log("=== DİYALOG KAYDETME SİSTEMİ KONTROLÜ ===");
        
        // MapManager'da diyalog kaydetme alanları kontrol et
        var mapManagerType = typeof(MapManager);
        var lastDialogueField = mapManagerType.GetField("lastDialogue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var lastDialoguePerMapField = mapManagerType.GetField("lastDialoguePerMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var saveDialogueMethod = mapManagerType.GetMethod("SaveDialogueForCurrentMap");
        
        if (lastDialogueField == null)
        {
            Debug.Log("✅ lastDialogue alanı kaldırıldı");
        }
        else
        {
            Debug.LogError("❌ lastDialogue alanı hala mevcut!");
        }
        
        if (lastDialoguePerMapField == null)
        {
            Debug.Log("✅ lastDialoguePerMap alanı kaldırıldı");
        }
        else
        {
            Debug.LogError("❌ lastDialoguePerMap alanı hala mevcut!");
        }
        
        if (saveDialogueMethod == null)
        {
            Debug.Log("✅ SaveDialogueForCurrentMap metodu kaldırıldı");
        }
        else
        {
            Debug.LogError("❌ SaveDialogueForCurrentMap metodu hala mevcut!");
        }
        
        // ChoiceSelectionUI'da diyalog kaydetme kontrol et
        var choiceSelectionUIType = typeof(ChoiceSelectionUI);
        var onPanelClosedMethod = choiceSelectionUIType.GetMethod("OnPanelClosed");
        
        if (onPanelClosedMethod == null)
        {
            Debug.Log("✅ OnPanelClosed metodu kaldırıldı");
        }
        else
        {
            Debug.LogError("❌ OnPanelClosed metodu hala mevcut!");
        }
        
        Debug.Log("=== DİYALOG KAYDETME SİSTEMİ TAMAMEN KALDIRILDI ===");
    }
    
    [ContextMenu("Harita Değiştirme Test Et")]
    public void TestMapChange()
    {
        if (MapManager.Instance != null)
        {
            Debug.Log("Harita değiştirme testi başlatılıyor...");
            
            // Varnan'a geç
            MapManager.Instance.SelectMap(MapType.Varnan);
            Debug.Log("Varnan haritasına geçildi");
            
            // Kısa bir süre sonra Astrahil'e geç
            StartCoroutine(TestMapChangeCoroutine());
        }
    }
    
    private System.Collections.IEnumerator TestMapChangeCoroutine()
    {
        yield return new WaitForSeconds(2f);
        
        if (MapManager.Instance != null)
        {
            MapManager.Instance.SelectMap(MapType.Astrahil);
            Debug.Log("Astrahil haritasına geçildi");
        }
    }
    
    [ContextMenu("Turn Sistemi Test Et")]
    public void TestTurnSystem()
    {
        if (MapManager.Instance != null)
        {
            Debug.Log("Turn sistemi testi başlatılıyor...");
            MapManager.Instance.StartNextDialogue();
        }
    }
    
    [ContextMenu("Tüm Sistemleri Sıfırla")]
    public void ResetAllSystems()
    {
        if (MapManager.Instance != null)
        {
            MapManager.Instance.ResetAllMaps();
            Debug.Log("Tüm sistemler sıfırlandı!");
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
            Debug.Log("Yeni oyun başlatıldı!");
        }
    }
} 