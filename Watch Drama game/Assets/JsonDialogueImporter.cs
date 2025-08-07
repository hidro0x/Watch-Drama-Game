using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// JSON formatından diyalog import etmek için sistem
/// 
/// Desteklenen JSON formatı:
/// {
///   "id": "n15",
///   "name": "Kayıp Ruh Avcısı",
///   "type": "normal",
///   "speaker": "Kayıp Ruh Avcısı",
///   "text": "Bazı ruhlar geri dönmez...",
///   "options": [
///     { "text": "Hayır, yolum ileri.", "effects": { "faith": 4, "trust": 3, "hostility": 1 } },
///     { "text": "Belki bir gün...", "effects": { "faith": 5, "trust": 4, "hostility": 0 } }
///   ]
/// }
/// </summary>
public class JsonDialogueImporter : MonoBehaviour
{
    [Header("Import Ayarları")]
    [SerializeField] private TextAsset jsonFile;
    [SerializeField] private DialogueDatabase targetDatabase;
    
    [Header("Import Sonuçları")]
    [SerializeField] private List<DialogueNode> importedDialogues = new List<DialogueNode>();
    
    [ContextMenu("JSON'dan Import Et")]
    public void ImportFromJson()
    {
        if (jsonFile == null)
        {
            Debug.LogError("JSON dosyası seçilmemiş!");
            return;
        }
        
        try
        {
            string jsonContent = jsonFile.text;
            
            // JSON array formatını kontrol et
            if (jsonContent.Trim().StartsWith("["))
            {
                // Array formatı - her bir diyalog için ayrı ayrı parse et
                var dialogueArray = JsonConvert.DeserializeObject<List<JsonDialogueData>>(jsonContent);
                int importedCount = 0;
                
                foreach (var jsonData in dialogueArray)
                {
                    DialogueNode dialogueNode = ConvertJsonToDialogueNode(jsonData);
                    importedDialogues.Add(dialogueNode);
                    
                    if (targetDatabase != null)
                    {
                        targetDatabase.generalDialogues.Add(dialogueNode);
                    }
                    
                    importedCount++;
                }
                
                Debug.Log($"{importedCount} diyalog başarıyla import edildi.");
            }
            else
            {
                // Tek diyalog formatı
                var jsonData = JsonConvert.DeserializeObject<JsonDialogueData>(jsonContent);
                
                // DialogueNode'a dönüştür
                DialogueNode dialogueNode = ConvertJsonToDialogueNode(jsonData);
                
                // Listeye ekle
                importedDialogues.Add(dialogueNode);
                
                // Database'e ekle (opsiyonel)
                if (targetDatabase != null)
                {
                    targetDatabase.generalDialogues.Add(dialogueNode);
                    Debug.Log($"Diyalog başarıyla import edildi ve database'e eklendi: {dialogueNode.id}");
                }
                else
                {
                    Debug.Log($"Diyalog başarıyla import edildi: {dialogueNode.id}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON import hatası: {e.Message}");
        }
    }
    
    [ContextMenu("Tüm JSON Dosyalarını Import Et")]
    public void ImportAllJsonFiles()
    {
        // Resources klasöründeki tüm JSON dosyalarını bul
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Dialogues");
        
        foreach (var file in jsonFiles)
        {
            if (file.name.EndsWith(".json"))
            {
                jsonFile = file;
                ImportFromJson();
            }
        }
    }
    
    private DialogueNode ConvertJsonToDialogueNode(JsonDialogueData jsonData)
    {
        DialogueNode dialogueNode = new DialogueNode
        {
            id = jsonData.id,
            name = jsonData.name ?? jsonData.speaker, // name yoksa speaker'ı kullan
            text = $"{jsonData.speaker}: {jsonData.text}",
            choices = new List<DialogueChoice>()
        };
        
        // Seçenekleri dönüştür
        foreach (var option in jsonData.options)
        {
            DialogueChoice choice = new DialogueChoice
            {
                text = option.text,
                trustChange = option.effects?.trust ?? 0,
                faithChange = option.effects?.faith ?? 0,
                hostilityChange = option.effects?.hostility ?? 0
            };
            
            dialogueNode.choices.Add(choice);
        }
        
        return dialogueNode;
    }
    
    [ContextMenu("Import Edilen Diyalogları Göster")]
    public void ShowImportedDialogues()
    {
        Debug.Log($"=== IMPORT EDİLEN DİYALOGLAR ({importedDialogues.Count}) ===");
        foreach (var dialogue in importedDialogues)
        {
            Debug.Log($"ID: {dialogue.id}");
            Debug.Log($"Text: {dialogue.text}");
            Debug.Log($"Choices: {dialogue.choices.Count}");
            Debug.Log("---");
        }
    }
    
    [ContextMenu("Import Edilen Diyalogları Temizle")]
    public void ClearImportedDialogues()
    {
        importedDialogues.Clear();
        Debug.Log("Import edilen diyaloglar temizlendi.");
    }
}

/// <summary>
/// JSON formatındaki diyalog verisi
/// </summary>
[System.Serializable]
public class JsonDialogueData
{
    public string id;
    public string name;
    public string type;
    public string speaker;
    public string text;
    public List<JsonDialogueOption> options;
}

/// <summary>
/// JSON formatındaki diyalog seçeneği
/// </summary>
[System.Serializable]
public class JsonDialogueOption
{
    public string text;
    public JsonDialogueEffects effects;
}

/// <summary>
/// JSON formatındaki diyalog etkileri
/// </summary>
[System.Serializable]
public class JsonDialogueEffects
{
    public int faith;
    public int trust;
    public int hostility;
} 