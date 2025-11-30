# Language Switcher Setup Guide

## Overview

The Language Switcher UI component allows players to switch between Turkish and English languages from the main menu (or any scene). The language preference is saved and persists across game sessions.

## Features

- ✅ Dropdown or button-based language selection
- ✅ Automatic language persistence (saved to PlayerPrefs)
- ✅ Visual feedback for current language
- ✅ Integrates with existing LocalizationManager
- ✅ Works in any scene

## Setup Instructions

### Step 1: Add LanguageSwitcherUI to Your Scene

1. **Open your main menu scene** (or the scene where you want the language switcher)

2. **Create a UI GameObject** for the language switcher:
   - Right-click in Hierarchy → UI → Canvas (if you don't have one)
   - Right-click on Canvas → UI → Panel (or create an empty GameObject)
   - Name it "LanguageSwitcher"

3. **Add the LanguageSwitcherUI Component**:
   - Select the "LanguageSwitcher" GameObject
   - Click "Add Component"
   - Search for "LanguageSwitcherUI" and add it

### Step 2: Choose Your UI Style

You can use either a **dropdown** or **buttons** for language selection.

#### Option A: Dropdown Style (Recommended)

1. **Create a Dropdown**:
   - Right-click on "LanguageSwitcher" → UI → Dropdown - TextMeshPro
   - Name it "LanguageDropdown"

2. **Configure the Component**:
   - Select "LanguageSwitcher" GameObject
   - In the LanguageSwitcherUI component:
     - ✅ Check "Use Dropdown"
     - Assign "LanguageDropdown" to the "Language Dropdown" field
     - Optionally assign a TextMeshProUGUI to "Current Language Text" to show current selection

3. **Position the Dropdown**:
   - Position it where you want (e.g., top-right corner of screen)
   - Adjust size and styling as needed

#### Option B: Button Style

1. **Create Buttons**:
   - Right-click on "LanguageSwitcher" → UI → Button - TextMeshPro
   - Create two buttons: "TurkishButton" and "EnglishButton"
   - Set their text to "Türkçe" and "English" respectively

2. **Configure the Component**:
   - Select "LanguageSwitcher" GameObject
   - In the LanguageSwitcherUI component:
     - ❌ Uncheck "Use Dropdown"
     - Assign "TurkishButton" to "Turkish Button"
     - Assign "EnglishButton" to "English Button"
     - Optionally assign a TextMeshProUGUI to "Current Language Text"

3. **Position the Buttons**:
   - Arrange buttons side by side or vertically
   - Style them as needed

### Step 3: Ensure LocalizationManager Exists

The LanguageSwitcher requires a LocalizationManager instance in the scene:

1. **Check if LocalizationManager exists**:
   - Look for a GameObject with "LocalizationManager" component in your scene
   - If it doesn't exist, create one:
     - Right-click in Hierarchy → Create Empty
     - Name it "LocalizationManager"
     - Add Component → "LocalizationManager"

2. **Verify LocalizationManager Setup**:
   - Make sure it has access to your dialogue JSON files in `Resources/NewDialogues/`
   - The LocalizationManager should persist across scenes (DontDestroyOnLoad)

### Step 4: Test the Language Switcher

1. **Enter Play Mode**
2. **Test Language Switching**:
   - Use the dropdown/buttons to switch languages
   - Verify that dialogues change language
   - Exit and restart the game to verify persistence

## Component Properties

### LanguageSwitcherUI Properties

| Property | Description |
|----------|-------------|
| **Language Dropdown** | TMP_Dropdown component for language selection (if using dropdown) |
| **Turkish Button** | Button component for Turkish language (if using buttons) |
| **English Button** | Button component for English language (if using buttons) |
| **Use Dropdown** | Toggle between dropdown and button mode |
| **Show Language Names** | Show localized names ("Türkçe"/"English") or enum names |
| **Current Language Text** | Optional TextMeshProUGUI to display current language |

## Example Scene Hierarchy

```
Canvas
└── LanguageSwitcher (GameObject)
    ├── LanguageSwitcherUI (Component)
    ├── LanguageDropdown (TMP_Dropdown) [if using dropdown]
    └── CurrentLanguageText (TextMeshProUGUI) [optional]
```

OR

```
Canvas
└── LanguageSwitcher (GameObject)
    ├── LanguageSwitcherUI (Component)
    ├── TurkishButton (Button)
    ├── EnglishButton (Button)
    └── CurrentLanguageText (TextMeshProUGUI) [optional]
```

## How It Works

1. **On Start**: The component loads the saved language preference from PlayerPrefs
2. **Language Selection**: When user selects a language, it:
   - Calls `LocalizationManager.Instance.SetLanguage()`
   - Saves the preference to PlayerPrefs
   - Updates UI to show current selection
3. **Persistence**: Language preference is saved and loaded automatically
4. **Events**: The component subscribes to `LocalizationManager.OnLanguageChanged` to update UI when language changes programmatically

## Customization

### Adding More Languages

To add more languages (e.g., Spanish):

1. **Update Language Enum** in `LocalizationManager.cs`:
```csharp
public enum Language
{
    Turkish = 0,
    English = 1,
    Spanish = 2,  // Add this
}
```

2. **Update LanguageSwitcherUI**:
   - Add more buttons or dropdown options
   - Update `GetLanguageDisplayName()` method to include new language names

3. **Update LocalizationManager**:
   - Add new language folder case in `GetLanguageFolder()` method

### Styling

- Customize button colors, fonts, and sizes in the Unity Inspector
- The component automatically highlights the active language button
- You can add icons or flags instead of text

## Troubleshooting

### Language Doesn't Change

- **Check LocalizationManager**: Ensure LocalizationManager exists in the scene
- **Check JSON Files**: Verify that language-specific JSON files exist in `Resources/NewDialogues/[language]/`
- **Check Console**: Look for error messages in the Unity Console

### Language Doesn't Persist

- **Check PlayerPrefs**: The preference is saved to `PlayerPrefs` with key "SelectedLanguage"
- **Verify Save**: Check that `PlayerPrefs.Save()` is being called (it is in the code)

### UI Not Showing

- **Check Canvas**: Ensure the LanguageSwitcher is a child of a Canvas
- **Check Active State**: Ensure the GameObject is active
- **Check Component**: Verify LanguageSwitcherUI component is enabled

## Integration with Main Menu

If you're adding this to a main menu scene:

1. **Position**: Place it in a visible location (e.g., top-right corner)
2. **Styling**: Match your main menu's visual style
3. **Accessibility**: Make sure it's easy to find and use

## Notes

- The language switcher works independently of the dialogue system
- Language changes take effect immediately for new dialogues
- Existing dialogues in memory may need to be refreshed (handled automatically by LocalizationManager)
- The component is designed to work in any scene, not just the main menu

