using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class WarriorJsonImporter : MonoBehaviour
{
    [Header("Import Ayarları")]
    [SerializeField] private DialogueDatabase targetDatabase;
    [SerializeField] private string jsonFilePath = "Warriors.json";
    

    
    [ContextMenu("Savaşçıları Import Et")]
    public void ImportWarriors()
    {
        if (targetDatabase == null)
        {
            Debug.LogError("Target Database atanmamış!");
            return;
        }
        
        string fullPath = Path.Combine(Application.dataPath, jsonFilePath);
        if (!File.Exists(fullPath))
        {
            Debug.LogError($"JSON dosyası bulunamadı: {fullPath}");
            return;
        }
        
        try
        {
            string jsonContent = File.ReadAllText(fullPath);
            WarriorImportData importData = JsonUtility.FromJson<WarriorImportData>(jsonContent);
            
            if (importData == null || importData.warriors == null)
            {
                Debug.LogError("JSON formatı geçersiz!");
                return;
            }
            
            int importedCount = 0;
            foreach (var warriorData in importData.warriors)
            {
                if (ConvertJsonToWarrior(warriorData, out CountryWarrior warrior))
                {
                    // Mevcut savaşçıyı güncelle veya yeni ekle
                    UpdateOrAddWarrior(warrior);
                    importedCount++;
                }
            }
            
            Debug.Log($"{importedCount} savaşçı başarıyla import edildi!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Import hatası: {e.Message}");
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
    
    [ContextMenu("Örnek JSON Oluştur")]
    public void CreateExampleJson()
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
    }
}
