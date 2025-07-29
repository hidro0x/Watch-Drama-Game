using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Newtonsoft.Json;

/// <summary>
/// GlobalDialogue JSON import işlemleri için Unity Editor Window
/// </summary>
public class JsonGlobalDialogueImportWindow : EditorWindow
{
    private TextAsset selectedJsonFile;
    private DialogueDatabase targetDatabase;
    private List<GlobalDialogueNode> importedGlobalDialogues = new List<GlobalDialogueNode>();
    
    [MenuItem("Kahin/Global Dialogue JSON Import")]
    public static void ShowWindow()
    {
        GetWindow<JsonGlobalDialogueImportWindow>("Global Dialogue JSON Import");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Global Dialogue JSON Import Sistemi", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // JSON dosyası seçimi
        GUILayout.Label("JSON Dosyası Seçimi", EditorStyles.boldLabel);
        selectedJsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON Dosyası", selectedJsonFile, typeof(TextAsset), false);
        
        // Database seçimi
        GUILayout.Space(10);
        GUILayout.Label("Hedef Database", EditorStyles.boldLabel);
        targetDatabase = (DialogueDatabase)EditorGUILayout.ObjectField("Dialogue Database", targetDatabase, typeof(DialogueDatabase), false);
        
        // Import butonları
        GUILayout.Space(20);
        
        if (GUILayout.Button("Seçili JSON'u Global Dialogue Import Et", GUILayout.Height(30)))
        {
            ImportSelectedGlobalDialogueJson();
        }
        
        if (GUILayout.Button("Tüm Global Dialogue JSON Dosyalarını Import Et", GUILayout.Height(30)))
        {
            ImportAllGlobalDialogueJsonFiles();
        }
        
        // Sonuçlar
        GUILayout.Space(20);
        GUILayout.Label($"Import Edilen Global Diyaloglar: {importedGlobalDialogues.Count}", EditorStyles.boldLabel);
        
        if (importedGlobalDialogues.Count > 0)
        {
            foreach (var dialogue in importedGlobalDialogues)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"ID: {dialogue.id}");
                EditorGUILayout.LabelField($"Text: {dialogue.text}");
                EditorGUILayout.LabelField($"Choices: {dialogue.choices.Count}");
                EditorGUILayout.EndVertical();
            }
        }
        
        // Temizleme butonu
        if (importedGlobalDialogues.Count > 0)
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Import Edilen Global Diyalogları Temizle"))
            {
                importedGlobalDialogues.Clear();
            }
        }
        
        // Örnek JSON oluşturma
        GUILayout.Space(20);
        if (GUILayout.Button("Örnek Global Dialogue JSON Oluştur"))
        {
            CreateExampleGlobalDialogueJson();
        }
    }
    
    private void ImportSelectedGlobalDialogueJson()
    {
        if (selectedJsonFile == null)
        {
            EditorUtility.DisplayDialog("Hata", "Lütfen bir JSON dosyası seçin!", "Tamam");
            return;
        }
        
        try
        {
            string jsonContent = selectedJsonFile.text;
            
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
                
                if (targetDatabase != null)
                {
                    EditorUtility.SetDirty(targetDatabase);
                    AssetDatabase.SaveAssets();
                }
                
                EditorUtility.DisplayDialog("Başarılı", $"{importedCount} global diyalog başarıyla import edildi.", "Tamam");
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
                    EditorUtility.SetDirty(targetDatabase);
                    AssetDatabase.SaveAssets();
                    EditorUtility.DisplayDialog("Başarılı", $"Global diyalog başarıyla import edildi ve database'e eklendi: {globalDialogueNode.id}", "Tamam");
                }
                else
                {
                    EditorUtility.DisplayDialog("Başarılı", $"Global diyalog başarıyla import edildi: {globalDialogueNode.id}", "Tamam");
                }
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Hata", $"Global Dialogue JSON import hatası: {e.Message}", "Tamam");
        }
    }
    
    private void ImportAllGlobalDialogueJsonFiles()
    {
        // Resources klasöründeki tüm JSON dosyalarını bul
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Dialogues");
        int totalImported = 0;
        
        foreach (var file in jsonFiles)
        {
            if (file.name.EndsWith("_global.json") || file.name.Contains("global"))
            {
                selectedJsonFile = file;
                ImportSelectedGlobalDialogueJson();
                totalImported++;
            }
        }
        
        if (totalImported > 0)
        {
            EditorUtility.DisplayDialog("Başarılı", $"{totalImported} global dialogue JSON dosyası import edildi.", "Tamam");
        }
        else
        {
            EditorUtility.DisplayDialog("Bilgi", "Import edilecek global dialogue JSON dosyası bulunamadı.", "Tamam");
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
    
    private void CreateExampleGlobalDialogueJson()
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
        
        // JSON dosyasını oluştur
        string path = EditorUtility.SaveFilePanel("Örnek Global Dialogue JSON Kaydet", "Assets/Resources/Dialogues", "example_global_dialogue", "json");
        if (!string.IsNullOrEmpty(path))
        {
            System.IO.File.WriteAllText(path, json);
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Başarılı", "Örnek Global Dialogue JSON dosyası oluşturuldu.", "Tamam");
        }
    }
} 