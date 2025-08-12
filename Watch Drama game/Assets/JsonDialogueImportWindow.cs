using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Unity Editor'da JSON diyalog import etmek için window
/// </summary>
public class JsonDialogueImportWindow : EditorWindow
{
    private TextAsset selectedJsonFile;
    private DialogueDatabase targetDatabase;
    private Vector2 scrollPosition;
    private List<DialogueNode> importedDialogues = new List<DialogueNode>();
    private bool showImportedDialogues = false;
    
    [MenuItem("Kahin/JSON Diyalog Import")]
    public static void ShowWindow()
    {
        GetWindow<JsonDialogueImportWindow>("JSON Diyalog Import");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("JSON Diyalog Import Sistemi", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // JSON dosyası seçimi
        GUILayout.Label("JSON Dosyası:", EditorStyles.boldLabel);
        selectedJsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON File", selectedJsonFile, typeof(TextAsset), false);
        
        // Database seçimi
        GUILayout.Label("Hedef Database:", EditorStyles.boldLabel);
        targetDatabase = (DialogueDatabase)EditorGUILayout.ObjectField("Target Database", targetDatabase, typeof(DialogueDatabase), false);
        
        GUILayout.Space(10);
        
        // Import butonları
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Seçili JSON'u Import Et"))
        {
            ImportSelectedJson();
        }
        if (GUILayout.Button("Tüm JSON Dosyalarını Import Et"))
        {
            ImportAllJsonFiles();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        // Map'e özel import butonu
        if (GUILayout.Button("Map'e Özel Diyalog Import Et"))
        {
            ImportMapSpecificDialogue();
        }
        
        GUILayout.Space(10);
        
        // Import edilen diyaloglar
        if (importedDialogues.Count > 0)
        {
            showImportedDialogues = EditorGUILayout.Foldout(showImportedDialogues, $"Import Edilen Diyaloglar ({importedDialogues.Count})");
            
            if (showImportedDialogues)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                for (int i = 0; i < importedDialogues.Count; i++)
                {
                    var dialogue = importedDialogues[i];
                    
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"ID: {dialogue.id}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Text: {dialogue.text}");
                    EditorGUILayout.LabelField($"Choices: {dialogue.choices.Count}");
                    
                    // Seçenekleri göster
                    if (dialogue.choices.Count > 0)
                    {
                        EditorGUILayout.LabelField("Choices:", EditorStyles.boldLabel);
                        for (int j = 0; j < dialogue.choices.Count; j++)
                        {
                            var choice = dialogue.choices[j];
                            EditorGUILayout.LabelField($"  {j + 1}. {choice.text}");
                            EditorGUILayout.LabelField($"     Trust: {choice.trustChange}, Faith: {choice.faithChange}, Hostility: {choice.hostilityChange}");
                        }
                    }
                    
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                }
                
                EditorGUILayout.EndScrollView();
                
                if (GUILayout.Button("Import Edilen Diyalogları Temizle"))
                {
                    importedDialogues.Clear();
                }
            }
        }
        
        // JSON formatı örneği
        GUILayout.Space(10);
        if (EditorGUILayout.Foldout(false, "JSON Format Örneği"))
        {
            EditorGUILayout.HelpBox(
                @"{
  ""id"": ""n15"",
  ""type"": ""normal"",
  ""speaker"": ""Kayıp Ruh Avcısı"",
  ""text"": ""Bazı ruhlar geri dönmez. Sen geri dönmeyi düşünüyor musun?"",
  ""options"": [
    { ""text"": ""Hayır, yolum ileri."", ""effects"": { ""faith"": 4, ""trust"": 3, ""hostility"": 1 } },
    { ""text"": ""Belki bir gün, ama henüz değil."", ""effects"": { ""faith"": 5, ""trust"": 4, ""hostility"": 0 } },
    { ""text"": ""Dönüşüm yok. Sonuna kadar gideceğim."", ""effects"": { ""faith"": 2, ""trust"": 2, ""hostility"": 3 } }
  ]
}", MessageType.Info);
        }
    }
    
    private void ImportSelectedJson()
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
                // Array formatı - Newtonsoft.Json kullanarak parse et
                var dialogueArray = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JsonDialogueData>>(jsonContent);
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
                
                if (targetDatabase != null)
                {
                    EditorUtility.SetDirty(targetDatabase);
                    AssetDatabase.SaveAssets();
                }
                
                Debug.Log($"{importedCount} diyalog başarıyla import edildi.");
                EditorUtility.DisplayDialog("Başarılı", $"{importedCount} diyalog başarıyla import edildi.", "Tamam");
            }
            else
            {
                // Tek diyalog formatı
                var jsonData = JsonUtility.FromJson<JsonDialogueData>(jsonContent);
                DialogueNode dialogueNode = ConvertJsonToDialogueNode(jsonData);
                
                // Listeye ekle
                importedDialogues.Add(dialogueNode);
                
                // Database'e ekle (opsiyonel)
                if (targetDatabase != null)
                {
                    targetDatabase.generalDialogues.Add(dialogueNode);
                    EditorUtility.SetDirty(targetDatabase);
                    AssetDatabase.SaveAssets();
                }
                
                Debug.Log($"Diyalog başarıyla import edildi: {dialogueNode.id}");
                EditorUtility.DisplayDialog("Başarılı", $"Diyalog başarıyla import edildi: {dialogueNode.id}", "Tamam");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON import hatası: {e.Message}");
            EditorUtility.DisplayDialog("Hata", $"JSON import hatası: {e.Message}", "Tamam");
        }
    }
    
    private void ImportMapSpecificDialogue()
    {
        if (selectedJsonFile == null)
        {
            EditorUtility.DisplayDialog("Hata", "Lütfen bir JSON dosyası seçin!", "Tamam");
            return;
        }
        
        try
        {
            string jsonContent = selectedJsonFile.text;
            
            // Map'e özel diyalog formatını kontrol et
            if (jsonContent.Contains("\"mapType\""))
            {
                var mapSpecificData = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonMapSpecificDialogueData>(jsonContent);
                
                // MapType'ı parse et
                if (System.Enum.TryParse<MapType>(mapSpecificData.mapType, out MapType mapType))
                {
                    int importedCount = 0;
                    
                    foreach (var jsonData in mapSpecificData.dialogues)
                    {
                        DialogueNode dialogueNode = ConvertJsonToDialogueNode(jsonData);
                        importedDialogues.Add(dialogueNode);
                        
                        if (targetDatabase != null)
                        {
                            // Map'e özel diyalogları ekle
                            if (!targetDatabase.specialGeneralDialoguesByMap.ContainsKey(mapType))
                            {
                                targetDatabase.specialGeneralDialoguesByMap[mapType] = new List<DialogueNode>();
                            }
                            targetDatabase.specialGeneralDialoguesByMap[mapType].Add(dialogueNode);
                        }
                        
                        importedCount++;
                    }
                    
                    if (targetDatabase != null)
                    {
                        EditorUtility.SetDirty(targetDatabase);
                        AssetDatabase.SaveAssets();
                    }
                    
                    Debug.Log($"{importedCount} diyalog {mapType} haritası için başarıyla import edildi.");
                    EditorUtility.DisplayDialog("Başarılı", $"{importedCount} diyalog {mapType} haritası için başarıyla import edildi.", "Tamam");
                }
                else
                {
                    EditorUtility.DisplayDialog("Hata", $"Geçersiz map türü: {mapSpecificData.mapType}", "Tamam");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Hata", "Bu dosya map'e özel diyalog formatında değil!", "Tamam");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Map'e özel diyalog import hatası: {e.Message}");
            EditorUtility.DisplayDialog("Hata", $"Map'e özel diyalog import hatası: {e.Message}", "Tamam");
        }
    }
    
    private void ImportAllJsonFiles()
    {
        // Resources klasöründeki tüm JSON dosyalarını bul
        string[] jsonFiles = Directory.GetFiles(Application.dataPath + "/Resources/Dialogues", "*.json", SearchOption.AllDirectories);
        
        int importedCount = 0;
        
        foreach (string filePath in jsonFiles)
        {
            try
            {
                string jsonContent = File.ReadAllText(filePath);
                
                // JSON array formatını kontrol et
                if (jsonContent.Trim().StartsWith("["))
                {
                    // Array formatı - Newtonsoft.Json kullanarak parse et
                    var dialogueArray = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JsonDialogueData>>(jsonContent);
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
                }
                else
                {
                    // Tek diyalog formatı
                    var jsonData = JsonUtility.FromJson<JsonDialogueData>(jsonContent);
                    DialogueNode dialogueNode = ConvertJsonToDialogueNode(jsonData);
                    importedDialogues.Add(dialogueNode);
                    
                    if (targetDatabase != null)
                    {
                        targetDatabase.generalDialogues.Add(dialogueNode);
                    }
                    
                    importedCount++;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Dosya import hatası ({filePath}): {e.Message}");
            }
        }
        
        if (targetDatabase != null)
        {
            EditorUtility.SetDirty(targetDatabase);
            AssetDatabase.SaveAssets();
        }
        
        Debug.Log($"{importedCount} diyalog başarıyla import edildi.");
        EditorUtility.DisplayDialog("Başarılı", $"{importedCount} diyalog başarıyla import edildi.", "Tamam");
    }
    
    private DialogueNode ConvertJsonToDialogueNode(JsonDialogueData jsonData)
    {
        DialogueNode dialogueNode = new DialogueNode
        {
            id = jsonData.id,
            // name öncelikli; yoksa speaker kullanılır
            name = !string.IsNullOrEmpty(jsonData.name) ? jsonData.name : jsonData.speaker,
            // Text'e konuşmacı adı eklenmez; yalnızca diyalog metni yazılır
            text = jsonData.text,
            choices = new List<DialogueChoice>()
        };
        
        // Seçenekleri dönüştür
        if (jsonData.options != null)
        {
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
        }
        
        return dialogueNode;
    }
}

/// <summary>
/// Map'e özel diyalog verisi
/// </summary>
[System.Serializable]
public class JsonMapSpecificDialogueData
{
    public string mapType;
    public List<JsonDialogueData> dialogues;
}

 