# Map Data System Setup Guide

## Overview
The Map Data System provides a centralized way to manage map information (names, icons, colors, descriptions) for the Map Completion Panel and other UI elements.

## 🎯 **What This System Provides:**

- **Map Names**: Custom display names for each map
- **Map Icons**: Unique sprites/icons for each map type
- **Map Colors**: Thematic colors for each map
- **Text Colors**: Appropriate text colors for readability
- **Map Descriptions**: Rich descriptions for each map
- **Centralized Data**: All map data in one place

## 📁 **Files Created:**

- `Assets/MapData.cs` - Contains MapData class and MapDataCollection ScriptableObject

## 🔧 **Setup Instructions:**

### 1. Create Map Data Collection
1. Right-click in your Project window
2. Go to `Create > Game Data > Map Data Collection`
3. Name it `MapDataCollection` (or your preferred name)

### 2. Initialize Default Data
1. Select your MapDataCollection asset
2. In the Inspector, click the **"Initialize Default Map Data"** button
3. This will populate all 5 maps with default data

### 3. Customize Map Data
For each map, you can customize:

#### **Map Information:**
- **Map Type**: Automatically set (Astrahil, Agnari, Solarya, Theon, Varnan)
- **Display Name**: Custom name shown in UI
- **Map Icon**: Sprite/icon for the map (drag from Project window)
- **Description**: Rich description text

#### **Visual Settings:**
- **Map Color**: Primary color theme for the map
- **Text Color**: Color for text elements (ensure readability)

### 4. Assign to MapCompletionPanelUI
1. Select your MapCompletionPanelUI GameObject in the scene
2. In the Inspector, find the **"Map Data"** section
3. Drag your MapDataCollection asset to the **"Map Data Collection"** field

## 🎨 **Default Map Themes:**

The system comes with pre-configured themes for each map:

### **Astrahil** - Mystical Blue
- Color: Light blue `(0.8, 0.9, 1.0)`
- Text: White
- Theme: Mystical realm with ancient magic

### **Agnari** - Noble Orange  
- Color: Light orange `(1.0, 0.9, 0.8)`
- Text: White
- Theme: Proud kingdom of honor and tradition

### **Solarya** - Radiant Gold
- Color: Light yellow `(1.0, 1.0, 0.8)`
- Text: Black (for contrast)
- Theme: Empire bathed in eternal golden light

### **Theon** - Scholarly Purple
- Color: Light purple `(0.9, 0.8, 1.0)`
- Text: White
- Theme: Nation of wisdom and knowledge

### **Varnan** - Shadowed Gray
- Color: Light gray `(0.7, 0.7, 0.7)`
- Text: White
- Theme: Shadowed lands of mysteries and secrets

## 🖼️ **Icon Setup:**

To add map icons:

1. **Prepare Your Icons**: Create or import 64x64 or 128x128 pixel sprites for each map
2. **Set Texture Type**: Ensure sprites are set to "Sprite (2D and UI)" in Import Settings
3. **Assign Icons**: Drag each sprite to the corresponding Map Icon field in MapDataCollection

## 🎮 **How It Works:**

### **Automatic Data Loading:**
When MapCompletionPanelUI shows a map completion:

1. **Gets Map Data**: Calls `mapDataCollection.GetMapData(mapType)`
2. **Sets Map Name**: Uses `mapData.displayName`
3. **Sets Map Icon**: Uses `mapData.mapIcon` (hides if null)
4. **Applies Colors**: Uses `mapData.mapColor` and `mapData.textColor`
5. **Background Tint**: Applies subtle background tint based on map color

### **Fallback System:**
If MapDataCollection is not assigned:
- Uses hardcoded fallback data
- Shows warning in console
- Still displays map completion with default styling

## 🔄 **Runtime Usage:**

### **Setting Map Data Collection:**
```csharp
// Get the MapCompletionPanelUI
MapCompletionPanelUI panel = FindFirstObjectByType<MapCompletionPanelUI>();

// Set a different map data collection
panel.SetMapDataCollection(myCustomMapDataCollection);
```

### **Getting Map Data:**
```csharp
// Get map data for a specific map
MapData astrahilData = mapDataCollection.GetMapData(MapType.Astrahil);
Debug.Log($"Astrahil's display name: {astrahilData.displayName}");
```

## 📋 **Testing:**

1. **Create Map Data Collection** and initialize default data
2. **Assign to MapCompletionPanelUI** in the scene
3. **Add some map icons** to the collection
4. **Test map completion** - should now show custom names, icons, and colors
5. **Verify fallback** - remove assignment to test fallback system

## 🛠️ **Customization Examples:**

### **Custom Map Names:**
```
Astrahil → "The Crystal Realm"
Agnari → "The Iron Kingdom"  
Solarya → "The Golden Empire"
Theon → "The Library Nation"
Varnan → "The Shadowlands"
```

### **Custom Colors:**
- **War Theme**: Red tones with white text
- **Peace Theme**: Green tones with white text
- **Magic Theme**: Purple tones with white text
- **Tech Theme**: Blue tones with white text

### **Custom Descriptions:**
Add rich lore descriptions for each map that can be used in tooltips, loading screens, or other UI elements.

## 🚀 **Benefits:**

- **Centralized Data**: All map information in one place
- **Easy Customization**: Change names, icons, colors without code changes
- **Consistent Theming**: Ensures visual consistency across the game
- **Extensible**: Easy to add new map types or data fields
- **Fallback Safe**: Always works even if data is missing
- **Runtime Flexible**: Can change map data during gameplay

This system makes it easy to create unique visual identities for each map while keeping all the data organized and maintainable!
