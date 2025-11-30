using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.Util;

/// <summary>
/// Excel Exporter for UI Localization JSON files
/// Exports UI localization data to Excel for easy translation
/// </summary>
public class UILocalizationExcelExporter : EditorWindow
{
    private string localizationFolderPath = "Assets/Resources/Localization";
    private string exportPath = "Assets/Exports";
    
    [MenuItem("Kahin/UI Localization Excel Exporter")]
    public static void ShowWindow()
    {
        GetWindow<UILocalizationExcelExporter>("UI Localization Excel Exporter");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("UI Localization Excel Exporter", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("Exports UI localization JSON files to Excel format (.xlsx).\n" +
                                "Perfect for translators! Each row is a localization key with columns for each language.", 
                                MessageType.Info);
        
        EditorGUILayout.Space();
        
        // Localization folder path
        EditorGUILayout.LabelField("Localization Folder:", localizationFolderPath);
        if (GUILayout.Button("Choose Localization Folder"))
        {
            string path = EditorUtility.OpenFolderPanel("Choose Localization Folder", localizationFolderPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                localizationFolderPath = path;
            }
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
        
        // Export button
        if (GUILayout.Button("Export UI Localization to Excel", GUILayout.Height(35)))
        {
            ExportToExcel();
        }
        
        EditorGUILayout.Space();
        
        if (!IsNPOIAvailable())
        {
            EditorGUILayout.HelpBox("NPOI library not found! Please install NPOI package.", MessageType.Warning);
        }
    }
    
    private void ExportToExcel()
    {
        try
        {
            if (!Directory.Exists(localizationFolderPath))
            {
                EditorUtility.DisplayDialog("Error", "Localization folder not found!", "OK");
                return;
            }
            
            if (!IsNPOIAvailable())
            {
                EditorUtility.DisplayDialog("NPOI Not Found", 
                    "NPOI library not found!\n\n" +
                    "Please install NPOI package to use this exporter.", 
                    "OK");
                return;
            }
            
            // Find all JSON files in localization folder
            string[] jsonFiles = Directory.GetFiles(localizationFolderPath, "*_localization.json");
            
            if (jsonFiles.Length == 0)
            {
                EditorUtility.DisplayDialog("No Files Found", 
                    "No localization JSON files found!\n\n" +
                    "Looking for files matching: '*_localization.json'\n" +
                    "Folder: " + localizationFolderPath, 
                    "OK");
                return;
            }
            
            // Create export directory
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }
            
            // Export to Excel
            string excelPath = Path.Combine(exportPath, "ui_localization_export.xlsx");
            ExportLocalizationToExcel(jsonFiles, excelPath);
            
            EditorUtility.DisplayDialog("Export Complete", 
                $"Successfully exported {jsonFiles.Length} localization files to:\n{excelPath}", 
                "OK");
            
            // Open the folder
            EditorUtility.RevealInFinder(excelPath);
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Export Error", 
                $"Error exporting to Excel:\n{e.Message}", 
                "OK");
            Debug.LogError($"Export error: {e}");
        }
    }
    
    private bool IsNPOIAvailable()
    {
        try
        {
            IWorkbook testWorkbook = new XSSFWorkbook();
            testWorkbook.Close();
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    private void ExportLocalizationToExcel(string[] jsonFiles, string filePath)
    {
        // Collect all keys and languages
        Dictionary<string, Dictionary<string, string>> allLocalizations = new Dictionary<string, Dictionary<string, string>>();
        List<string> languages = new List<string>();
        
        foreach (string jsonFile in jsonFiles)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(jsonFile);
                // Extract language name from filename (e.g., "turkish_localization.json" -> "turkish")
                string language = fileName.Replace("_localization", "").ToLower();
                languages.Add(language);
                
                string jsonContent = File.ReadAllText(jsonFile);
                var localizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonContent);
                
                if (localizationData != null)
                {
                    foreach (var kvp in localizationData)
                    {
                        if (!allLocalizations.ContainsKey(kvp.Key))
                        {
                            allLocalizations[kvp.Key] = new Dictionary<string, string>();
                        }
                        
                        allLocalizations[kvp.Key][language] = kvp.Value;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading {jsonFile}: {e.Message}");
            }
        }
        
        // Create Excel workbook
        IWorkbook workbook = new XSSFWorkbook();
        ISheet sheet = workbook.CreateSheet("UI Localization");
        
        // Create styles
        ICellStyle headerStyle = CreateHeaderStyle(workbook);
        ICellStyle keyStyle = CreateKeyStyle(workbook);
        ICellStyle textStyle = CreateTextStyle(workbook);
        
        // Create header row
        IRow headerRow = sheet.CreateRow(0);
        ICell keyHeader = headerRow.CreateCell(0);
        keyHeader.SetCellValue("Key");
        keyHeader.CellStyle = headerStyle;
        
        for (int i = 0; i < languages.Count; i++)
        {
            ICell langHeader = headerRow.CreateCell(i + 1);
            langHeader.SetCellValue(CapitalizeFirst(languages[i]));
            langHeader.CellStyle = headerStyle;
        }
        
        // Write data rows
        int rowIndex = 1;
        foreach (var kvp in allLocalizations)
        {
            IRow row = sheet.CreateRow(rowIndex++);
            
            // Key column
            ICell keyCell = row.CreateCell(0);
            keyCell.SetCellValue(kvp.Key);
            keyCell.CellStyle = keyStyle;
            
            // Language columns
            for (int i = 0; i < languages.Count; i++)
            {
                string lang = languages[i];
                string text = kvp.Value.ContainsKey(lang) ? kvp.Value[lang] : "";
                
                ICell textCell = row.CreateCell(i + 1);
                textCell.SetCellValue(text);
                textCell.CellStyle = textStyle;
            }
        }
        
        // Auto-size columns
        sheet.AutoSizeColumn(0); // Key column
        if (sheet.GetColumnWidth(0) < 3000)
        {
            sheet.SetColumnWidth(0, 3000);
        }
        
        for (int i = 0; i < languages.Count; i++)
        {
            sheet.AutoSizeColumn(i + 1);
            if (sheet.GetColumnWidth(i + 1) < 5000)
            {
                sheet.SetColumnWidth(i + 1, 5000);
            }
            if (sheet.GetColumnWidth(i + 1) > 20000)
            {
                sheet.SetColumnWidth(i + 1, 20000);
            }
        }
        
        // Freeze header row
        sheet.CreateFreezePane(0, 1);
        
        // Write to file
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            workbook.Write(fileStream);
        }
        
        workbook.Close();
        
        Debug.Log($"Exported {allLocalizations.Count} localization keys to Excel: {filePath}");
    }
    
    private ICellStyle CreateHeaderStyle(IWorkbook workbook)
    {
        ICellStyle style = workbook.CreateCellStyle();
        IFont font = workbook.CreateFont();
        font.Boldweight = (short)FontBoldWeight.Bold;
        font.FontHeightInPoints = 12;
        style.SetFont(font);
        
        style.FillForegroundColor = IndexedColors.LightBlue.Index;
        style.FillPattern = FillPattern.SolidForeground;
        style.Alignment = HorizontalAlignment.Center;
        style.VerticalAlignment = VerticalAlignment.Center;
        style.WrapText = true;
        
        style.BorderTop = BorderStyle.Thin;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;
        
        return style;
    }
    
    private ICellStyle CreateKeyStyle(IWorkbook workbook)
    {
        ICellStyle style = workbook.CreateCellStyle();
        
        style.FillForegroundColor = IndexedColors.Grey25Percent.Index;
        style.FillPattern = FillPattern.SolidForeground;
        style.Alignment = HorizontalAlignment.Left;
        style.VerticalAlignment = VerticalAlignment.Center;
        
        style.BorderTop = BorderStyle.Thin;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;
        
        return style;
    }
    
    private ICellStyle CreateTextStyle(IWorkbook workbook)
    {
        ICellStyle style = workbook.CreateCellStyle();
        
        style.WrapText = true;
        style.VerticalAlignment = VerticalAlignment.Top;
        
        style.BorderTop = BorderStyle.Thin;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;
        
        return style;
    }
    
    private string CapitalizeFirst(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        return char.ToUpper(text[0]) + text.Substring(1);
    }
}

