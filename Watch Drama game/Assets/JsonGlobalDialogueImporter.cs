using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

/// <summary>
/// GlobalDialogue JSON formatından import etmek için sistem
/// 
/// Desteklenen JSON formatı:
/// {
///   "id": "global_01",
///   "type": "global",
///   "speaker": "Kahin",
///   "text": "Tüm ülkeler için önemli bir karar...",
///   "choices": [
///     {
///       "text": "Barış yolunu seç",
///       "globalEffects": [
///         { "country": "Agnari", "trustChange": 5, "faithChange": 3, "hostilityChange": -2 },
///         { "country": "Astrahil", "trustChange": 3, "faithChange": 4, "hostilityChange": -1 }
///       ]
///     }
///   ]
/// }
/// </summary>
public class JsonGlobalDialogueImporter : MonoBehaviour
{
    [Header("Import Ayarları")]
    [SerializeField] private TextAsset jsonFile;
    [SerializeField] private DialogueDatabase targetDatabase;
    
    [Header("Import Sonuçları")]
    [SerializeField] private List<GlobalDialogueNode> importedGlobalDialogues = new List<GlobalDialogueNode>();
    
    [ContextMenu("Global Dialogue JSON'dan Import Et")]
    public void ImportGlobalDialogueFromJson()
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
                // Array formatı - her bir global diyalog için ayrı ayrı parse et
                var globalDialogueArray = JsonConvert.DeserializeObject<List<JsonGlobalDialogueData>>(jsonContent);
                int importedCount = 0;
                
                foreach (var jsonData in globalDialogueArray)
                {
                    GlobalDialogueNode globalDialogueNode = ConvertJsonToGlobalDialogueNode(jsonData);
                    importedGlobalDialogues.Add(globalDialogueNode);
                    
                    if (targetDatabase != null)
                    {
                        targetDatabase.globalDialogueEffects.Add(globalDialogueNode);
                    }
                    
                    importedCount++;
                }
                
                Debug.Log($"{importedCount} global diyalog başarıyla import edildi.");
            }
            else
            {
                // Tek global diyalog formatı
                var jsonData = JsonConvert.DeserializeObject<JsonGlobalDialogueData>(jsonContent);
                
                // GlobalDialogueNode'a dönüştür
                GlobalDialogueNode globalDialogueNode = ConvertJsonToGlobalDialogueNode(jsonData);
                
                // Listeye ekle
                importedGlobalDialogues.Add(globalDialogueNode);
                
                // Database'e ekle (opsiyonel)
                if (targetDatabase != null)
                {
                    targetDatabase.globalDialogueEffects.Add(globalDialogueNode);
                    Debug.Log($"Global diyalog başarıyla import edildi ve database'e eklendi: {globalDialogueNode.id}");
                }
                else
                {
                    Debug.Log($"Global diyalog başarıyla import edildi: {globalDialogueNode.id}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Global Dialogue JSON import hatası: {e.Message}");
        }
    }
    
    [ContextMenu("Tüm Global Dialogue JSON Dosyalarını Import Et")]
    public void ImportAllGlobalDialogueJsonFiles()
    {
        // Resources klasöründeki tüm JSON dosyalarını bul
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Dialogues");
        
        foreach (var file in jsonFiles)
        {
            if (file.name.EndsWith("_global.json") || file.name.Contains("global"))
            {
                jsonFile = file;
                ImportGlobalDialogueFromJson();
            }
        }
    }
    
    private GlobalDialogueNode ConvertJsonToGlobalDialogueNode(JsonGlobalDialogueData jsonData)
    {
        GlobalDialogueNode globalDialogueNode = new GlobalDialogueNode
        {
            id = jsonData.id,
            text = $"{jsonData.speaker}: {jsonData.text}",
            choices = new List<GlobalDialogueChoice>()
        };
        
        // Seçenekleri dönüştür
        foreach (var choice in jsonData.choices)
        {
            GlobalDialogueChoice globalChoice = new GlobalDialogueChoice
            {
                text = choice.text,
                globalEffects = new List<CountryBarEffect>()
            };
            
            // Global etkileri dönüştür
            if (choice.globalEffects != null)
            {
                foreach (var effect in choice.globalEffects)
                {
                    CountryBarEffect countryEffect = new CountryBarEffect
                    {
                        country = ParseMapType(effect.country),
                        trustChange = effect.trustChange,
                        faithChange = effect.faithChange,
                        hostilityChange = effect.hostilityChange
                    };
                    
                    globalChoice.globalEffects.Add(countryEffect);
                }
            }
            
            globalDialogueNode.choices.Add(globalChoice);
        }
        
        return globalDialogueNode;
    }
    
    private MapType ParseMapType(string countryName)
    {
        // String'den MapType'a dönüştür
        if (System.Enum.TryParse<MapType>(countryName, out MapType mapType))
        {
            return mapType;
        }
        
        // Alternatif isimler için kontrol
        switch (countryName.ToLower())
        {
            case "agnari":
                return MapType.Agnari;
            case "astharil":
                return MapType.Astrahil;
            case "varnan":
                return MapType.Varnan;
            case "theon":
                return MapType.Theon;
            case "solarya":
                return MapType.Solarya;
            default:
                Debug.LogWarning($"Bilinmeyen ülke adı: {countryName}. Varsayılan olarak Agnari kullanılıyor.");
                return MapType.Agnari;
        }
    }
    
    [ContextMenu("Import Edilen Global Diyalogları Göster")]
    public void ShowImportedGlobalDialogues()
    {
        Debug.Log($"=== IMPORT EDİLEN GLOBAL DİYALOGLAR ({importedGlobalDialogues.Count}) ===");
        foreach (var dialogue in importedGlobalDialogues)
        {
            Debug.Log($"ID: {dialogue.id}");
            Debug.Log($"Text: {dialogue.text}");
            Debug.Log($"Choices: {dialogue.choices.Count}");
            foreach (var choice in dialogue.choices)
            {
                Debug.Log($"  - {choice.text} (Global Effects: {choice.globalEffects.Count})");
            }
            Debug.Log("---");
        }
    }
    
    [ContextMenu("Import Edilen Global Diyalogları Temizle")]
    public void ClearImportedGlobalDialogues()
    {
        importedGlobalDialogues.Clear();
        Debug.Log("Import edilen global diyaloglar temizlendi.");
    }
    
    [ContextMenu("Örnek Global Dialogue JSON Oluştur")]
    public void CreateExampleGlobalDialogueJson()
    {
        var exampleData = new JsonGlobalDialogueData
        {
            id = "global_example_01",
            type = "global",
            speaker = "Kahin",
            text = "Tüm ülkeler için önemli bir karar vermen gerekiyor. Bu karar tüm dünyayı etkileyecek.",
            choices = new List<JsonGlobalDialogueChoice>
            {
                new JsonGlobalDialogueChoice
                {
                    text = "Barış yolunu seç",
                    globalEffects = new List<JsonCountryBarEffect>
                    {
                        new JsonCountryBarEffect { country = "Agnari", trustChange = 5, faithChange = 3, hostilityChange = -2 },
                        new JsonCountryBarEffect { country = "Astrahil", trustChange = 3, faithChange = 4, hostilityChange = -1 },
                        new JsonCountryBarEffect { country = "Varnan", trustChange = 4, faithChange = 2, hostilityChange = -3 },
                        new JsonCountryBarEffect { country = "Theon", trustChange = 2, faithChange = 5, hostilityChange = -1 },
                        new JsonCountryBarEffect { country = "Solarya", trustChange = 3, faithChange = 3, hostilityChange = -2 }
                    }
                },
                new JsonGlobalDialogueChoice
                {
                    text = "Savaş yolunu seç",
                    globalEffects = new List<JsonCountryBarEffect>
                    {
                        new JsonCountryBarEffect { country = "Agnari", trustChange = -3, faithChange = -2, hostilityChange = 5 },
                        new JsonCountryBarEffect { country = "Astrahil", trustChange = -2, faithChange = -1, hostilityChange = 4 },
                        new JsonCountryBarEffect { country = "Varnan", trustChange = -4, faithChange = -3, hostilityChange = 6 },
                        new JsonCountryBarEffect { country = "Theon", trustChange = -1, faithChange = -4, hostilityChange = 3 },
                        new JsonCountryBarEffect { country = "Solarya", trustChange = -2, faithChange = -2, hostilityChange = 4 }
                    }
                },
                new JsonGlobalDialogueChoice
                {
                    text = "Tarafsız kal",
                    globalEffects = new List<JsonCountryBarEffect>
                    {
                        new JsonCountryBarEffect { country = "Agnari", trustChange = 0, faithChange = 1, hostilityChange = 0 },
                        new JsonCountryBarEffect { country = "Astrahil", trustChange = 1, faithChange = 0, hostilityChange = 0 },
                        new JsonCountryBarEffect { country = "Varnan", trustChange = 0, faithChange = 0, hostilityChange = 1 },
                        new JsonCountryBarEffect { country = "Theon", trustChange = 1, faithChange = 1, hostilityChange = 0 },
                        new JsonCountryBarEffect { country = "Solarya", trustChange = 0, faithChange = 0, hostilityChange = 0 }
                    }
                }
            }
        };
        
        string json = JsonConvert.SerializeObject(exampleData, Formatting.Indented);
        Debug.Log("=== ÖRNEK GLOBAL DIALOGUE JSON ===");
        Debug.Log(json);
        Debug.Log("=== JSON SONU ===");
    }
}

/// <summary>
/// JSON formatındaki global diyalog verisi
/// </summary>
[System.Serializable]
public class JsonGlobalDialogueData
{
    public string id;
    public string type;
    public string speaker;
    public string text;
    public List<JsonGlobalDialogueChoice> choices;
}

/// <summary>
/// JSON formatındaki global diyalog seçeneği
/// </summary>
[System.Serializable]
public class JsonGlobalDialogueChoice
{
    public string text;
    public List<JsonCountryBarEffect> globalEffects;
}

/// <summary>
/// JSON formatındaki ülke bar etkisi
/// </summary>
[System.Serializable]
public class JsonCountryBarEffect
{
    public string country;
    public int trustChange;
    public int faithChange;
    public int hostilityChange;
} 