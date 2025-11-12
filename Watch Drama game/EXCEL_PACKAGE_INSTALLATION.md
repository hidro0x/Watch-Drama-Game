# Excel Package Installation Guide for Unity

## Option 1: EPPlus (May have compatibility issues)

EPPlus requires .NET Framework 4.7.2+ and may not work directly in Unity. However, you can try:

**Git URL (if available):**
```
https://github.com/EPPlusSoftware/EPPlus.git
```

**Note:** EPPlus might not compile in Unity due to .NET Standard compatibility issues.

## Option 2: ClosedXML (Recommended for Unity)

ClosedXML is more Unity-friendly and works with .NET Standard 2.0:

**Git URL:**
```
https://github.com/ClosedXML/ClosedXML.git
```

**Installation:**
1. Open `Packages/manifest.json`
2. Add this line in the `dependencies` section:
```json
"com.github.closedxml": "https://github.com/ClosedXML/ClosedXML.git?path=/src/ClosedXML"
```

## Option 3: ExcelDataReader (Read-only, but Unity-compatible)

For reading Excel files:
```
https://github.com/ExcelDataReader/ExcelDataReader.git
```

## Option 4: NPOI (Best Unity Compatibility)

NPOI works well with Unity and supports both reading and writing:

**Git URL:**
```
https://github.com/nissl-lab/npoi.git
```

**Installation:**
Add to `Packages/manifest.json`:
```json
"com.github.npoi": "https://github.com/nissl-lab/npoi.git?path=/src/NPOI"
```

## Recommended: Use NPOI

NPOI is the most Unity-compatible option. Here's how to add it:

1. **Edit `Packages/manifest.json`**
2. **Add this line:**
```json
"com.github.npoi": "https://github.com/nissl-lab/npoi.git?path=/src/NPOI"
```

3. **Wait for Unity to import the package**

## Alternative: Manual Installation

If Git URLs don't work, you can:

1. Download the library DLL files
2. Place them in `Assets/Plugins/` folder
3. Configure them in Unity

## Current Solution (No Package Needed)

The current CSV export solution works without any packages! You can:
- Export to CSV
- Open in Excel
- Apply styling manually
- Save as .XLSX

This preserves all styling and doesn't require any additional packages.

