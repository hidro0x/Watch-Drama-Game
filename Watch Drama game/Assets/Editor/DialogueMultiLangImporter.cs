using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text.RegularExpressions;

/// <summary>
/// Excel Importer for Multi-Language format (same row, multiple language columns)
/// Imports both DialogueDatabase (base + scores) and DialogueLocalizationData (translations)
/// </summary>
public class DialogueMultiLangImporter : EditorWindow
{
    private DialogueDatabase targetDatabase;
    private DialogueLocalizationData targetLocalizationData;
    private string excelFilePath = "";
    private List<string> detectedLanguages = new List<string>();
    private List<string> detectedHeaders = new List<string>();
    private string baseLanguage = "TR"; // Default to TR since that's where the data is
    private bool importToDatabase = true;
    private bool importToLocalization = true;
    private bool showDebugInfo = false;
    private Vector2 scrollPosition;
    private Vector2 headerScrollPosition;
    
    [MenuItem("Kahin/Dialogue Multi-Language Importer")]
    public static void ShowWindow()
    {
        GetWindow<DialogueMultiLangImporter>("Multi-Language Importer");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Dialogue Multi-Language Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Imports multi-language Excel files (same row format).\n\n" +
            "• Imports scores to DialogueDatabase\n" +
            "• Imports ALL language texts to DialogueLocalizationData\n" +
            "• Base language text goes to DialogueDatabase too", 
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        // Excel file
        EditorGUILayout.LabelField("Excel File:", excelFilePath);
        if (GUILayout.Button("Choose Excel File"))
        {
            string path = EditorUtility.OpenFilePanel("Choose Excel File", "", "xlsx");
            if (!string.IsNullOrEmpty(path))
            {
                excelFilePath = path;
                DetectLanguages();
            }
        }
        
        // Debug toggle
        showDebugInfo = EditorGUILayout.Toggle("Show Debug Info", showDebugInfo);
        
        // Show detected headers (debug)
        if (showDebugInfo && detectedHeaders.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Detected Headers:", EditorStyles.boldLabel);
            headerScrollPosition = EditorGUILayout.BeginScrollView(headerScrollPosition, GUILayout.Height(60));
            EditorGUILayout.LabelField(string.Join(" | ", detectedHeaders), EditorStyles.wordWrappedLabel);
            EditorGUILayout.EndScrollView();
        }
        
        // Show detected languages
        if (detectedLanguages.Count > 0)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Detected Languages ({detectedLanguages.Count}):", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(50));
            EditorGUILayout.BeginHorizontal();
            foreach (var lang in detectedLanguages)
            {
                GUILayout.Label(lang, EditorStyles.helpBox);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
            
            // Base language selection
            int baseIndex = detectedLanguages.IndexOf(baseLanguage);
            if (baseIndex < 0) baseIndex = 0;
            baseIndex = EditorGUILayout.Popup("Base Language (for Database)", baseIndex, detectedLanguages.ToArray());
            if (baseIndex < detectedLanguages.Count)
            {
                baseLanguage = detectedLanguages[baseIndex];
            }
        }
        else if (!string.IsNullOrEmpty(excelFilePath))
        {
            EditorGUILayout.HelpBox("No languages detected! Make sure columns are named like: Name_EN, Text_TR, Choice1_EN", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        
        // Target assets
        EditorGUILayout.LabelField("Import Targets:", EditorStyles.boldLabel);
        
        importToDatabase = EditorGUILayout.Toggle("Import to DialogueDatabase", importToDatabase);
        if (importToDatabase)
        {
            targetDatabase = (DialogueDatabase)EditorGUILayout.ObjectField(
                "  Dialogue Database", targetDatabase, typeof(DialogueDatabase), false);
        }
        
        importToLocalization = EditorGUILayout.Toggle("Import to LocalizationData", importToLocalization);
        if (importToLocalization)
        {
            targetLocalizationData = (DialogueLocalizationData)EditorGUILayout.ObjectField(
                "  Localization Data", targetLocalizationData, typeof(DialogueLocalizationData), false);
        }
        
        EditorGUILayout.Space();
        
        // Import button
        bool canImport = !string.IsNullOrEmpty(excelFilePath) && File.Exists(excelFilePath) &&
                         detectedLanguages.Count > 0 &&
                         ((importToDatabase && targetDatabase != null) || 
                          (importToLocalization && targetLocalizationData != null));
        
        GUI.enabled = canImport;
        if (GUILayout.Button("Import", GUILayout.Height(40)))
        {
            Import();
        }
        GUI.enabled = true;
        
        if (!canImport && !string.IsNullOrEmpty(excelFilePath))
        {
            if (detectedLanguages.Count == 0)
            {
                EditorGUILayout.HelpBox("Cannot import: No languages detected in Excel file.", MessageType.Error);
            }
        }
        
        EditorGUILayout.Space();
        
        // Preview button
        if (!string.IsNullOrEmpty(excelFilePath) && File.Exists(excelFilePath))
        {
            if (GUILayout.Button("Preview Excel Contents", GUILayout.Height(30)))
            {
                PreviewExcel();
            }
        }
    }
    
    private void PreviewExcel()
    {
        try
        {
            string preview = "";
            
            using (FileStream fs = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(fs);
                
                preview += $"=== EXCEL PREVIEW ===\n";
                preview += $"Total Sheets: {workbook.NumberOfSheets}\n\n";
                
                for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
                {
                    ISheet sheet = workbook.GetSheetAt(sheetIndex);
                    string sheetName = sheet.SheetName;
                    
                    preview += $"--- Sheet {sheetIndex + 1}: '{sheetName}' ---\n";
                    preview += $"Rows: {sheet.LastRowNum + 1}\n";
                    
                    // Show header row
                    IRow headerRow = sheet.GetRow(0);
                    if (headerRow != null)
                    {
                        preview += "Headers: ";
                        List<string> headers = new List<string>();
                        for (int i = 0; i < headerRow.LastCellNum; i++)
                        {
                            ICell cell = headerRow.GetCell(i);
                            string val = GetCellStringValue(cell);
                            if (!string.IsNullOrEmpty(val))
                            {
                                headers.Add($"[{i}]{val}");
                            }
                        }
                        preview += string.Join(", ", headers) + "\n";
                    }
                    
                    // Show first data row
                    if (sheet.LastRowNum >= 1)
                    {
                        IRow dataRow = sheet.GetRow(1);
                        if (dataRow != null)
                        {
                            preview += "First Row: ";
                            List<string> values = new List<string>();
                            for (int i = 0; i < dataRow.LastCellNum && i < 10; i++)
                            {
                                ICell cell = dataRow.GetCell(i);
                                string val = GetCellStringValue(cell);
                                values.Add($"[{i}]{(val.Length > 20 ? val.Substring(0, 20) + "..." : val)}");
                            }
                            preview += string.Join(", ", values) + "\n";
                        }
                    }
                    
                    // Check if sheet name matches expected patterns
                    bool matches = sheetName == "General Dialogues" || 
                                   sheetName == "Global Dialogues" ||
                                   sheetName.StartsWith("Map - ") ||
                                   sheetName.StartsWith("Map: ");
                    preview += $"Sheet Name Matches Pattern: {(matches ? "YES ✓" : "NO ✗")}\n";
                    preview += "\n";
                }
                
                workbook.Close();
            }
            
            Debug.Log(preview);
            EditorUtility.DisplayDialog("Excel Preview", preview, "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Preview Error", e.Message, "OK");
            Debug.LogError($"Preview error: {e}");
        }
    }
    
    private void DetectLanguages()
    {
        detectedLanguages.Clear();
        detectedHeaders.Clear();
        
        if (!File.Exists(excelFilePath)) return;
        
        try
        {
            using (FileStream fs = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(fs);
                
                if (workbook.NumberOfSheets > 0)
                {
                    ISheet sheet = workbook.GetSheetAt(0);
                    IRow headerRow = sheet.GetRow(0);
                    
                    if (headerRow != null)
                    {
                        HashSet<string> langs = new HashSet<string>();
                        // Match patterns like: Name_EN, Text_TR, Choice1_EN, Option1_TR, Speaker_EN, etc.
                        Regex langRegex = new Regex(@"^(Name|Text|Choice\d+|Option\d+|Speaker)_(\w+)$", RegexOptions.IgnoreCase);
                        
                        for (int i = 0; i < headerRow.LastCellNum; i++)
                        {
                            ICell cell = headerRow.GetCell(i);
                            if (cell != null)
                            {
                                string header = GetCellStringValue(cell);
                                if (!string.IsNullOrEmpty(header))
                                {
                                    detectedHeaders.Add(header);
                                    
                                    Match match = langRegex.Match(header);
                                    if (match.Success)
                                    {
                                        string lang = match.Groups[2].Value;
                                        langs.Add(lang);
                                        Debug.Log($"Found language column: {header} -> {lang}");
                                    }
                                }
                            }
                        }
                        
                        detectedLanguages.AddRange(langs);
                        detectedLanguages.Sort();
                        
                        Debug.Log($"Detected {detectedLanguages.Count} languages: {string.Join(", ", detectedLanguages)}");
                        Debug.Log($"Detected {detectedHeaders.Count} headers: {string.Join(", ", detectedHeaders)}");
                    }
                    else
                    {
                        Debug.LogWarning("Header row is null!");
                    }
                }
                else
                {
                    Debug.LogWarning("No sheets in workbook!");
                }
                
                workbook.Close();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error detecting languages: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private string GetCellStringValue(ICell cell)
    {
        if (cell == null) return "";
        
        try
        {
            switch (cell.CellType)
            {
                case CellType.String:
                    return cell.StringCellValue ?? "";
                case CellType.Numeric:
                    return cell.NumericCellValue.ToString();
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                case CellType.Formula:
                    // Try to get cached value
                    try { return cell.StringCellValue ?? ""; }
                    catch { return cell.NumericCellValue.ToString(); }
                default:
                    return "";
            }
        }
        catch
        {
            return "";
        }
    }
    
    private void Import()
    {
        try
        {
            int dbImported = 0;
            int locImported = 0;
            List<string> importedSheets = new List<string>();
            
            Debug.Log($"Starting import from: {excelFilePath}");
            Debug.Log($"Base language: {baseLanguage}");
            Debug.Log($"Detected languages: {string.Join(", ", detectedLanguages)}");
            
            using (FileStream fs = new FileStream(excelFilePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(fs);
                
                Debug.Log($"Workbook has {workbook.NumberOfSheets} sheets");
                
                for (int sheetIndex = 0; sheetIndex < workbook.NumberOfSheets; sheetIndex++)
                {
                    ISheet sheet = workbook.GetSheetAt(sheetIndex);
                    string sheetName = sheet.SheetName;
                    
                    Debug.Log($"Processing sheet: '{sheetName}' with {sheet.LastRowNum} rows");
                    
                    // Parse header to get column mappings
                    var columnMap = ParseHeader(sheet);
                    if (columnMap == null || columnMap.Count == 0)
                    {
                        Debug.LogWarning($"Sheet '{sheetName}' has no valid headers, skipping");
                        continue;
                    }
                    
                    Debug.Log($"Column map for '{sheetName}': {string.Join(", ", columnMap.Keys)}");
                    
                    // Check if this is the new format with Type/MapType columns
                    bool hasTypeColumn = columnMap.ContainsKey("Type") || columnMap.ContainsKey("type");
                    bool hasMapTypeColumn = columnMap.ContainsKey("MapType") || columnMap.ContainsKey("maptype");
                    
                    if (hasTypeColumn || hasMapTypeColumn)
                    {
                        // NEW FORMAT: Single "Dialogues" sheet with Type and MapType columns
                        Debug.Log("Detected NEW format with Type/MapType columns");
                        var result = ImportUnifiedDialogueSheet(sheet, columnMap);
                        dbImported += result.Item1;
                        locImported += result.Item2;
                        importedSheets.Add($"Unified: {result.Item1} db, {result.Item2} loc");
                    }
                    else if (sheetName == "General Dialogues")
                    {
                        var result = ImportDialogueSheet(sheet, columnMap, null);
                        dbImported += result.Item1;
                        locImported += result.Item2;
                        importedSheets.Add($"General: {result.Item1} db, {result.Item2} loc");
                    }
                    else if (sheetName.StartsWith("Map - ") || sheetName.StartsWith("Map: "))
                    {
                        string mapTypeStr = sheetName.StartsWith("Map - ") 
                            ? sheetName.Substring(6) 
                            : sheetName.Substring(5);
                        
                        if (System.Enum.TryParse<MapType>(mapTypeStr, out MapType mapType))
                        {
                            var result = ImportDialogueSheet(sheet, columnMap, mapType);
                            dbImported += result.Item1;
                            locImported += result.Item2;
                            importedSheets.Add($"{mapType}: {result.Item1} db, {result.Item2} loc");
                        }
                        else
                        {
                            Debug.LogWarning($"Could not parse MapType from: {mapTypeStr}");
                        }
                    }
                    else if (sheetName == "Global Dialogues")
                    {
                        var result = ImportGlobalDialogueSheet(sheet, columnMap);
                        dbImported += result.Item1;
                        locImported += result.Item2;
                        importedSheets.Add($"Global: {result.Item1} db, {result.Item2} loc");
                    }
                    else
                    {
                        // Try to import any sheet as unified format
                        Debug.Log($"Trying to import '{sheetName}' as unified format");
                        var result = ImportUnifiedDialogueSheet(sheet, columnMap);
                        dbImported += result.Item1;
                        locImported += result.Item2;
                        importedSheets.Add($"{sheetName}: {result.Item1} db, {result.Item2} loc");
                    }
                }
                
                workbook.Close();
            }
            
            // Save assets
            if (importToDatabase && targetDatabase != null)
            {
                EditorUtility.SetDirty(targetDatabase);
                Debug.Log($"Saved DialogueDatabase with {targetDatabase.generalDialogues?.Count ?? 0} general dialogues");
            }
            if (importToLocalization && targetLocalizationData != null)
            {
                EditorUtility.SetDirty(targetLocalizationData);
                Debug.Log($"Saved LocalizationData with {targetLocalizationData.GetDialogueCount()} dialogues");
            }
            AssetDatabase.SaveAssets();
            
            string sheetDetails = importedSheets.Count > 0 ? $"\n\nDetails:\n{string.Join("\n", importedSheets)}" : "";
            
            EditorUtility.DisplayDialog("Import Complete",
                $"Database: {dbImported} dialogues\nLocalization: {locImported} translations{sheetDetails}",
                "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Import Error", $"{e.Message}\n\nCheck Console for details.", "OK");
            Debug.LogError($"Import error: {e}\n{e.StackTrace}");
        }
    }
    
    private Dictionary<string, int> ParseHeader(ISheet sheet)
    {
        IRow headerRow = sheet.GetRow(0);
        if (headerRow == null)
        {
            Debug.LogWarning($"Sheet '{sheet.SheetName}' has no header row");
            return null;
        }
        
        var map = new Dictionary<string, int>();
        
        for (int i = 0; i < headerRow.LastCellNum; i++)
        {
            ICell cell = headerRow.GetCell(i);
            string headerValue = GetCellStringValue(cell);
            if (!string.IsNullOrEmpty(headerValue))
            {
                map[headerValue] = i;
            }
        }
        
        return map;
    }
    
    private (int, int) ImportDialogueSheet(ISheet sheet, Dictionary<string, int> columnMap, MapType? mapType)
    {
        int dbCount = 0;
        int locCount = 0;
        
        for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            IRow row = sheet.GetRow(rowIndex);
            if (row == null) continue;
            
            string dialogueId = GetCellValue(row, columnMap, "ID");
            if (string.IsNullOrEmpty(dialogueId)) continue;
            
            // Import to Database (base language + scores)
            if (importToDatabase && targetDatabase != null)
            {
                DialogueNode node = new DialogueNode();
                node.id = dialogueId;
                node.name = GetCellValue(row, columnMap, $"Name_{baseLanguage}");
                node.text = GetCellValue(row, columnMap, $"Text_{baseLanguage}");
                node.choices = new List<DialogueChoice>();
                
                // Parse choices with scores
                for (int c = 1; c <= 3; c++)
                {
                    string choiceText = GetCellValue(row, columnMap, $"Choice{c}_{baseLanguage}");
                    if (!string.IsNullOrEmpty(choiceText))
                    {
                        DialogueChoice choice = new DialogueChoice();
                        choice.text = choiceText;
                        int.TryParse(GetCellValue(row, columnMap, $"Faith{c}"), out choice.faithChange);
                        int.TryParse(GetCellValue(row, columnMap, $"Trust{c}"), out choice.trustChange);
                        int.TryParse(GetCellValue(row, columnMap, $"Hostility{c}"), out choice.hostilityChange);
                        node.choices.Add(choice);
                    }
                }
                
                // Add to database
                if (mapType.HasValue)
                {
                    if (targetDatabase.specialGeneralDialoguesByMap == null)
                        targetDatabase.specialGeneralDialoguesByMap = new Dictionary<MapType, List<DialogueNode>>();
                    if (!targetDatabase.specialGeneralDialoguesByMap.ContainsKey(mapType.Value))
                        targetDatabase.specialGeneralDialoguesByMap[mapType.Value] = new List<DialogueNode>();
                    
                    var list = targetDatabase.specialGeneralDialoguesByMap[mapType.Value];
                    int existingIndex = list.FindIndex(d => d.id == dialogueId);
                    if (existingIndex >= 0)
                    {
                        node.sprite = list[existingIndex].sprite;
                        node.backgroundSprite = list[existingIndex].backgroundSprite;
                        list[existingIndex] = node;
                    }
                    else
                    {
                        list.Add(node);
                    }
                }
                else
                {
                    if (targetDatabase.generalDialogues == null)
                        targetDatabase.generalDialogues = new List<DialogueNode>();
                    
                    int existingIndex = targetDatabase.generalDialogues.FindIndex(d => d.id == dialogueId);
                    if (existingIndex >= 0)
                    {
                        node.sprite = targetDatabase.generalDialogues[existingIndex].sprite;
                        node.backgroundSprite = targetDatabase.generalDialogues[existingIndex].backgroundSprite;
                        targetDatabase.generalDialogues[existingIndex] = node;
                    }
                    else
                    {
                        targetDatabase.generalDialogues.Add(node);
                    }
                }
                
                dbCount++;
            }
            
            // Import to LocalizationData (all languages)
            if (importToLocalization && targetLocalizationData != null)
            {
                foreach (var lang in detectedLanguages)
                {
                    LocalizedDialogueText locText = new LocalizedDialogueText();
                    locText.name = GetCellValue(row, columnMap, $"Name_{lang}");
                    locText.text = GetCellValue(row, columnMap, $"Text_{lang}");
                    locText.choiceTexts = new List<string>();
                    
                    for (int c = 1; c <= 4; c++)
                    {
                        locText.choiceTexts.Add(GetCellValue(row, columnMap, $"Choice{c}_{lang}"));
                    }
                    
                    // Only add if there's actual content
                    if (!string.IsNullOrEmpty(locText.name) || !string.IsNullOrEmpty(locText.text))
                    {
                        targetLocalizationData.SetLocalizedDialogue(dialogueId, GetFullLanguageName(lang), locText);
                        locCount++;
                    }
                }
            }
        }
        
        return (dbCount, locCount);
    }
    
    /// <summary>
    /// Import unified format: Single sheet with Type and MapType columns
    /// Columns: ID, Type, MapType, Name_TR, Speaker_TR, Text_TR, Option1_TR, Option2_TR, Option3_TR, Name_EN, etc.
    /// </summary>
    private (int, int) ImportUnifiedDialogueSheet(ISheet sheet, Dictionary<string, int> columnMap)
    {
        int dbCount = 0;
        int locCount = 0;
        
        for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            IRow row = sheet.GetRow(rowIndex);
            if (row == null) continue;
            
            string dialogueId = GetCellValue(row, columnMap, "ID");
            if (string.IsNullOrEmpty(dialogueId)) continue;
            
            string dialogueType = GetCellValue(row, columnMap, "Type").ToLower();
            string mapTypeStr = GetCellValue(row, columnMap, "MapType");
            
            // Determine map type
            MapType? mapType = null;
            if (!string.IsNullOrEmpty(mapTypeStr) && System.Enum.TryParse<MapType>(mapTypeStr, true, out MapType parsedMapType))
            {
                mapType = parsedMapType;
            }
            
            bool isGlobal = dialogueType == "global";
            
            // Import to Database (base language + scores)
            if (importToDatabase && targetDatabase != null)
            {
                // Get name - try Name_{lang} first, then Speaker_{lang}
                string name = GetCellValueMultiKey(row, columnMap, $"Name_{baseLanguage}", $"Speaker_{baseLanguage}");
                string text = GetCellValue(row, columnMap, $"Text_{baseLanguage}");
                
                // Warn if base language data is empty
                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(text))
                {
                    Debug.LogWarning($"Row {rowIndex}: No data found for base language '{baseLanguage}'. Check if columns Name_{baseLanguage} and Text_{baseLanguage} exist and have data.");
                }
                
                Debug.Log($"Row {rowIndex}: ID={dialogueId}, Type={dialogueType}, MapType={mapTypeStr}, Name={name}, Text={(text.Length > 30 ? text.Substring(0, 30) + "..." : text)}");
                
                if (isGlobal)
                {
                    // Import as global dialogue
                    GlobalDialogueNode node = new GlobalDialogueNode();
                    node.id = dialogueId;
                    node.name = name;
                    node.text = text;
                    node.choices = new List<GlobalDialogueChoice>();
                    
                    // Parse choices (Option1, Option2, Option3)
                    for (int c = 1; c <= 3; c++)
                    {
                        string choiceText = GetCellValueMultiKey(row, columnMap, $"Option{c}_{baseLanguage}", $"Choice{c}_{baseLanguage}");
                        if (!string.IsNullOrEmpty(choiceText))
                        {
                            GlobalDialogueChoice choice = new GlobalDialogueChoice();
                            choice.text = choiceText;
                            choice.globalEffects = new List<CountryBarEffect>();
                            
                            // Parse effects for this choice
                            foreach (MapType mt in System.Enum.GetValues(typeof(MapType)))
                            {
                                CountryBarEffect effect = new CountryBarEffect();
                                effect.country = mt;
                                int.TryParse(GetCellValue(row, columnMap, $"Faith{c}"), out effect.faithChange);
                                int.TryParse(GetCellValue(row, columnMap, $"Trust{c}"), out effect.trustChange);
                                int.TryParse(GetCellValue(row, columnMap, $"Hostility{c}"), out effect.hostilityChange);
                                choice.globalEffects.Add(effect);
                            }
                            
                            node.choices.Add(choice);
                        }
                    }
                    
                    if (targetDatabase.globalDialogueEffects == null)
                        targetDatabase.globalDialogueEffects = new List<GlobalDialogueNode>();
                    
                    int existingIndex = targetDatabase.globalDialogueEffects.FindIndex(d => d.id == dialogueId);
                    if (existingIndex >= 0)
                    {
                        node.sprite = targetDatabase.globalDialogueEffects[existingIndex].sprite;
                        targetDatabase.globalDialogueEffects[existingIndex] = node;
                    }
                    else
                    {
                        targetDatabase.globalDialogueEffects.Add(node);
                    }
                }
                else
                {
                    // Import as normal dialogue
                    DialogueNode node = new DialogueNode();
                    node.id = dialogueId;
                    node.name = name;
                    node.text = text;
                    node.choices = new List<DialogueChoice>();
                    
                    // Parse choices with scores (Option1/Choice1, etc.)
                    for (int c = 1; c <= 3; c++)
                    {
                        string choiceText = GetCellValueMultiKey(row, columnMap, $"Option{c}_{baseLanguage}", $"Choice{c}_{baseLanguage}");
                        if (!string.IsNullOrEmpty(choiceText))
                        {
                            DialogueChoice choice = new DialogueChoice();
                            choice.text = choiceText;
                            int.TryParse(GetCellValue(row, columnMap, $"Faith{c}"), out choice.faithChange);
                            int.TryParse(GetCellValue(row, columnMap, $"Trust{c}"), out choice.trustChange);
                            int.TryParse(GetCellValue(row, columnMap, $"Hostility{c}"), out choice.hostilityChange);
                            node.choices.Add(choice);
                        }
                    }
                    
                    // Add to appropriate list based on MapType
                    if (mapType.HasValue)
                    {
                        if (targetDatabase.specialGeneralDialoguesByMap == null)
                            targetDatabase.specialGeneralDialoguesByMap = new Dictionary<MapType, List<DialogueNode>>();
                        if (!targetDatabase.specialGeneralDialoguesByMap.ContainsKey(mapType.Value))
                            targetDatabase.specialGeneralDialoguesByMap[mapType.Value] = new List<DialogueNode>();
                        
                        var list = targetDatabase.specialGeneralDialoguesByMap[mapType.Value];
                        int existingIndex = list.FindIndex(d => d.id == dialogueId);
                        if (existingIndex >= 0)
                        {
                            node.sprite = list[existingIndex].sprite;
                            node.backgroundSprite = list[existingIndex].backgroundSprite;
                            list[existingIndex] = node;
                        }
                        else
                        {
                            list.Add(node);
                        }
                    }
                    else
                    {
                        if (targetDatabase.generalDialogues == null)
                            targetDatabase.generalDialogues = new List<DialogueNode>();
                        
                        int existingIndex = targetDatabase.generalDialogues.FindIndex(d => d.id == dialogueId);
                        if (existingIndex >= 0)
                        {
                            node.sprite = targetDatabase.generalDialogues[existingIndex].sprite;
                            node.backgroundSprite = targetDatabase.generalDialogues[existingIndex].backgroundSprite;
                            targetDatabase.generalDialogues[existingIndex] = node;
                        }
                        else
                        {
                            targetDatabase.generalDialogues.Add(node);
                        }
                    }
                }
                
                dbCount++;
            }
            
            // Import to LocalizationData (all languages)
            if (importToLocalization && targetLocalizationData != null)
            {
                foreach (var lang in detectedLanguages)
                {
                    LocalizedDialogueText locText = new LocalizedDialogueText();
                    locText.name = GetCellValueMultiKey(row, columnMap, $"Name_{lang}", $"Speaker_{lang}");
                    locText.text = GetCellValue(row, columnMap, $"Text_{lang}");
                    locText.choiceTexts = new List<string>();
                    
                    for (int c = 1; c <= 4; c++)
                    {
                        locText.choiceTexts.Add(GetCellValueMultiKey(row, columnMap, $"Option{c}_{lang}", $"Choice{c}_{lang}"));
                    }
                    
                    // Only add if there's actual content
                    if (!string.IsNullOrEmpty(locText.name) || !string.IsNullOrEmpty(locText.text))
                    {
                        if (isGlobal)
                        {
                            targetLocalizationData.SetLocalizedGlobalDialogue(dialogueId, GetFullLanguageName(lang), locText);
                        }
                        else
                        {
                            targetLocalizationData.SetLocalizedDialogue(dialogueId, GetFullLanguageName(lang), locText);
                        }
                        locCount++;
                    }
                }
            }
        }
        
        return (dbCount, locCount);
    }
    
    /// <summary>
    /// Try multiple column keys and return the first non-empty value
    /// </summary>
    private string GetCellValueMultiKey(IRow row, Dictionary<string, int> columnMap, params string[] keys)
    {
        foreach (var key in keys)
        {
            string value = GetCellValue(row, columnMap, key);
            if (!string.IsNullOrEmpty(value))
            {
                return value;
            }
        }
        return "";
    }
    
    private (int, int) ImportGlobalDialogueSheet(ISheet sheet, Dictionary<string, int> columnMap)
    {
        int dbCount = 0;
        int locCount = 0;
        
        for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
        {
            IRow row = sheet.GetRow(rowIndex);
            if (row == null) continue;
            
            string dialogueId = GetCellValue(row, columnMap, "ID");
            if (string.IsNullOrEmpty(dialogueId)) continue;
            
            // Import to Database
            if (importToDatabase && targetDatabase != null)
            {
                GlobalDialogueNode node = new GlobalDialogueNode();
                node.id = dialogueId;
                node.name = GetCellValue(row, columnMap, $"Name_{baseLanguage}");
                node.text = GetCellValue(row, columnMap, $"Text_{baseLanguage}");
                node.choices = new List<GlobalDialogueChoice>();
                
                string choiceText = GetCellValue(row, columnMap, $"Choice1_{baseLanguage}");
                if (!string.IsNullOrEmpty(choiceText))
                {
                    GlobalDialogueChoice choice = new GlobalDialogueChoice();
                    choice.text = choiceText;
                    choice.globalEffects = new List<CountryBarEffect>();
                    
                    foreach (MapType mapType in System.Enum.GetValues(typeof(MapType)))
                    {
                        CountryBarEffect effect = new CountryBarEffect();
                        effect.country = mapType;
                        int.TryParse(GetCellValue(row, columnMap, $"{mapType}_F"), out effect.faithChange);
                        int.TryParse(GetCellValue(row, columnMap, $"{mapType}_T"), out effect.trustChange);
                        int.TryParse(GetCellValue(row, columnMap, $"{mapType}_H"), out effect.hostilityChange);
                        choice.globalEffects.Add(effect);
                    }
                    
                    node.choices.Add(choice);
                }
                
                if (targetDatabase.globalDialogueEffects == null)
                    targetDatabase.globalDialogueEffects = new List<GlobalDialogueNode>();
                
                int existingIndex = targetDatabase.globalDialogueEffects.FindIndex(d => d.id == dialogueId);
                if (existingIndex >= 0)
                {
                    node.sprite = targetDatabase.globalDialogueEffects[existingIndex].sprite;
                    targetDatabase.globalDialogueEffects[existingIndex] = node;
                }
                else
                {
                    targetDatabase.globalDialogueEffects.Add(node);
                }
                
                dbCount++;
            }
            
            // Import to LocalizationData
            if (importToLocalization && targetLocalizationData != null)
            {
                foreach (var lang in detectedLanguages)
                {
                    LocalizedDialogueText locText = new LocalizedDialogueText();
                    locText.name = GetCellValue(row, columnMap, $"Name_{lang}");
                    locText.text = GetCellValue(row, columnMap, $"Text_{lang}");
                    locText.choiceTexts = new List<string> { GetCellValue(row, columnMap, $"Choice1_{lang}") };
                    
                    if (!string.IsNullOrEmpty(locText.name) || !string.IsNullOrEmpty(locText.text))
                    {
                        targetLocalizationData.SetLocalizedGlobalDialogue(dialogueId, GetFullLanguageName(lang), locText);
                        locCount++;
                    }
                }
            }
        }
        
        return (dbCount, locCount);
    }
    
    private string GetCellValue(IRow row, Dictionary<string, int> columnMap, string columnName)
    {
        if (!columnMap.TryGetValue(columnName, out int colIndex))
        {
            // Try case-insensitive search
            foreach (var kvp in columnMap)
            {
                if (kvp.Key.Equals(columnName, System.StringComparison.OrdinalIgnoreCase))
                {
                    colIndex = kvp.Value;
                    break;
                }
            }
            if (colIndex == 0 && !columnMap.ContainsKey(columnName))
            {
                return "";
            }
        }
        
        ICell cell = row.GetCell(colIndex);
        return GetCellStringValue(cell);
    }
    
    private string GetFullLanguageName(string shortCode)
    {
        switch (shortCode.ToUpper())
        {
            case "EN": return "English";
            case "TR": return "Turkish";
            case "DE": return "German";
            case "FR": return "French";
            case "ES": return "Spanish";
            default: return shortCode;
        }
    }
}

