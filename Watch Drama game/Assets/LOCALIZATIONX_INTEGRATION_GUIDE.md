# LocalizationX Lite Integration Guide

## Overview

LocalizationX Lite is a powerful localization tool that:
- ✅ **Exports/Imports from Excel** (using NPOI - already included!)
- ✅ **Supports Google Sheets** integration
- ✅ **Generates localization classes** automatically
- ✅ **Uses key-based system** (perfect for dialogue IDs!)
- ✅ **Works with Resources folder** (compatible with our system)

## Key Features

### 1. Excel/Google Sheets Integration
- Import translations directly from Excel files
- Export to Excel format
- Sync with Google Sheets (optional)
- **NPOI library included** - perfect for our Excel exporter!

### 2. Auto-Generated Classes
- Creates static classes with localization keys
- Type-safe localization IDs
- Easy to use: `LocalizationClass.Get("dialogue_id")`

### 3. Key-Based System
- Uses string IDs (like our dialogue IDs: "universal_01", "varnan_01")
- Perfect match for our dialogue system!

## How LocalizationX Works

### Step 1: Create Localization Sheet
1. Go to **Assets > Create > LocalizationX** menu
2. Create a new localization sheet
3. Set up columns: **ID**, **Turkish**, **English**, etc.

### Step 2: Export/Import Excel
- **Export**: Creates Excel file from Unity
- **Import**: Loads translations from Excel
- **Google Sheets**: Can sync with Google Sheets (requires setup)

### Step 3: Generated Classes
LocalizationX generates classes like:
```csharp
public static class DialogueLocalization
{
    public static string Get(string id) { ... }
    public static string Get("universal_01") { ... }
}
```

## Integration with Dialogue System

### Option 1: Use LocalizationX for All Dialogues

**Pros:**
- Excel/Google Sheets integration built-in
- Auto-generated classes
- Professional workflow

**Cons:**
- Need to migrate existing JSON files
- Different workflow than current system

### Option 2: Hybrid Approach (Recommended)

**Use LocalizationX for:**
- UI text (buttons, menus, labels)
- System messages
- Common strings

**Use Our System for:**
- Dialogue content (already working well)
- Dynamic dialogue loading
- Map-specific dialogues

### Option 3: Use NPOI from LocalizationX

Since LocalizationX includes NPOI, we can:
- ✅ Use NPOI for Excel export with styling
- ✅ Keep our existing dialogue system
- ✅ Get best of both worlds!

## Using NPOI for Excel Export

Since LocalizationX Lite includes NPOI.dll, we can now create proper Excel files!

**NPOI Location:**
```
Assets/LocalizationX/Editor/Plugins/NPOI.dll
```

**Benefits:**
- Real Excel format (.xlsx)
- Styling support (colors, fonts, borders)
- Column width, row height
- Cell protection
- All styling preserved!

## Next Steps

1. **Update Excel Exporter** to use NPOI (from LocalizationX)
2. **Create Excel export** with proper styling
3. **Keep dialogue system** as-is (it works great!)
4. **Optionally migrate** to LocalizationX for future dialogues

## Documentation

Full documentation: https://hnb-rabear.github.io/LocalizationXLite

## Menu Items

Look for LocalizationX menu items:
- **Assets > Create > LocalizationX** - Create localization sheets
- **Assets > Create > LocalizationX > Refine IDs** - Refine localization IDs

