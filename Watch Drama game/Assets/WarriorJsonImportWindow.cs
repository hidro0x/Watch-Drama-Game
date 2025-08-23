using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Unity Editor'da Savaşçı JSON import etmek için window
/// </summary>
public class WarriorJsonImportWindow : EditorWindow
{
    private TextAsset selectedJsonFile;
    private DialogueDatabase targetDatabase;
    private Vector2 scrollPosition;
    private List<CountryWarrior> importedWarriors = new List<CountryWarrior>();
    private bool showImportedWarriors = false;

    
    [MenuItem("Kahin/Savaşçı JSON Import")]
    public static void ShowWindow()
    {
        GetWindow<WarriorJsonImportWindow>("Savaşçı JSON Import");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Savaşçı JSON Import Sistemi", EditorStyles.boldLabel);
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
        if (GUILayout.Button("Örnek JSON Oluştur"))
        {
            CreateExampleJson();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // Import edilen savaşçılar
        if (importedWarriors.Count > 0)
        {
            showImportedWarriors = EditorGUILayout.Foldout(showImportedWarriors, $"Import Edilen Savaşçılar ({importedWarriors.Count})");
            
            if (showImportedWarriors)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                
                for (int i = 0; i < importedWarriors.Count; i++)
                {
                    var warrior = importedWarriors[i];
                    
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField($"Ülke: {warrior.country}", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"Savaşçı Adı: {warrior.warriorName}");
                    EditorGUILayout.LabelField($"İttifak Metni: {warrior.allianceProposalText}");
                    EditorGUILayout.LabelField($"Seçenek 1: {warrior.joinWarriorSideText}");
                    EditorGUILayout.LabelField($"Seçenek 2: {warrior.helpCurrentCountryText}");
                    
                    EditorGUILayout.LabelField("Savaşçı Tarafına Katılma Etkileri:", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"  Trust: {warrior.joinWarriorTrust}, Faith: {warrior.joinWarriorFaith}, Hostility: {warrior.joinWarriorHostility}");
                    
                    EditorGUILayout.LabelField("Mevcut Ülkeye Yardım Etkileri:", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"  Trust: {warrior.helpCurrentTrust}, Faith: {warrior.helpCurrentFaith}, Hostility: {warrior.helpCurrentHostility}");
                    
                    EditorGUILayout.EndVertical();
                    GUILayout.Space(5);
                }
                
                EditorGUILayout.EndScrollView();
                
                if (GUILayout.Button("Import Edilen Savaşçıları Temizle"))
                {
                    importedWarriors.Clear();
                }
            }
        }
        
        // JSON formatı örneği
        GUILayout.Space(10);
        if (EditorGUILayout.Foldout(false, "JSON Format Örneği"))
        {
            EditorGUILayout.HelpBox(
                @"{
  ""warriors"": [
    {
      ""country"": ""Astrahil"",
      ""warriorName"": ""Astrahil Savaşçısı"",
      
      ""allianceProposalText"": ""Ben Astrahil'in en güçlü savaşçısıyım! Seninle ittifak kurmak istiyorum!"",
      ""joinWarriorSideText"": ""Astrahil'in tarafına katıl"",
      ""helpCurrentCountryText"": ""Mevcut ülkeye yardım et"",
      ""joinWarriorEffects"": {
        ""trust"": 5, ""faith"": 3, ""hostility"": 8,
        ""opponentTrust"": 3, ""opponentFaith"": 2, ""opponentHostility"": 5
      },
      ""helpCurrentEffects"": {
        ""trust"": -2, ""faith"": -1, ""hostility"": 2,
        ""opponentTrust"": -2, ""opponentFaith"": -1, ""opponentHostility"": 1
      }
    }
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
        
        if (targetDatabase == null)
        {
            EditorUtility.DisplayDialog("Hata", "Lütfen hedef Database'i seçin!", "Tamam");
            return;
        }
        
        try
        {
            string jsonContent = selectedJsonFile.text;
            WarriorImportData importData = JsonUtility.FromJson<WarriorImportData>(jsonContent);
            
            if (importData == null || importData.warriors == null)
            {
                EditorUtility.DisplayDialog("Hata", "JSON formatı geçersiz!", "Tamam");
                return;
            }
            
            int importedCount = 0;
            foreach (var warriorData in importData.warriors)
            {
                if (ConvertJsonToWarrior(warriorData, out CountryWarrior warrior))
                {
                    // Mevcut savaşçıyı güncelle veya yeni ekle
                    UpdateOrAddWarrior(warrior);
                    importedWarriors.Add(warrior);
                    importedCount++;
                }
            }
            
            if (targetDatabase != null)
            {
                EditorUtility.SetDirty(targetDatabase);
                AssetDatabase.SaveAssets();
            }
            
            Debug.Log($"{importedCount} savaşçı başarıyla import edildi!");
            EditorUtility.DisplayDialog("Başarılı", $"{importedCount} savaşçı başarıyla import edildi!", "Tamam");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Import hatası: {e.Message}");
            EditorUtility.DisplayDialog("Hata", $"Import hatası: {e.Message}", "Tamam");
        }
    }
    
    private bool ConvertJsonToWarrior(WarriorJsonData jsonData, out CountryWarrior warrior)
    {
        warrior = null;
        
        // MapType'ı parse et
        if (!System.Enum.TryParse<MapType>(jsonData.country, out MapType country))
        {
            Debug.LogWarning($"Geçersiz ülke: {jsonData.country}");
            return false;
        }
        

        
        warrior = new CountryWarrior
        {
            country = country,
            warriorName = jsonData.warriorName,
            warriorSprite = null, // Sprite kullanmıyoruz
            allianceProposalText = jsonData.allianceProposalText,
            joinWarriorSideText = jsonData.joinWarriorSideText,
            helpCurrentCountryText = jsonData.helpCurrentCountryText,
            joinWarriorTrust = jsonData.joinWarriorEffects?.trust ?? 0,
            joinWarriorFaith = jsonData.joinWarriorEffects?.faith ?? 0,
            joinWarriorHostility = jsonData.joinWarriorEffects?.hostility ?? 0,
            helpCurrentTrust = jsonData.helpCurrentEffects?.trust ?? 0,
            helpCurrentFaith = jsonData.helpCurrentEffects?.faith ?? 0,
            helpCurrentHostility = jsonData.helpCurrentEffects?.hostility ?? 0
        };
        
        return true;
    }
    

    
    private void UpdateOrAddWarrior(CountryWarrior newWarrior)
    {
        if (targetDatabase.countryWarriors == null)
        {
            targetDatabase.countryWarriors = new List<CountryWarrior>();
        }
        
        // Mevcut savaşçıyı bul
        var existingWarrior = targetDatabase.countryWarriors.FirstOrDefault(w => w.country == newWarrior.country);
        
        if (existingWarrior != null)
        {
            // Mevcut savaşçıyı güncelle
            int index = targetDatabase.countryWarriors.IndexOf(existingWarrior);
            targetDatabase.countryWarriors[index] = newWarrior;
            Debug.Log($"Savaşçı güncellendi: {newWarrior.warriorName} ({newWarrior.country})");
        }
        else
        {
            // Yeni savaşçı ekle
            targetDatabase.countryWarriors.Add(newWarrior);
            Debug.Log($"Yeni savaşçı eklendi: {newWarrior.warriorName} ({newWarrior.country})");
        }
    }
    
    private void CreateExampleJson()
    {
        var exampleData = new WarriorImportData
        {
            warriors = new List<WarriorJsonData>
            {
                new WarriorJsonData
                {
                    country = "Astrahil",
                    warriorName = "Astrahil Savaşçısı",
                    allianceProposalText = "Ben Astrahil'in en güçlü savaşçısıyım! Seninle ittifak kurmak istiyorum!",
                    joinWarriorSideText = "Astrahil'in tarafına katıl",
                    helpCurrentCountryText = "Mevcut ülkeye yardım et",
                    joinWarriorEffects = new WarriorEffects
                    {
                        trust = 5,
                        faith = 3,
                        hostility = 8,
                        opponentTrust = 3,
                        opponentFaith = 2,
                        opponentHostility = 5
                    },
                    helpCurrentEffects = new WarriorEffects
                    {
                        trust = -2,
                        faith = -1,
                        hostility = 2,
                        opponentTrust = -2,
                        opponentFaith = -1,
                        opponentHostility = 1
                    }
                }
            }
        };
        
        string json = JsonUtility.ToJson(exampleData, true);
        string path = Path.Combine(Application.dataPath, "Warriors_Example.json");
        File.WriteAllText(path, json);
        Debug.Log($"Örnek JSON oluşturuldu: {path}");
        EditorUtility.DisplayDialog("Başarılı", $"Örnek JSON oluşturuldu: {path}", "Tamam");
    }
}
