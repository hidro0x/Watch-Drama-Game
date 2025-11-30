# Simple Runtime Localization Setup Guide

## Overview

The SimpleRuntimeLocalizer is a lightweight localization system that:
- ✅ Loads translations from JSON files
- ✅ Supports Excel export/import (via JSON)
- ✅ Automatically localizes UI text components
- ✅ Persists language preference
- ✅ Easy to use and lightweight

## Setup Steps

### 1. Create Localization Files

Create JSON files in `Assets/Resources/Localization/` folder:

**Turkish file:** `turkish_localization.json`
```json
{
  "ui_start": "Başla",
  "ui_language": "Dil",
  "ui_turkish": "Türkçe",
  "ui_english": "İngilizce",
  "ui_settings": "Ayarlar",
  "ui_exit": "Çıkış"
}
```

**English file:** `english_localization.json`
```json
{
  "ui_start": "Start",
  "ui_language": "Language",
  "ui_turkish": "Turkish",
  "ui_english": "English",
  "ui_settings": "Settings",
  "ui_exit": "Exit"
}
```

### 2. Setup SimpleRuntimeLocalizer

1. Create an empty GameObject in your Main Menu scene
2. Add `SimpleRuntimeLocalizer` component to it
3. Configure settings:
   - **Localization Folder**: `Localization` (matches Resources folder path)
   - **Default Language**: `Turkish`
   - **Supported Languages**: Add `Turkish` and `English`

### 3. Localize UI Text

**Option A: Using LocalizableText Component**

1. Select your Text or TextMeshProUGUI component
2. Add `LocalizableText` component
3. Set the **Localization Key** (e.g., `ui_start`)

**Option B: Manual Localization**

```csharp
string text = SimpleRuntimeLocalizer.Instance.GetLocalizedText("ui_start");
myTextComponent.text = text;
```

### 4. Setup Main Menu

1. Create your Main Menu scene
2. Add `MainMenuController` component to a GameObject
3. Assign UI elements:
   - **Start Button**: Your start button
   - **Language Switch Button**: Button to switch languages
   - **Language Switch Button Text**: Text component showing current language
   - **Intro Scene Name**: `LoadingScreen` (or your intro scene name)

### 5. Switch Languages

The language switch button automatically:
- Cycles through available languages
- Saves preference to PlayerPrefs
- Refreshes all localized texts
- Updates UI display

## Excel Export/Import (Optional)

### Export to Excel

You can use the NPOI Excel Exporter to create Excel files from JSON:

1. Create Excel file with columns: **Key**, **Turkish**, **English**
2. Fill in your translations
3. Export back to JSON format

### Import from Excel

Use any Excel-to-JSON converter or manually copy data to JSON files.

## Example Usage

### In MainMenuController

```csharp
// The MainMenuController automatically:
// - Loads saved language preference
// - Handles start button (loads intro scene)
// - Handles language switching
// - Integrates with SimpleRuntimeLocalizer
```

### Manual Language Switch

```csharp
// Change language programmatically
SimpleRuntimeLocalizer.Instance.SetLanguage("English");

// Get localized text
string startText = SimpleRuntimeLocalizer.Instance.GetLocalizedText("ui_start");
```

### Listen to Language Changes

```csharp
void OnEnable()
{
    SimpleRuntimeLocalizer.Instance.OnLanguageChanged += OnLanguageChanged;
}

void OnDisable()
{
    SimpleRuntimeLocalizer.Instance.OnLanguageChanged -= OnLanguageChanged;
}

private void OnLanguageChanged(string newLanguage)
{
    Debug.Log($"Language changed to: {newLanguage}");
    // Refresh your custom UI elements
}
```

## File Structure

```
Assets/
├── Resources/
│   └── Localization/
│       ├── turkish_localization.json
│       └── english_localization.json
├── MainMenuController.cs
└── SimpleRuntimeLocalizer.cs
```

## JSON Format

```json
{
  "key_name": "Translated Text",
  "another_key": "Another Translation"
}
```

**Rules:**
- Keys must be unique
- Values are plain strings
- Use lowercase with underscores for keys (e.g., `ui_start`, `dialog_intro`)
- UTF-8 encoding for special characters

## Tips

1. **Key Naming**: Use prefixes like `ui_`, `dialog_`, `error_` to organize keys
2. **Fallback**: If a key is missing in current language, it falls back to default language
3. **Missing Keys**: Missing keys will show the key name itself (with warning)
4. **Hot Reload**: Call `SimpleRuntimeLocalizer.Instance.ReloadLocalizationData()` to reload from Resources

## Integration with Audio Manager

The MainMenuController automatically plays button click sounds if AudioManager is available:

```csharp
// Automatic integration - no setup needed!
// Just make sure AudioManager.Instance exists
```

## Notes

- SimpleRuntimeLocalizer persists across scenes (DontDestroyOnLoad)
- Language preference is saved in PlayerPrefs
- All LocalizableText components automatically refresh on language change
- Works with both Text and TextMeshProUGUI components

