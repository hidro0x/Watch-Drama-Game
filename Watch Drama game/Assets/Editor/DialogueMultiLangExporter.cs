using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

/// <summary>
/// Excel Exporter with Multi-Language columns in SAME ROW
/// Scores are defined once, text columns are per-language
/// Matches the user's Excel format: ID, Type, MapType, Name_TR, Text_TR, Option1_TR, etc.
/// </summary>
public class DialogueMultiLangExporter : EditorWindow
{
    private DialogueDatabase dialogueDatabase;
    private DialogueLocalizationData localizationData;
    private string exportPath = "Assets/Exports";
    private List<string> languages = new List<string> { "TR", "EN" };
    private string newLanguage = "";
    private bool useOptionInsteadOfChoice = true;
    private bool includeTypeAndMapType = true;
    private bool exportAllInOneSheet = true; // Export all dialogues in one sheet like user's format
    private Vector2 scrollPosition;
    
    [MenuItem("Kahin/Dialogue Multi-Language Exporter")]
    public static void ShowWindow()
    {
        GetWindow<DialogueMultiLangExporter>("Multi-Language Exporter");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Dialogue Multi-Language Exporter", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "Exports dialogues with ALL languages in the SAME ROW.\n\n" +
            "Format: ID | Type | MapType | Name_TR | Name_EN | Text_TR | Text_EN | Option1_TR | Option1_EN | Faith1 | Trust1 | Hostility1\n\n" +
            "✅ Scores are defined ONCE per dialogue\n" +
            "✅ Easy to compare translations side-by-side", 
            MessageType.Info);
        
        EditorGUILayout.Space();
        
        // Data sources
        dialogueDatabase = (DialogueDatabase)EditorGUILayout.ObjectField(
            "Dialogue Database", dialogueDatabase, typeof(DialogueDatabase), false);
        
        localizationData = (DialogueLocalizationData)EditorGUILayout.ObjectField(
            "Localization Data (Optional)", localizationData, typeof(DialogueLocalizationData), false);
        
        EditorGUILayout.Space();
        
        // Languages
        EditorGUILayout.LabelField("Languages:", EditorStyles.boldLabel);
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(80));
        for (int i = 0; i < languages.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            languages[i] = EditorGUILayout.TextField(languages[i], GUILayout.Width(100));
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                languages.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
        
        EditorGUILayout.BeginHorizontal();
        newLanguage = EditorGUILayout.TextField(newLanguage, GUILayout.Width(100));
        if (GUILayout.Button("Add Language", GUILayout.Width(100)) && !string.IsNullOrEmpty(newLanguage))
        {
            if (!languages.Contains(newLanguage))
            {
                languages.Add(newLanguage);
            }
            newLanguage = "";
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Format options
        EditorGUILayout.LabelField("Format Options:", EditorStyles.boldLabel);
        useOptionInsteadOfChoice = EditorGUILayout.Toggle("Use 'Option' instead of 'Choice'", useOptionInsteadOfChoice);
        includeTypeAndMapType = EditorGUILayout.Toggle("Include Type & MapType columns", includeTypeAndMapType);
        exportAllInOneSheet = EditorGUILayout.Toggle("Export all in one 'Dialogues' sheet", exportAllInOneSheet);
        
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
        GUI.enabled = dialogueDatabase != null && languages.Count > 0;
        if (GUILayout.Button("Export Multi-Language Excel", GUILayout.Height(40)))
        {
            ExportToExcel();
        }
        GUI.enabled = true;
    }
    
    private void ExportToExcel()
    {
        try
        {
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }
            
            string filePath = Path.Combine(exportPath, "dialogues_multilang.xlsx");
            
            IWorkbook workbook = new XSSFWorkbook();
            
            // Create styles
            ICellStyle headerStyle = CreateHeaderStyle(workbook);
            ICellStyle textStyle = CreateTextStyle(workbook);
            ICellStyle numberStyle = CreateNumberStyle(workbook);
            
            if (exportAllInOneSheet)
            {
                // Export all dialogues in one sheet (like user's format)
                ExportAllDialoguesToSingleSheet(workbook, headerStyle, textStyle, numberStyle);
            }
            else
            {
                // Export to separate sheets
                if (dialogueDatabase.generalDialogues != null && dialogueDatabase.generalDialogues.Count > 0)
                {
                    ExportDialoguesSheet(workbook, "General Dialogues", dialogueDatabase.generalDialogues, 
                        headerStyle, textStyle, numberStyle, "normal", "");
                }
                
                if (dialogueDatabase.specialGeneralDialoguesByMap != null)
                {
                    foreach (var kvp in dialogueDatabase.specialGeneralDialoguesByMap)
                    {
                        if (kvp.Value != null && kvp.Value.Count > 0)
                        {
                            string sheetName = SanitizeSheetName($"Map - {kvp.Key}");
                            ExportDialoguesSheet(workbook, sheetName, kvp.Value, 
                                headerStyle, textStyle, numberStyle, "normal", kvp.Key.ToString());
                        }
                    }
                }
                
                if (dialogueDatabase.globalDialogueEffects != null && dialogueDatabase.globalDialogueEffects.Count > 0)
                {
                    ExportGlobalDialoguesSheet(workbook, "Global Dialogues", headerStyle, textStyle, numberStyle);
                }
            }
            
            // Write to file
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                workbook.Write(fileStream);
            }
            
            workbook.Close();
            
            EditorUtility.DisplayDialog("Export Complete", 
                $"Successfully exported to:\n{filePath}\n\nLanguages: {string.Join(", ", languages)}", 
                "OK");
            
            EditorUtility.RevealInFinder(filePath);
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Export Error", $"Error: {e.Message}", "OK");
            Debug.LogError($"Export error: {e}");
        }
    }
    
    private void ExportAllDialoguesToSingleSheet(IWorkbook workbook, ICellStyle headerStyle, ICellStyle textStyle, ICellStyle numberStyle)
    {
        ISheet sheet = workbook.CreateSheet("Dialogues");
        
        // Create header
        IRow headerRow = sheet.CreateRow(0);
        List<string> headers = CreateHeaders();
        
        for (int i = 0; i < headers.Count; i++)
        {
            ICell cell = headerRow.CreateCell(i);
            cell.SetCellValue(headers[i]);
            cell.CellStyle = headerStyle;
        }
        
        int rowIndex = 1;
        
        // Export general dialogues
        if (dialogueDatabase.generalDialogues != null)
        {
            foreach (var dialogue in dialogueDatabase.generalDialogues)
            {
                ExportDialogueRow(sheet, rowIndex++, dialogue, textStyle, numberStyle, "normal", "");
            }
        }
        
        // Export map-specific dialogues
        if (dialogueDatabase.specialGeneralDialoguesByMap != null)
        {
            foreach (var kvp in dialogueDatabase.specialGeneralDialoguesByMap)
            {
                if (kvp.Value != null)
                {
                    foreach (var dialogue in kvp.Value)
                    {
                        ExportDialogueRow(sheet, rowIndex++, dialogue, textStyle, numberStyle, "normal", kvp.Key.ToString());
                    }
                }
            }
        }
        
        // Export global dialogues
        if (dialogueDatabase.globalDialogueEffects != null)
        {
            foreach (var dialogue in dialogueDatabase.globalDialogueEffects)
            {
                ExportGlobalDialogueRow(sheet, rowIndex++, dialogue, textStyle, numberStyle);
            }
        }
        
        AutoSizeColumns(sheet, headers.Count);
    }
    
    private List<string> CreateHeaders()
    {
        List<string> headers = new List<string> { "ID" };
        
        if (includeTypeAndMapType)
        {
            headers.Add("Type");
            headers.Add("MapType");
        }
        
        // Name columns for each language
        foreach (var lang in languages)
        {
            headers.Add($"Name_{lang}");
        }
        
        // Text columns for each language
        foreach (var lang in languages)
        {
            headers.Add($"Text_{lang}");
        }
        
        // Choice/Option columns for each language + scores
        string choicePrefix = useOptionInsteadOfChoice ? "Option" : "Choice";
        for (int c = 1; c <= 3; c++)
        {
            foreach (var lang in languages)
            {
                headers.Add($"{choicePrefix}{c}_{lang}");
            }
            headers.Add($"Faith{c}");
            headers.Add($"Trust{c}");
            headers.Add($"Hostility{c}");
        }
        
        return headers;
    }
    
    private void ExportDialoguesSheet(IWorkbook workbook, string sheetName, List<DialogueNode> dialogues,
        ICellStyle headerStyle, ICellStyle textStyle, ICellStyle numberStyle, string dialogueType, string mapTypeName)
    {
        ISheet sheet = workbook.CreateSheet(sheetName);
        
        // Create header
        IRow headerRow = sheet.CreateRow(0);
        List<string> headers = CreateHeaders();
        
        for (int i = 0; i < headers.Count; i++)
        {
            ICell cell = headerRow.CreateCell(i);
            cell.SetCellValue(headers[i]);
            cell.CellStyle = headerStyle;
        }
        
        // Export dialogues
        int rowIndex = 1;
        foreach (var dialogue in dialogues)
        {
            ExportDialogueRow(sheet, rowIndex++, dialogue, textStyle, numberStyle, dialogueType, mapTypeName);
        }
        
        AutoSizeColumns(sheet, headers.Count);
    }
    
    private void ExportDialogueRow(ISheet sheet, int rowIndex, DialogueNode dialogue, 
        ICellStyle textStyle, ICellStyle numberStyle, string dialogueType, string mapTypeName)
    {
        IRow row = sheet.CreateRow(rowIndex);
        int col = 0;
        
        // ID
        CreateCell(row, col++, dialogue.id ?? "", textStyle);
        
        // Type and MapType
        if (includeTypeAndMapType)
        {
            CreateCell(row, col++, dialogueType, textStyle);
            CreateCell(row, col++, mapTypeName, textStyle);
        }
        
        // Name for each language
        foreach (var lang in languages)
        {
            string name = GetLocalizedName(dialogue.id, lang, dialogue.name);
            CreateCell(row, col++, name, textStyle);
        }
        
        // Text for each language
        foreach (var lang in languages)
        {
            string text = GetLocalizedText(dialogue.id, lang, dialogue.text);
            CreateCell(row, col++, text, textStyle);
        }
        
        // Choices
        for (int c = 0; c < 3; c++)
        {
            // Choice text for each language
            foreach (var lang in languages)
            {
                string choiceText = "";
                if (dialogue.choices != null && c < dialogue.choices.Count)
                {
                    choiceText = GetLocalizedChoiceText(dialogue.id, lang, c, dialogue.choices[c].text);
                }
                CreateCell(row, col++, choiceText, textStyle);
            }
            
            // Scores (once per choice)
            if (dialogue.choices != null && c < dialogue.choices.Count)
            {
                var choice = dialogue.choices[c];
                CreateCell(row, col++, choice.faithChange.ToString(), numberStyle);
                CreateCell(row, col++, choice.trustChange.ToString(), numberStyle);
                CreateCell(row, col++, choice.hostilityChange.ToString(), numberStyle);
            }
            else
            {
                CreateCell(row, col++, "0", numberStyle);
                CreateCell(row, col++, "0", numberStyle);
                CreateCell(row, col++, "0", numberStyle);
            }
        }
    }
    
    private void ExportGlobalDialoguesSheet(IWorkbook workbook, string sheetName,
        ICellStyle headerStyle, ICellStyle textStyle, ICellStyle numberStyle)
    {
        ISheet sheet = workbook.CreateSheet(sheetName);
        
        List<string> headers = CreateHeaders();
        IRow headerRow = sheet.CreateRow(0);
        
        for (int i = 0; i < headers.Count; i++)
        {
            ICell cell = headerRow.CreateCell(i);
            cell.SetCellValue(headers[i]);
            cell.CellStyle = headerStyle;
        }
        
        int rowIndex = 1;
        foreach (var dialogue in dialogueDatabase.globalDialogueEffects)
        {
            ExportGlobalDialogueRow(sheet, rowIndex++, dialogue, textStyle, numberStyle);
        }
        
        AutoSizeColumns(sheet, headers.Count);
    }
    
    private void ExportGlobalDialogueRow(ISheet sheet, int rowIndex, GlobalDialogueNode dialogue,
        ICellStyle textStyle, ICellStyle numberStyle)
    {
        IRow row = sheet.CreateRow(rowIndex);
        int col = 0;
        
        // ID
        CreateCell(row, col++, dialogue.id ?? "", textStyle);
        
        // Type and MapType
        if (includeTypeAndMapType)
        {
            CreateCell(row, col++, "global", textStyle);
            CreateCell(row, col++, "", textStyle); // Global dialogues don't have a specific map
        }
        
        // Name for each language
        foreach (var lang in languages)
        {
            string name = GetLocalizedGlobalName(dialogue.id, lang, dialogue.name);
            CreateCell(row, col++, name, textStyle);
        }
        
        // Text for each language
        foreach (var lang in languages)
        {
            string text = GetLocalizedGlobalText(dialogue.id, lang, dialogue.text);
            CreateCell(row, col++, text, textStyle);
        }
        
        // Choices (global dialogues may have different structure)
        for (int c = 0; c < 3; c++)
        {
            foreach (var lang in languages)
            {
                string choiceText = "";
                if (dialogue.choices != null && c < dialogue.choices.Count)
                {
                    choiceText = GetLocalizedGlobalChoiceText(dialogue.id, lang, c, dialogue.choices[c].text);
                }
                CreateCell(row, col++, choiceText, textStyle);
            }
            
            // For global dialogues, scores might work differently
            // Using first country's effects as placeholder
            if (dialogue.choices != null && c < dialogue.choices.Count && 
                dialogue.choices[c].globalEffects != null && dialogue.choices[c].globalEffects.Count > 0)
            {
                var effect = dialogue.choices[c].globalEffects[0];
                CreateCell(row, col++, effect.faithChange.ToString(), numberStyle);
                CreateCell(row, col++, effect.trustChange.ToString(), numberStyle);
                CreateCell(row, col++, effect.hostilityChange.ToString(), numberStyle);
            }
            else
            {
                CreateCell(row, col++, "0", numberStyle);
                CreateCell(row, col++, "0", numberStyle);
                CreateCell(row, col++, "0", numberStyle);
            }
        }
    }
    
    // Localization helpers
    private string GetLocalizedName(string dialogueId, string lang, string defaultValue)
    {
        if (localizationData != null)
        {
            var loc = localizationData.GetLocalizedDialogue(dialogueId, GetFullLanguageName(lang));
            if (loc != null && !string.IsNullOrEmpty(loc.name)) return loc.name;
        }
        return lang == languages[0] ? defaultValue : "";
    }
    
    private string GetLocalizedText(string dialogueId, string lang, string defaultValue)
    {
        if (localizationData != null)
        {
            var loc = localizationData.GetLocalizedDialogue(dialogueId, GetFullLanguageName(lang));
            if (loc != null && !string.IsNullOrEmpty(loc.text)) return loc.text;
        }
        return lang == languages[0] ? defaultValue : "";
    }
    
    private string GetLocalizedChoiceText(string dialogueId, string lang, int choiceIndex, string defaultValue)
    {
        if (localizationData != null)
        {
            var loc = localizationData.GetLocalizedDialogue(dialogueId, GetFullLanguageName(lang));
            if (loc != null && loc.choiceTexts != null && choiceIndex < loc.choiceTexts.Count)
            {
                if (!string.IsNullOrEmpty(loc.choiceTexts[choiceIndex])) return loc.choiceTexts[choiceIndex];
            }
        }
        return lang == languages[0] ? defaultValue : "";
    }
    
    private string GetLocalizedGlobalName(string dialogueId, string lang, string defaultValue)
    {
        if (localizationData != null)
        {
            var loc = localizationData.GetLocalizedGlobalDialogue(dialogueId, GetFullLanguageName(lang));
            if (loc != null && !string.IsNullOrEmpty(loc.name)) return loc.name;
        }
        return lang == languages[0] ? defaultValue : "";
    }
    
    private string GetLocalizedGlobalText(string dialogueId, string lang, string defaultValue)
    {
        if (localizationData != null)
        {
            var loc = localizationData.GetLocalizedGlobalDialogue(dialogueId, GetFullLanguageName(lang));
            if (loc != null && !string.IsNullOrEmpty(loc.text)) return loc.text;
        }
        return lang == languages[0] ? defaultValue : "";
    }
    
    private string GetLocalizedGlobalChoiceText(string dialogueId, string lang, int choiceIndex, string defaultValue)
    {
        if (localizationData != null)
        {
            var loc = localizationData.GetLocalizedGlobalDialogue(dialogueId, GetFullLanguageName(lang));
            if (loc != null && loc.choiceTexts != null && choiceIndex < loc.choiceTexts.Count)
            {
                if (!string.IsNullOrEmpty(loc.choiceTexts[choiceIndex])) return loc.choiceTexts[choiceIndex];
            }
        }
        return lang == languages[0] ? defaultValue : "";
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
    
    // Helper methods
    private void CreateCell(IRow row, int col, string value, ICellStyle style)
    {
        ICell cell = row.CreateCell(col);
        cell.SetCellValue(value ?? "");
        cell.CellStyle = style;
    }
    
    private string SanitizeSheetName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "Sheet";
        char[] invalidChars = { '\\', '/', '*', '?', ':', '[', ']' };
        foreach (char c in invalidChars)
        {
            name = name.Replace(c, '-');
        }
        if (name.Length > 31) name = name.Substring(0, 31);
        return name;
    }
    
    private void AutoSizeColumns(ISheet sheet, int count)
    {
        for (int i = 0; i < count; i++)
        {
            sheet.AutoSizeColumn(i);
            if (sheet.GetColumnWidth(i) > 12000) sheet.SetColumnWidth(i, 12000);
            if (sheet.GetColumnWidth(i) < 2500) sheet.SetColumnWidth(i, 2500);
        }
    }
    
    private ICellStyle CreateHeaderStyle(IWorkbook workbook)
    {
        ICellStyle style = workbook.CreateCellStyle();
        IFont font = workbook.CreateFont();
        font.Boldweight = (short)FontBoldWeight.Bold;
        style.SetFont(font);
        style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightBlue.Index;
        style.FillPattern = FillPattern.SolidForeground;
        style.Alignment = HorizontalAlignment.Center;
        style.BorderTop = style.BorderBottom = style.BorderLeft = style.BorderRight = BorderStyle.Thin;
        return style;
    }
    
    private ICellStyle CreateTextStyle(IWorkbook workbook)
    {
        ICellStyle style = workbook.CreateCellStyle();
        style.WrapText = true;
        style.VerticalAlignment = VerticalAlignment.Top;
        style.BorderTop = style.BorderBottom = style.BorderLeft = style.BorderRight = BorderStyle.Thin;
        return style;
    }
    
    private ICellStyle CreateNumberStyle(IWorkbook workbook)
    {
        ICellStyle style = workbook.CreateCellStyle();
        style.Alignment = HorizontalAlignment.Center;
        style.BorderTop = style.BorderBottom = style.BorderLeft = style.BorderRight = BorderStyle.Thin;
        return style;
    }
}
