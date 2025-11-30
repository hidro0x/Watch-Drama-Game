using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

/// <summary>
/// Exports dialogue JSON files to Excel/CSV format for easy translation
/// Supports multiple languages in a single Excel file
/// </summary>
public class DialogueExcelExporter : EditorWindow
{
    private string exportPath = "Assets/Exports";
    private bool includeTurkish = true;
    private bool includeEnglish = true;
    private bool exportAsCSV = true;
    
    [MenuItem("Kahin/Excel Dialogue Exporter")]
    public static void ShowWindow()
    {
        GetWindow<DialogueExcelExporter>("Dialogue Excel Exporter");
    }
    
    private int dialogueCount = 0;
    private Vector2 scrollPosition;
    
    private void OnGUI()
    {
        GUILayout.Label("Dialogue Excel/CSV Exporter", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This tool exports ALL current dialogue JSON files to Excel/CSV format.\n" +
                                "Each row represents a dialogue, with columns for different languages.\n" +
                                "Perfect for translators! All your current dialogues will be exported.", MessageType.Info);
        
        EditorGUILayout.Space();
        
        // Preview button
        if (GUILayout.Button("Preview: Count Dialogues", GUILayout.Height(25)))
        {
            PreviewDialogueCount();
        }
        
        if (dialogueCount > 0)
        {
            EditorGUILayout.HelpBox($"Found {dialogueCount} dialogues ready to export", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        
        // Export path
        EditorGUILayout.LabelField("Export Path:", exportPath);
        if (GUILayout.Button("Choose Export Folder"))
        {
            string path = EditorUtility.SaveFolderPanel("Choose Export Folder", exportPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                exportPath = path;
            }
        }
        
        EditorGUILayout.Space();
        
        // Language options
        EditorGUILayout.LabelField("Languages to Include:", EditorStyles.boldLabel);
        includeTurkish = EditorGUILayout.Toggle("Turkish (from root folder)", includeTurkish);
        includeEnglish = EditorGUILayout.Toggle("English (from en/ folder)", includeEnglish);
        
        EditorGUILayout.Space();
        
        // Format option
        exportAsCSV = EditorGUILayout.Toggle("Export as CSV (Excel compatible)", exportAsCSV);
        
        EditorGUILayout.Space();
        
        // Export button
        GUI.enabled = includeTurkish || includeEnglish;
        if (GUILayout.Button("Export ALL Dialogues to Excel/CSV", GUILayout.Height(35)))
        {
            ExportToExcel();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Note: CSV files can be opened in Excel, Google Sheets, or any spreadsheet software.\n" +
                                "The export includes:\n" +
                                "• general_npc_dialogues.json\n" +
                                "• varnan_dialogues.json\n" +
                                "• agnari_dialogues_full.json\n" +
                                "• theon_dialogues_full.json\n" +
                                "• solarya_dialogues_full.json\n" +
                                "• astharil_dialogues_full.json\n" +
                                "• global_dialogues_5_choices.json\n" +
                                "• And all other dialogue files!\n\n" +
                                "💡 Tip: Use 'Excel Dialogue Exporter (With Styling)' menu for styling instructions!", MessageType.Info);
    }
    
    private void PreviewDialogueCount()
    {
        Dictionary<string, DialogueExportData> dialogues = new Dictionary<string, DialogueExportData>();
        
        if (includeTurkish)
        {
            LoadDialoguesFromLanguage("", "Turkish", dialogues);
            LoadDialoguesFromLanguage("tr", "Turkish", dialogues);
        }
        
        if (includeEnglish)
        {
            LoadDialoguesFromLanguage("en", "English", dialogues);
        }
        
        dialogueCount = dialogues.Count;
        Debug.Log($"Preview: Found {dialogueCount} unique dialogues to export");
    }
    
    private void ExportToExcel()
    {
        try
        {
            // Create export directory
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }
            
            // Collect all dialogues
            Dictionary<string, DialogueExportData> dialogues = new Dictionary<string, DialogueExportData>();
            
            int turkishCount = 0;
            int englishCount = 0;
            
            // Load Turkish dialogues
            if (includeTurkish)
            {
                Debug.Log("=== Loading Turkish Dialogues ===");
                int beforeCount = dialogues.Count;
                LoadDialoguesFromLanguage("", "Turkish", dialogues);
                LoadDialoguesFromLanguage("tr", "Turkish", dialogues);
                turkishCount = dialogues.Count - beforeCount;
                Debug.Log($"Loaded {turkishCount} Turkish dialogues. Total: {dialogues.Count}");
            }
            
            // Load English dialogues
            if (includeEnglish)
            {
                Debug.Log("=== Loading English Dialogues ===");
                int beforeCount = dialogues.Count;
                LoadDialoguesFromLanguage("en", "English", dialogues);
                int newEnglishDialogues = dialogues.Count - beforeCount;
                englishCount = CountEnglishDialogues(dialogues);
                Debug.Log($"Found {newEnglishDialogues} new dialogue entries from English files.");
                Debug.Log($"Total dialogues with English translations: {englishCount} out of {dialogues.Count}");
            }
            
            // Export to CSV
            string csvPath = Path.Combine(exportPath, "dialogues_export.csv");
            ExportToCSV(dialogues, csvPath);
            
            // Create styling instructions file
            string instructionsPath = Path.Combine(exportPath, "STYLING_INSTRUCTIONS.txt");
            CreateQuickStylingGuide(instructionsPath);
            
            string summaryMessage = $"Successfully exported {dialogues.Count} dialogues to:\n{csvPath}\n\n";
            if (includeTurkish)
            {
                summaryMessage += $"Turkish: {turkishCount} dialogues loaded\n";
            }
            if (includeEnglish)
            {
                summaryMessage += $"English: {englishCount} dialogues with translations\n";
                if (englishCount < dialogues.Count)
                {
                    summaryMessage += $"⚠️ Note: {dialogues.Count - englishCount} dialogues missing English translations\n";
                }
            }
            summaryMessage += "\nStyling guide saved to:\n" + instructionsPath + "\n\n";
            summaryMessage += "You can now open this file in Excel or Google Sheets for translation.\n\n";
            summaryMessage += "💡 For advanced styling, use 'Excel Dialogue Exporter (With Styling)' menu!";
            
            EditorUtility.DisplayDialog("Export Complete", summaryMessage, "OK");
            
            // Open the folder
            EditorUtility.RevealInFinder(csvPath);
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Export Error", $"Error exporting dialogues:\n{e.Message}", "OK");
            Debug.LogError($"Export error: {e}");
        }
    }
    
    private void LoadDialoguesFromLanguage(string languageFolder, string languageName, Dictionary<string, DialogueExportData> dialogues)
    {
        string resourcesPath = string.IsNullOrEmpty(languageFolder) 
            ? "NewDialogues" 
            : $"NewDialogues/{languageFolder}";
        
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>(resourcesPath);
        
        if (jsonFiles == null || jsonFiles.Length == 0)
        {
            Debug.LogWarning($"⚠️ No files found in Resources/{resourcesPath} for {languageName}");
            Debug.LogWarning($"   Make sure files exist in: Assets/Resources/{resourcesPath}/");
            return;
        }
        
        Debug.Log($"✓ Found {jsonFiles.Length} files in Resources/{resourcesPath} for {languageName}");
        
        foreach (var jsonFile in jsonFiles)
        {
            if (jsonFile == null)
            {
                Debug.LogWarning("Skipping null file");
                continue;
            }
            
            // Skip files in subfolders when loading from root (for Turkish)
            if (string.IsNullOrEmpty(languageFolder) && jsonFile.name.Contains("/"))
            {
                continue;
            }
            
            // Include all dialogue files
            if (jsonFile.name.Contains("dialogues") || jsonFile.name.Contains("dialogue") || jsonFile.name.EndsWith(".json"))
            {
                // Skip meta files
                if (jsonFile.name.EndsWith(".meta"))
                {
                    continue;
                }
                
                Debug.Log($"  Processing file: {jsonFile.name}");
                
                try
                {
                    if (jsonFile == null || string.IsNullOrEmpty(jsonFile.text))
                    {
                        Debug.LogWarning($"  ⚠️ Skipping file {jsonFile?.name ?? "unknown"}: file is null or empty");
                        continue;
                    }
                    
                    string jsonContent = jsonFile.text;
                    
                    if (string.IsNullOrEmpty(jsonContent))
                    {
                        Debug.LogWarning($"  ⚠️ Skipping file {jsonFile.name}: content is empty");
                        continue;
                    }
                    
                    int dialoguesInFile = 0;
                    
                    // Handle array format
                    if (jsonContent.Trim().StartsWith("["))
                    {
                        var dialogueArray = JsonConvert.DeserializeObject<List<JsonDialogueData>>(jsonContent);
                        if (dialogueArray != null)
                        {
                            foreach (var dialogue in dialogueArray)
                            {
                                if (dialogue != null)
                                {
                                    AddDialogueToExport(dialogues, dialogue, languageName);
                                    dialoguesInFile++;
                                }
                            }
                        }
                        Debug.Log($"    ✓ Loaded {dialoguesInFile} dialogues from {jsonFile.name} ({languageName})");
                    }
                    // Handle map-specific format
                    else if (jsonContent.Contains("\"mapType\""))
                    {
                        // Use JsonMapSpecificDialogueData which is available in Editor context
                        var mapData = JsonConvert.DeserializeObject<JsonMapSpecificDialogueData>(jsonContent);
                        if (mapData != null && mapData.dialogues != null)
                        {
                            foreach (var dialogue in mapData.dialogues)
                            {
                                if (dialogue != null)
                                {
                                    AddDialogueToExport(dialogues, dialogue, languageName, mapData.mapType ?? "");
                                    dialoguesInFile++;
                                }
                            }
                        }
                        Debug.Log($"    ✓ Loaded {dialoguesInFile} dialogues from {jsonFile.name} ({languageName}, map: {mapData?.mapType ?? "unknown"})");
                    }
                    // Single dialogue format
                    else
                    {
                        var dialogue = JsonConvert.DeserializeObject<JsonDialogueData>(jsonContent);
                        if (dialogue != null)
                        {
                            AddDialogueToExport(dialogues, dialogue, languageName);
                            dialoguesInFile++;
                            Debug.Log($"    ✓ Loaded 1 dialogue from {jsonFile.name} ({languageName})");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error processing file {jsonFile?.name ?? "unknown"}: {e.Message}\nStackTrace: {e.StackTrace}");
                }
            }
        }
    }
    
    private void AddDialogueToExport(Dictionary<string, DialogueExportData> dialogues, JsonDialogueData dialogue, string languageName, string mapType = "")
    {
        if (dialogue == null)
        {
            Debug.LogWarning("⚠️ Attempted to add null dialogue to export");
            return;
        }
        
        if (string.IsNullOrEmpty(dialogue.id))
        {
            Debug.LogWarning("⚠️ Attempted to add dialogue with empty ID");
            return;
        }
        
        bool isNewDialogue = !dialogues.ContainsKey(dialogue.id);
        
        if (isNewDialogue)
        {
            dialogues[dialogue.id] = new DialogueExportData
            {
                id = dialogue.id,
                type = dialogue.type ?? "normal",
                mapType = mapType ?? ""
            };
        }
        
        var exportData = dialogues[dialogue.id];
        
        // Set language-specific data
        if (languageName == "Turkish")
        {
            exportData.nameTurkish = dialogue.name ?? dialogue.speaker ?? "";
            exportData.speakerTurkish = dialogue.speaker ?? dialogue.name ?? "";
            exportData.textTurkish = dialogue.text ?? "";
            exportData.optionsTurkish = dialogue.options ?? new List<JsonDialogueOption>();
        }
        else if (languageName == "English")
        {
            exportData.nameEnglish = dialogue.name ?? dialogue.speaker ?? "";
            exportData.speakerEnglish = dialogue.speaker ?? dialogue.name ?? "";
            exportData.textEnglish = dialogue.text ?? "";
            exportData.optionsEnglish = dialogue.options ?? new List<JsonDialogueOption>();
        }
    }
    
    /// <summary>
    /// Count how many dialogues have English translations
    /// </summary>
    private int CountEnglishDialogues(Dictionary<string, DialogueExportData> dialogues)
    {
        int count = 0;
        foreach (var kvp in dialogues)
        {
            var dialogue = kvp.Value;
            // Check if dialogue has any English content
            if (!string.IsNullOrEmpty(dialogue.textEnglish) || 
                !string.IsNullOrEmpty(dialogue.nameEnglish) ||
                (dialogue.optionsEnglish != null && dialogue.optionsEnglish.Count > 0))
            {
                count++;
            }
        }
        return count;
    }
    
    private void ExportToCSV(Dictionary<string, DialogueExportData> dialogues, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
        {
            // Write BOM for Excel UTF-8 compatibility
            writer.Write('\uFEFF');
            
            // Write header
            writer.WriteLine("ID,Type,MapType," +
                           "Name_TR,Speaker_TR,Text_TR,Option1_TR,Option2_TR,Option3_TR," +
                           "Name_EN,Speaker_EN,Text_EN,Option1_EN,Option2_EN,Option3_EN," +
                           "Faith1,Trust1,Hostility1,Faith2,Trust2,Hostility2,Faith3,Trust3,Hostility3");
            
            // Write data rows
            foreach (var kvp in dialogues)
            {
                var dialogue = kvp.Value;
                var row = new List<string>();
                
                // Basic info
                row.Add(EscapeCSV(dialogue.id));
                row.Add(EscapeCSV(dialogue.type));
                row.Add(EscapeCSV(dialogue.mapType));
                
                // Turkish columns
                row.Add(EscapeCSV(dialogue.nameTurkish));
                row.Add(EscapeCSV(dialogue.speakerTurkish));
                row.Add(EscapeCSV(dialogue.textTurkish));
                
                // Turkish options (up to 3)
                for (int i = 0; i < 3; i++)
                {
                    if (i < dialogue.optionsTurkish.Count)
                    {
                        row.Add(EscapeCSV(dialogue.optionsTurkish[i].text));
                    }
                    else
                    {
                        row.Add("");
                    }
                }
                
                // English columns
                row.Add(EscapeCSV(dialogue.nameEnglish));
                row.Add(EscapeCSV(dialogue.speakerEnglish));
                row.Add(EscapeCSV(dialogue.textEnglish));
                
                // English options (up to 3)
                for (int i = 0; i < 3; i++)
                {
                    if (i < dialogue.optionsEnglish.Count)
                    {
                        row.Add(EscapeCSV(dialogue.optionsEnglish[i].text));
                    }
                    else
                    {
                        row.Add("");
                    }
                }
                
                // Effects (use Turkish options as reference, effects are same for all languages)
                for (int i = 0; i < 3; i++)
                {
                    if (i < dialogue.optionsTurkish.Count && dialogue.optionsTurkish[i].effects != null)
                    {
                        row.Add(dialogue.optionsTurkish[i].effects.faith.ToString());
                        row.Add(dialogue.optionsTurkish[i].effects.trust.ToString());
                        row.Add(dialogue.optionsTurkish[i].effects.hostility.ToString());
                    }
                    else
                    {
                        row.Add("0");
                        row.Add("0");
                        row.Add("0");
                    }
                }
                
                writer.WriteLine(string.Join(",", row));
            }
        }
        
        Debug.Log($"Exported {dialogues.Count} dialogues to CSV: {filePath}");
    }
    
    private string EscapeCSV(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }
        
        // If contains comma, quote, or newline, wrap in quotes and escape quotes
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        
        return value;
    }
    
    private void CreateQuickStylingGuide(string filePath)
    {
        StringBuilder guide = new StringBuilder();
        guide.AppendLine("QUICK STYLING GUIDE FOR EXCEL");
        guide.AppendLine("==============================");
        guide.AppendLine();
        guide.AppendLine("After opening the CSV in Excel:");
        guide.AppendLine();
        guide.AppendLine("1. SELECT HEADER ROW (Row 1):");
        guide.AppendLine("   - Click row number '1'");
        guide.AppendLine("   - Format: Bold, Light Blue background (#D9E1F2)");
        guide.AppendLine();
        guide.AppendLine("2. FREEZE HEADER:");
        guide.AppendLine("   - Click A2, then View > Freeze Panes");
        guide.AppendLine();
        guide.AppendLine("3. AUTO-SIZE COLUMNS:");
        guide.AppendLine("   - Select all (Ctrl+A), double-click column borders");
        guide.AppendLine();
        guide.AppendLine("4. COLOR CODE:");
        guide.AppendLine("   - Turkish columns (D-I): Light Yellow (#FFF2CC)");
        guide.AppendLine("   - English columns (J-O): Light Green (#E2EFDA)");
        guide.AppendLine("   - Effects columns (P-X): Light Gray (#F2F2F2)");
        guide.AppendLine();
        guide.AppendLine("5. PROTECT STYLING:");
        guide.AppendLine("   - Format header row as 'Locked'");
        guide.AppendLine("   - Review > Protect Sheet");
        guide.AppendLine("   - Check 'Format cells' to protect styling");
        guide.AppendLine();
        guide.AppendLine("6. SAVE AS .XLSX to preserve styling!");
        guide.AppendLine();
        
        File.WriteAllText(filePath, guide.ToString(), Encoding.UTF8);
    }
}

/// <summary>
/// Data structure for exporting dialogues
/// </summary>
[System.Serializable]
public class DialogueExportData
{
    public string id;
    public string type;
    public string mapType;
    
    // Turkish
    public string nameTurkish = "";
    public string speakerTurkish = "";
    public string textTurkish = "";
    public List<JsonDialogueOption> optionsTurkish = new List<JsonDialogueOption>();
    
    // English
    public string nameEnglish = "";
    public string speakerEnglish = "";
    public string textEnglish = "";
    public List<JsonDialogueOption> optionsEnglish = new List<JsonDialogueOption>();
}

/// <summary>
/// Map-specific dialogue data structure for JSON parsing (Editor version)
/// </summary>
[System.Serializable]
public class JsonMapSpecificDialogueData
{
    public string mapType;
    public List<JsonDialogueData> dialogues;
}

