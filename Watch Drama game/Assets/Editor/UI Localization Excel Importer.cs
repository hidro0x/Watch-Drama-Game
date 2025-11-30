using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

/// <summary>
/// Excel Importer for UI Localization
/// Imports UI localization data from Excel back to JSON files
/// </summary>
public class UILocalizationExcelImporter : EditorWindow
{
    private string excelFilePath = "";
    private string localizationFolderPath = "Assets/Resources/Localization";
    private bool overwriteExisting = true; // Default to true
    private bool showDebug = false;
    
    [MenuItem("Kahin/UI Localization Excel Importer")]
    public static void ShowWindow()
    {
        GetWindow<UILocalizationExcelImporter>("UI Localization Excel Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("UI Localization Excel Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("Imports UI localization from Excel format (.xlsx) back to JSON files.\n" +
                                "The Excel file should have:\n" +
                                "• First column: Localization Keys\n" +
                                "• Other columns: Languages (e.g., Turkish, English)", 
                                MessageType.Info);
        
        EditorGUILayout.Space();
        
        // Excel file selection
        EditorGUILayout.LabelField("Excel File:", excelFilePath);
        if (GUILayout.Button("Choose Excel File"))
        {
            string path = EditorUtility.OpenFilePanel("Choose Excel File", "", "xlsx");
            if (!string.IsNullOrEmpty(path))
            {
                excelFilePath = path;
            }
        }
        
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
        
        // Options
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing Files", overwriteExisting);
        showDebug = EditorGUILayout.Toggle("Show Debug Info", showDebug);
        
        EditorGUILayout.Space();
        
        // Create folder button
        if (!Directory.Exists(localizationFolderPath))
        {
            EditorGUILayout.HelpBox($"Folder '{localizationFolderPath}' does not exist!", MessageType.Warning);
            if (GUILayout.Button("Create Folder"))
            {
                Directory.CreateDirectory(localizationFolderPath);
                AssetDatabase.Refresh();
            }
        }
        
        EditorGUILayout.HelpBox("Warning: This will create/update JSON files in the localization folder.", MessageType.Warning);
        
        EditorGUILayout.Space();
        
        // Import button
        GUI.enabled = !string.IsNullOrEmpty(excelFilePath) && File.Exists(excelFilePath) && Directory.Exists(localizationFolderPath);
        if (GUILayout.Button("Import from Excel", GUILayout.Height(35)))
        {
            ImportFromExcel();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        
        if (!IsNPOIAvailable())
        {
            EditorGUILayout.HelpBox("NPOI library not found! Please install NPOI package.", MessageType.Warning);
        }
    }
    
    private void ImportFromExcel()
    {
        try
        {
            if (!File.Exists(excelFilePath))
            {
                EditorUtility.DisplayDialog("Error", "Excel file not found!", "OK");
                return;
            }
            
            if (!Directory.Exists(localizationFolderPath))
            {
                EditorUtility.DisplayDialog("Error", "Localization folder not found!", "OK");
                return;
            }
            
            if (!IsNPOIAvailable())
            {
                EditorUtility.DisplayDialog("NPOI Not Found", 
                    "NPOI library not found!\n\n" +
                    "Please install NPOI package to use this importer.", 
                    "OK");
                return;
            }
            
            int importedCount = ImportExcelToJSON(excelFilePath, localizationFolderPath);
            
            EditorUtility.DisplayDialog("Import Complete", 
                $"Successfully imported {importedCount} localization files!", 
                "OK");
            
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Import Error", 
                $"Error importing from Excel:\n{e.Message}", 
                "OK");
            Debug.LogError($"Import error: {e}");
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
    
    private int ImportExcelToJSON(string filePath, string outputFolder)
    {
        Dictionary<string, Dictionary<string, string>> localizationData = new Dictionary<string, Dictionary<string, string>>();
        List<string> languages = new List<string>();
        
        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            IWorkbook workbook = new XSSFWorkbook(fileStream);
            ISheet sheet = workbook.GetSheetAt(0); // Use first sheet
            
            if (sheet == null || sheet.LastRowNum < 1)
            {
                EditorUtility.DisplayDialog("Error", "Excel file is empty or invalid!", "OK");
                workbook.Close();
                return 0;
            }
            
            // Read header row (first row)
            IRow headerRow = sheet.GetRow(0);
            if (headerRow == null)
            {
                EditorUtility.DisplayDialog("Error", "Header row not found!", "OK");
                workbook.Close();
                return 0;
            }
            
            // Extract language names from header (skip first column which is "Key")
            Debug.Log($"Header row has {headerRow.LastCellNum} cells");
            for (int i = 1; i <= headerRow.LastCellNum; i++)
            {
                ICell cell = headerRow.GetCell(i);
                if (cell != null)
                {
                    string langName = GetCellValue(cell).Trim();
                    Debug.Log($"Column {i}: '{langName}'");
                    if (!string.IsNullOrEmpty(langName))
                    {
                        languages.Add(langName.ToLower());
                    }
                }
            }
            
            Debug.Log($"Detected {languages.Count} languages: {string.Join(", ", languages)}");
            
            if (languages.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No language columns found in header!\n\nExpected format:\n| Key | Turkish | English |\n| ui_start | Başla | Start |", "OK");
                workbook.Close();
                return 0;
            }
            
            // Read data rows
            for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (row == null) continue;
                
                ICell keyCell = row.GetCell(0);
                if (keyCell == null) continue;
                
                string key = GetCellValue(keyCell).Trim();
                if (string.IsNullOrEmpty(key)) continue;
                
                // Read translations for each language
                for (int langIndex = 0; langIndex < languages.Count; langIndex++)
                {
                    int cellIndex = langIndex + 1;
                    ICell textCell = row.GetCell(cellIndex);
                    string text = textCell != null ? GetCellValue(textCell).Trim() : "";
                    
                    if (!localizationData.ContainsKey(languages[langIndex]))
                    {
                        localizationData[languages[langIndex]] = new Dictionary<string, string>();
                    }
                    
                    localizationData[languages[langIndex]][key] = text;
                }
            }
            
            workbook.Close();
        }
        
        // Create output folder if it doesn't exist
        if (!Directory.Exists(outputFolder))
        {
            Debug.Log($"Creating folder: {outputFolder}");
            Directory.CreateDirectory(outputFolder);
        }
        
        // Write JSON files for each language
        int filesCreated = 0;
        List<string> createdFiles = new List<string>();
        
        Debug.Log($"Output folder: {outputFolder}");
        Debug.Log($"localizationData has {localizationData.Count} languages");
        
        foreach (var langKvp in localizationData)
        {
            try
            {
                string language = langKvp.Key;
                Dictionary<string, string> langData = langKvp.Value;
                
                Debug.Log($"Processing language '{language}' with {langData.Count} keys");
                
                // Create JSON structure
                string jsonContent = JsonConvert.SerializeObject(langData, Formatting.Indented);
                Debug.Log($"JSON content length: {jsonContent.Length}");
                
                // Write to file
                string fileName = $"{language}_localization.json";
                string jsonFilePath = Path.Combine(outputFolder, fileName);
                
                Debug.Log($"Full path: {jsonFilePath}");
                Debug.Log($"File exists: {File.Exists(jsonFilePath)}, Overwrite: {overwriteExisting}");
                
                // Check if file exists
                if (File.Exists(jsonFilePath) && !overwriteExisting)
                {
                    Debug.LogWarning($"File {fileName} already exists. Skipping.");
                    continue;
                }
                
                Debug.Log($"Writing file: {jsonFilePath}");
                File.WriteAllText(jsonFilePath, jsonContent);
                createdFiles.Add(fileName);
                filesCreated++;
                Debug.Log($"✓ Successfully wrote: {fileName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing language {langKvp.Key}: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        Debug.Log($"Created {filesCreated} files: {string.Join(", ", createdFiles)}");
        return filesCreated;
    }
    
    private string GetCellValue(ICell cell)
    {
        if (cell == null) return "";
        
        switch (cell.CellType)
        {
            case CellType.String:
                return cell.StringCellValue ?? "";
            case CellType.Numeric:
                return cell.NumericCellValue.ToString();
            case CellType.Boolean:
                return cell.BooleanCellValue.ToString();
            default:
                return "";
        }
    }
}

