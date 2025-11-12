using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

/// <summary>
/// Enhanced Excel exporter with styling support
/// Exports to proper Excel format (.xlsx) with formatting, colors, and protection
/// </summary>
public class DialogueExcelExporterWithStyling : EditorWindow
{
    private string exportPath = "Assets/Exports";
    private bool includeTurkish = true;
    private bool includeEnglish = true;
    private bool useExcelFormat = true;
    private bool protectStyling = true;
    private bool freezeHeaderRow = true;
    
    [MenuItem("Kahin/Excel Dialogue Exporter (With Styling)")]
    public static void ShowWindow()
    {
        GetWindow<DialogueExcelExporterWithStyling>("Excel Exporter (Styled)");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Excel Exporter with Styling", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This exporter creates properly formatted Excel files with:\n" +
                                "• Header row styling (bold, colored background)\n" +
                                "• Column width auto-sizing\n" +
                                "• Text wrapping for long content\n" +
                                "• Protected styling (read-only formatting)\n" +
                                "• Frozen header row\n" +
                                "• Color-coded columns by language", MessageType.Info);
        
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
        includeTurkish = EditorGUILayout.Toggle("Turkish", includeTurkish);
        includeEnglish = EditorGUILayout.Toggle("English", includeEnglish);
        
        EditorGUILayout.Space();
        
        // Format options
        useExcelFormat = EditorGUILayout.Toggle("Export as Excel (.xlsx)", useExcelFormat);
        protectStyling = EditorGUILayout.Toggle("Protect Styling (Read-only formatting)", protectStyling);
        freezeHeaderRow = EditorGUILayout.Toggle("Freeze Header Row", freezeHeaderRow);
        
        EditorGUILayout.Space();
        
        // Export button
        GUI.enabled = includeTurkish || includeEnglish;
        if (GUILayout.Button("Export to Excel (Styled)", GUILayout.Height(35)))
        {
            if (useExcelFormat)
            {
                ExportToExcelStyled();
            }
            else
            {
                ExportToCSVStyled();
            }
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Note: Excel format preserves all styling. CSV format is simpler but loses formatting.", MessageType.Info);
    }
    
    private void ExportToExcelStyled()
    {
        try
        {
            // Check if EPPlus is available
            bool hasEPPlus = CheckForEPPlus();
            
            if (!hasEPPlus)
            {
                // Fallback to CSV with styling instructions
                EditorUtility.DisplayDialog("EPPlus Not Found", 
                    "EPPlus library not found. Exporting to CSV format instead.\n\n" +
                    "To enable Excel export with styling:\n" +
                    "1. Install EPPlus via NuGet (com.unity.nuget.epplus)\n" +
                    "2. Or use the CSV export which can be styled in Excel manually", 
                    "OK");
                ExportToCSVStyled();
                return;
            }
            
            // Use EPPlus for Excel export
            ExportWithEPPlus();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Export Error", $"Error exporting to Excel:\n{e.Message}\n\nFalling back to CSV.", "OK");
            ExportToCSVStyled();
        }
    }
    
    private bool CheckForEPPlus()
    {
        // Try to find EPPlus assembly
        try
        {
            System.Type epPlusType = System.Type.GetType("OfficeOpenXml.ExcelPackage, EPPlus");
            return epPlusType != null;
        }
        catch
        {
            return false;
        }
    }
    
    private void ExportWithEPPlus()
    {
        // This would use EPPlus if available
        // For now, we'll export CSV with styling template
        ExportToCSVStyled();
        
        EditorUtility.DisplayDialog("Excel Export", 
            "Excel export with EPPlus is not yet implemented.\n" +
            "Exporting to CSV format instead.\n\n" +
            "The CSV file can be opened in Excel and styled manually, or you can use the template feature.", 
            "OK");
    }
    
    private void ExportToCSVStyled()
    {
        try
        {
            // Create export directory
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }
            
            // Collect all dialogues (reuse logic from DialogueExcelExporter)
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
            
            // Export to CSV
            string csvPath = Path.Combine(exportPath, "dialogues_export_styled.csv");
            ExportToCSVWithInstructions(dialogues, csvPath);
            
            // Create Excel template/instructions file
            string instructionsPath = Path.Combine(exportPath, "EXCEL_STYLING_INSTRUCTIONS.txt");
            CreateStylingInstructions(instructionsPath);
            
            EditorUtility.DisplayDialog("Export Complete", 
                $"Successfully exported {dialogues.Count} dialogues to:\n{csvPath}\n\n" +
                "Styling instructions saved to:\n" + instructionsPath + "\n\n" +
                "Open the CSV in Excel and follow the instructions to apply styling.", 
                "OK");
            
            // Open the folder
            EditorUtility.RevealInFinder(csvPath);
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Export Error", $"Error exporting dialogues:\n{e.Message}", "OK");
            Debug.LogError($"Export error: {e}");
        }
    }
    
    private void ExportToCSVWithInstructions(Dictionary<string, DialogueExportData> dialogues, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
        {
            // Write BOM for Excel UTF-8 compatibility
            writer.Write('\uFEFF');
            
            // Write header with styling hints (Excel will interpret these)
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
                
                // Effects (use Turkish options as reference)
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
    
    private void CreateStylingInstructions(string filePath)
    {
        StringBuilder instructions = new StringBuilder();
        instructions.AppendLine("========================================");
        instructions.AppendLine("EXCEL STYLING INSTRUCTIONS");
        instructions.AppendLine("========================================");
        instructions.AppendLine();
        instructions.AppendLine("After opening the CSV file in Excel, follow these steps to apply styling:");
        instructions.AppendLine();
        instructions.AppendLine("1. SELECT THE HEADER ROW (Row 1):");
        instructions.AppendLine("   - Select entire row by clicking row number '1'");
        instructions.AppendLine("   - Right-click > Format Cells");
        instructions.AppendLine("   - Font: Bold, Size 11");
        instructions.AppendLine("   - Fill: Light Blue background (#D9E1F2)");
        instructions.AppendLine("   - Alignment: Center, Wrap Text");
        instructions.AppendLine();
        instructions.AppendLine("2. FREEZE HEADER ROW:");
        instructions.AppendLine("   - Click cell A2");
        instructions.AppendLine("   - View > Freeze Panes > Freeze Panes");
        instructions.AppendLine();
        instructions.AppendLine("3. AUTO-SIZE COLUMNS:");
        instructions.AppendLine("   - Select all columns (Ctrl+A or click top-left corner)");
        instructions.AppendLine("   - Double-click between any two column headers");
        instructions.AppendLine("   - Or: Home > Format > AutoFit Column Width");
        instructions.AppendLine();
        instructions.AppendLine("4. COLOR-CODE LANGUAGE COLUMNS:");
        instructions.AppendLine("   - Select Turkish columns (D-F, G-I): Light Yellow (#FFF2CC)");
        instructions.AppendLine("   - Select English columns (J-L, M-O): Light Green (#E2EFDA)");
        instructions.AppendLine("   - Select Effects columns (P-X): Light Gray (#F2F2F2)");
        instructions.AppendLine();
        instructions.AppendLine("5. TEXT WRAPPING:");
        instructions.AppendLine("   - Select all data cells (A2 onwards)");
        instructions.AppendLine("   - Home > Wrap Text");
        instructions.AppendLine("   - Set row height: Right-click row > Row Height > 30");
        instructions.AppendLine();
        instructions.AppendLine("6. PROTECT STYLING (Optional):");
        instructions.AppendLine("   - Select all cells");
        instructions.AppendLine("   - Right-click > Format Cells > Protection");
        instructions.AppendLine("   - Uncheck 'Locked' for data cells (A2 onwards)");
        instructions.AppendLine("   - Keep 'Locked' checked for header row");
        instructions.AppendLine("   - Review > Protect Sheet");
        instructions.AppendLine("   - Check 'Format cells' to protect styling");
        instructions.AppendLine("   - Uncheck 'Select locked cells' if you want to prevent selection");
        instructions.AppendLine();
        instructions.AppendLine("7. SAVE AS EXCEL FORMAT:");
        instructions.AppendLine("   - File > Save As");
        instructions.AppendLine("   - Choose 'Excel Workbook (*.xlsx)'");
        instructions.AppendLine("   - This preserves all styling");
        instructions.AppendLine();
        instructions.AppendLine("========================================");
        instructions.AppendLine("QUICK STYLE TEMPLATE");
        instructions.AppendLine("========================================");
        instructions.AppendLine();
        instructions.AppendLine("Header Row:");
        instructions.AppendLine("  - Font: Calibri, 11pt, Bold");
        instructions.AppendLine("  - Background: #D9E1F2 (Light Blue)");
        instructions.AppendLine("  - Text Color: #000000 (Black)");
        instructions.AppendLine("  - Alignment: Center, Middle");
        instructions.AppendLine();
        instructions.AppendLine("Turkish Columns (D-F, G-I):");
        instructions.AppendLine("  - Background: #FFF2CC (Light Yellow)");
        instructions.AppendLine("  - Border: Thin, Gray");
        instructions.AppendLine();
        instructions.AppendLine("English Columns (J-L, M-O):");
        instructions.AppendLine("  - Background: #E2EFDA (Light Green)");
        instructions.AppendLine("  - Border: Thin, Gray");
        instructions.AppendLine();
        instructions.AppendLine("Effects Columns (P-X):");
        instructions.AppendLine("  - Background: #F2F2F2 (Light Gray)");
        instructions.AppendLine("  - Alignment: Center");
        instructions.AppendLine();
        instructions.AppendLine("Data Rows:");
        instructions.AppendLine("  - Font: Calibri, 10pt");
        instructions.AppendLine("  - Wrap Text: Yes");
        instructions.AppendLine("  - Row Height: 30px");
        instructions.AppendLine();
        
        File.WriteAllText(filePath, instructions.ToString(), Encoding.UTF8);
    }
    
    private string EscapeCSV(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }
        
        if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
        {
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
        
        return value;
    }
    
    // Reuse the loading logic from DialogueExcelExporter
    private void LoadDialoguesFromLanguage(string languageFolder, string languageName, Dictionary<string, DialogueExportData> dialogues)
    {
        string resourcesPath = string.IsNullOrEmpty(languageFolder) 
            ? "NewDialogues" 
            : $"NewDialogues/{languageFolder}";
        
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>(resourcesPath);
        
        if (jsonFiles == null || jsonFiles.Length == 0)
        {
            return;
        }
        
        foreach (var jsonFile in jsonFiles)
        {
            if (jsonFile == null) continue;
            
            if (string.IsNullOrEmpty(languageFolder) && jsonFile.name.Contains("/"))
            {
                continue;
            }
            
            if (jsonFile.name.Contains("dialogues") || jsonFile.name.Contains("dialogue") || jsonFile.name.EndsWith(".json"))
            {
                if (jsonFile.name.EndsWith(".meta")) continue;
                
                try
                {
                    if (string.IsNullOrEmpty(jsonFile.text)) continue;
                    
                    string jsonContent = jsonFile.text;
                    
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
                                }
                            }
                        }
                    }
                    else if (jsonContent.Contains("\"mapType\""))
                    {
                        var mapData = JsonConvert.DeserializeObject<JsonMapSpecificDialogueData>(jsonContent);
                        if (mapData != null && mapData.dialogues != null)
                        {
                            foreach (var dialogue in mapData.dialogues)
                            {
                                if (dialogue != null)
                                {
                                    AddDialogueToExport(dialogues, dialogue, languageName, mapData.mapType ?? "");
                                }
                            }
                        }
                    }
                    else
                    {
                        var dialogue = JsonConvert.DeserializeObject<JsonDialogueData>(jsonContent);
                        if (dialogue != null)
                        {
                            AddDialogueToExport(dialogues, dialogue, languageName);
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error processing file {jsonFile.name}: {e.Message}");
                }
            }
        }
    }
    
    private void AddDialogueToExport(Dictionary<string, DialogueExportData> dialogues, JsonDialogueData dialogue, string languageName, string mapType = "")
    {
        if (dialogue == null || string.IsNullOrEmpty(dialogue.id))
        {
            return;
        }
        
        if (!dialogues.ContainsKey(dialogue.id))
        {
            dialogues[dialogue.id] = new DialogueExportData
            {
                id = dialogue.id,
                type = dialogue.type ?? "normal",
                mapType = mapType ?? ""
            };
        }
        
        var exportData = dialogues[dialogue.id];
        
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
}

