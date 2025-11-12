# Excel Package Installation

## Quick Install: NPOI (Recommended)

Add this to your `Packages/manifest.json` file:

```json
"com.github.npoi": "https://github.com/nissl-lab/npoi.git?path=/src/NPOI"
```

## Other Options:

### ClosedXML
```json
"com.github.closedxml": "https://github.com/ClosedXML/ClosedXML.git?path=/src/ClosedXML"
```

### EPPlus (May not work in Unity)
```json
"com.github.epplus": "https://github.com/EPPlusSoftware/EPPlus.git"
```

## Note:
The current CSV export solution works perfectly without any packages!
You can style the CSV in Excel and save as .XLSX to preserve formatting.

