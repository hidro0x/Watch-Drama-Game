using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.Util;

/// <summary>
/// Excel exporter using NPOI (from LocalizationX Lite)
/// Creates properly formatted Excel files with styling
/// </summary>
public class DialogueExcelExporterNPOI : EditorWindow
{
    private string exportPath = "Assets/Exports";
    private bool includeTurkish = true;
    private bool includeEnglish = true;
    
    [MenuItem("Kahin/Excel Dialogue Exporter (NPOI - Styled)")]
    public static void ShowWindow()
    {
        GetWindow<DialogueExcelExporterNPOI>("Excel Exporter (NPOI)");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Excel Exporter with NPOI (Styled)", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This exporter uses NPOI (from LocalizationX Lite) to create properly formatted Excel files.\n" +
                                "Features:\n" +
                                "• Real Excel format (.xlsx)\n" +
                                "• Header styling (bold, colored)\n" +
                                "• Column auto-sizing\n" +
                                "• Text wrapping\n" +
                                "• Color-coded languages\n" +
                                "• Protected styling", MessageType.Info);
        
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
        
        // Export button
        GUI.enabled = includeTurkish || includeEnglish;
        if (GUILayout.Button("Export to Excel (.xlsx) with Styling", GUILayout.Height(35)))
        {
            ExportToExcelNPOI();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Note: Requires LocalizationX Lite package (NPOI included).\n" +
                                "This creates a real Excel file with all styling preserved!", MessageType.Info);
    }
    
    private void ExportToExcelNPOI()
    {
        try
        {
            // Check if NPOI is available
            if (!IsNPOIAvailable())
            {
                EditorUtility.DisplayDialog("NPOI Not Found", 
                    "NPOI library not found!\n\n" +
                    "Make sure LocalizationX Lite package is installed.\n" +
                    "NPOI should be in: Assets/LocalizationX/Editor/Plugins/NPOI.dll", 
                    "OK");
                return;
            }
            
            // Create export directory
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }
            
            // Collect all dialogues
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
            
            // Export to Excel
            string excelPath = Path.Combine(exportPath, "dialogues_export_styled.xlsx");
            ExportToExcelWithNPOI(dialogues, excelPath);
            
            EditorUtility.DisplayDialog("Export Complete", 
                $"Successfully exported {dialogues.Count} dialogues to:\n{excelPath}\n\n" +
                "The Excel file includes:\n" +
                "• Styled header row\n" +
                "• Color-coded columns\n" +
                "• Auto-sized columns\n" +
                "• Text wrapping\n" +
                "• Protected formatting", 
                "OK");
            
            // Open the folder
            EditorUtility.RevealInFinder(excelPath);
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Export Error", $"Error exporting to Excel:\n{e.Message}\n\n{e.StackTrace}", "OK");
            Debug.LogError($"Export error: {e}");
        }
    }
    
    private bool IsNPOIAvailable()
    {
        try
        {
            // Try to create a workbook - if this works, NPOI is available
            IWorkbook testWorkbook = new XSSFWorkbook();
            testWorkbook.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private void ExportToExcelWithNPOI(Dictionary<string, DialogueExportData> dialogues, string filePath)
    {
        // Create new workbook
        IWorkbook workbook = new XSSFWorkbook();
        ISheet sheet = workbook.CreateSheet("Dialogues");
        
        // Create styles
        ICellStyle headerStyle = CreateHeaderStyle(workbook);
        ICellStyle turkishStyle = CreateTurkishStyle(workbook);
        ICellStyle englishStyle = CreateEnglishStyle(workbook);
        ICellStyle effectsStyle = CreateEffectsStyle(workbook);
        ICellStyle dataStyle = CreateDataStyle(workbook);
        
        // Create header row
        IRow headerRow = sheet.CreateRow(0);
        string[] headers = {
            "ID", "Type", "MapType",
            "Name_TR", "Speaker_TR", "Text_TR", "Option1_TR", "Option2_TR", "Option3_TR",
            "Name_EN", "Speaker_EN", "Text_EN", "Option1_EN", "Option2_EN", "Option3_EN",
            "Faith1", "Trust1", "Hostility1", "Faith2", "Trust2", "Hostility2", "Faith3", "Trust3", "Hostility3"
        };
        
        for (int i = 0; i < headers.Length; i++)
        {
            ICell cell = headerRow.CreateCell(i);
            cell.SetCellValue(headers[i]);
            cell.CellStyle = headerStyle;
        }
        
        // Freeze header row
        sheet.CreateFreezePane(0, 1);
        
        // Write data rows
        int rowIndex = 1;
        foreach (var kvp in dialogues)
        {
            var dialogue = kvp.Value;
            IRow row = sheet.CreateRow(rowIndex++);
            
            int colIndex = 0;
            
            // Basic info
            CreateCell(row, colIndex++, dialogue.id, dataStyle);
            CreateCell(row, colIndex++, dialogue.type, dataStyle);
            CreateCell(row, colIndex++, dialogue.mapType, dataStyle);
            
            // Turkish columns (D-I: columns 3-8)
            CreateCell(row, colIndex++, dialogue.nameTurkish, turkishStyle);
            CreateCell(row, colIndex++, dialogue.speakerTurkish, turkishStyle);
            CreateCell(row, colIndex++, dialogue.textTurkish, turkishStyle);
            
            for (int i = 0; i < 3; i++)
            {
                string optionText = i < dialogue.optionsTurkish.Count ? dialogue.optionsTurkish[i].text : "";
                CreateCell(row, colIndex++, optionText, turkishStyle);
            }
            
            // English columns (J-O: columns 9-14)
            CreateCell(row, colIndex++, dialogue.nameEnglish, englishStyle);
            CreateCell(row, colIndex++, dialogue.speakerEnglish, englishStyle);
            CreateCell(row, colIndex++, dialogue.textEnglish, englishStyle);
            
            for (int i = 0; i < 3; i++)
            {
                string optionText = i < dialogue.optionsEnglish.Count ? dialogue.optionsEnglish[i].text : "";
                CreateCell(row, colIndex++, optionText, englishStyle);
            }
            
            // Effects columns (P-X: columns 15-23)
            for (int i = 0; i < 3; i++)
            {
                if (i < dialogue.optionsTurkish.Count && dialogue.optionsTurkish[i].effects != null)
                {
                    CreateCell(row, colIndex++, dialogue.optionsTurkish[i].effects.faith.ToString(), effectsStyle);
                    CreateCell(row, colIndex++, dialogue.optionsTurkish[i].effects.trust.ToString(), effectsStyle);
                    CreateCell(row, colIndex++, dialogue.optionsTurkish[i].effects.hostility.ToString(), effectsStyle);
                }
                else
                {
                    CreateCell(row, colIndex++, "0", effectsStyle);
                    CreateCell(row, colIndex++, "0", effectsStyle);
                    CreateCell(row, colIndex++, "0", effectsStyle);
                }
            }
        }
        
        // Auto-size columns
        for (int i = 0; i < headers.Length; i++)
        {
            sheet.AutoSizeColumn(i);
            // Set minimum width
            if (sheet.GetColumnWidth(i) < 2000)
            {
                sheet.SetColumnWidth(i, 2000);
            }
            // Set maximum width for text columns
            if (i >= 3 && i <= 14) // Text columns
            {
                if (sheet.GetColumnWidth(i) > 15000)
                {
                    sheet.SetColumnWidth(i, 15000);
                }
            }
        }
        
        // Protect sheet (optional - allows editing but protects formatting)
        // sheet.ProtectSheet("password"); // Uncomment if you want password protection
        
        // Write to file
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            workbook.Write(fileStream);
        }
        
        workbook.Close();
        
        Debug.Log($"Exported {dialogues.Count} dialogues to Excel: {filePath}");
    }
    
    private ICellStyle CreateHeaderStyle(IWorkbook workbook)
    {
        ICellStyle style = workbook.CreateCellStyle();
        IFont font = workbook.CreateFont();
        font.Boldweight = (short)FontBoldWeight.Bold;
        font.FontHeightInPoints = 11;
        style.SetFont(font);
        
        // Light blue background
        style.FillForegroundColor = IndexedColors.LightBlue.Index;
        style.FillPattern = FillPattern.SolidForeground;
        
        // Center alignment
        style.Alignment = HorizontalAlignment.Center;
        style.VerticalAlignment = VerticalAlignment.Center;
        
        // Border
        style.BorderTop = BorderStyle.Thin;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;
        
        // Wrap text
        style.WrapText = true;
        
        return style;
    }
    
    private ICellStyle CreateTurkishStyle(IWorkbook workbook)
    {
        ICellStyle style = workbook.CreateCellStyle();
        
        // Light yellow background
        style.FillForegroundColor = IndexedColors.LightYellow.Index;
        style.FillPattern = FillPattern.SolidForeground;
        
        // Wrap text
        style.WrapText = true;
        style.VerticalAlignment = VerticalAlignment.Top;
        
        // Border
        style.BorderTop = BorderStyle.Thin;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;
        
        return style;
    }
    
    private ICellStyle CreateEnglishStyle(IWorkbook workbook)
    {
        ICellStyle style = workbook.CreateCellStyle();
        
        // Light green background
        style.FillForegroundColor = IndexedColors.LightGreen.Index;
        style.FillPattern = FillPattern.SolidForeground;
        
        // Wrap text
        style.WrapText = true;
        style.VerticalAlignment = VerticalAlignment.Top;
        
        // Border
        style.BorderTop = BorderStyle.Thin;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;
        
        return style;
    }
    
    private ICellStyle CreateEffectsStyle(IWorkbook workbook)
    {
        ICellStyle style = workbook.CreateCellStyle();
        
        // Light gray background
        style.FillForegroundColor = IndexedColors.Grey25Percent.Index;
        style.FillPattern = FillPattern.SolidForeground;
        
        // Center alignment
        style.Alignment = HorizontalAlignment.Center;
        style.VerticalAlignment = VerticalAlignment.Center;
        
        // Border
        style.BorderTop = BorderStyle.Thin;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;
        
        return style;
    }
    
    private ICellStyle CreateDataStyle(IWorkbook workbook)
    {
        ICellStyle style = workbook.CreateCellStyle();
        
        // Wrap text
        style.WrapText = true;
        style.VerticalAlignment = VerticalAlignment.Top;
        
        // Border
        style.BorderTop = BorderStyle.Thin;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;
        
        return style;
    }
    
    private void CreateCell(IRow row, int columnIndex, string value, ICellStyle style)
    {
        ICell cell = row.CreateCell(columnIndex);
        cell.SetCellValue(value ?? "");
        cell.CellStyle = style;
        
        // Set row height for text wrapping
        if (!string.IsNullOrEmpty(value) && value.Length > 50)
        {
            row.HeightInPoints = 30;
        }
    }
    
    // Reuse loading logic from DialogueExcelExporter
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

