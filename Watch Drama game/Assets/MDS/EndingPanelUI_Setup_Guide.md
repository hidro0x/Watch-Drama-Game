# EndingPanelUI Setup Guide

## Overview
The EndingPanelUI has been completely reworked to provide a cinematic ending experience with multiple scenario support, smooth CanvasGroup animations, and GameManager integration.

## Features Added

### 🎬 **Cinematic Animations**
- **CanvasGroup Integration**: Smooth fade in/out animations
- **Typewriter Effect**: Character-by-character text display
- **Button Animations**: Staggered button reveals with scale animations
- **Scenario Button System**: Dynamic button creation for different endings

### 🎯 **Multiple Ending Scenarios**
- **Trust Victory**: Diplomatic success ending
- **Faith Victory**: Spiritual restoration ending
- **Hostility Victory**: Power through strength ending
- **Balanced Victory**: Perfect balance achievement
- **All Maps Completed**: Journey completion ending
- **Defeat Scenarios**: Trust, Faith, and Hostility defeat endings
- **Custom**: Flexible custom ending support

### 🎮 **GameManager Integration**
- **Scenario Processing**: Each ending applies specific effects to game values
- **Save System**: Completion data saved to PlayerPrefs
- **Event System**: `OnGameFinished` event for other systems to listen
- **Odin Inspector Buttons**: Easy testing from inspector

## Setup Instructions

### 1. **Scene Setup**

#### Create the EndingPanelUI GameObject:
```
EndingPanelUI (GameObject)
├── CanvasGroup (Component)
├── Image (Background - Component)
├── EndingImage (Image - Child)
├── EndingTitle (TextMeshPro - Child)
├── EndingText (TextMeshPro - Child)
├── ActionButtons (GameObject - Child)
│   ├── ContinueButton (Button)
│   ├── RestartButton (Button)
│   └── MainMenuButton (Button)
└── ScenarioButtonsParent (GameObject - Child)
    └── [Dynamic buttons created at runtime]
```

#### Required Components:
- **EndingPanelUI**: Main script
- **CanvasGroup**: For smooth show/hide animations
- **Image**: Background component (optional)
- **Button Components**: For all interactive elements
- **TextMeshPro**: For title and description text

### 2. **Script Configuration**

#### In the EndingPanelUI Inspector:

**UI Components:**
- `endingImage`: Reference to the background image
- `endingTitle`: Reference to the title text component
- `endingText`: Reference to the description text component
- `continueButton`: Reference to continue button
- `restartButton`: Reference to restart button
- `mainMenuButton`: Reference to main menu button

**Scenario Buttons:**
- `scenarioButtonsParent`: Transform where scenario buttons will be created
- `scenarioButtonPrefab`: Prefab for scenario buttons (must have Button + TextMeshPro)

**Animation Settings:**
- `fadeInDuration`: 1.5f (time for panel to fade in)
- `fadeOutDuration`: 0.8f (time for panel to fade out)
- `typewriterSpeed`: 0.03f (delay between characters)
- `buttonDelay`: 0.5f (delay before showing buttons)
- `scenarioButtonSpacing`: 10f (spacing between scenario buttons)

### 3. **Ending Data Configuration**

#### Default Ending Data:
The system comes with 5 pre-configured endings:

1. **Trust Victory** (Blue theme)
   - Title: "Trust Triumphs"
   - Description: Diplomatic success message
   - Colors: Blue background, white text

2. **Faith Victory** (Yellow theme)
   - Title: "Faith Restored"
   - Description: Spiritual restoration message
   - Colors: Yellow background, black text

3. **Hostility Victory** (Red theme)
   - Title: "Power Through Strength"
   - Description: Power achievement message
   - Colors: Red background, white text

4. **Balanced Victory** (Gray theme)
   - Title: "Perfect Balance"
   - Description: Balance achievement message
   - Colors: Gray background, white text

5. **All Maps Completed** (Green theme)
   - Title: "Journey Complete"
   - Description: Journey completion message
   - Colors: Green background, white text

#### Customizing Endings:
1. Click **"Create Default Ending Data"** button in inspector
2. Modify the `endingDataCollection` list
3. Add custom sprites, colors, and text
4. Create new `EndingScenario` enum values if needed

### 4. **GameManager Integration**

#### New GameManager Methods:
- `FinishGameWithScenario(EndingScenario scenario)`: Main completion method
- `GetLastCompletedScenario()`: Get last completed ending
- `HasGameBeenCompleted()`: Check if game was completed
- Odin Inspector buttons for testing each scenario

#### Event System:
```csharp
// Listen for game completion
GameManager.OnGameFinished += (scenario) => {
    Debug.Log($"Game completed with: {scenario}");
};
```

### 5. **Usage Examples**

#### Show Specific Ending:
```csharp
// Get EndingPanelUI reference
EndingPanelUI endingPanel = FindFirstObjectByType<EndingPanelUI>();

// Show specific ending
endingPanel.ShowEnding(EndingScenario.TrustVictory);

// Show custom ending
EndingData customEnding = new EndingData {
    scenario = EndingScenario.Custom,
    title = "Custom Victory",
    description = "Your custom ending message",
    backgroundColor = Color.magenta,
    textColor = Color.white
};
endingPanel.ShowEnding(customEnding);
```

#### Show Scenario Selection:
```csharp
// Show buttons for all available scenarios
endingPanel.ShowScenarioButtons();
```

#### Finish Game with Scenario:
```csharp
// Complete game with specific scenario
GameManager.Instance.FinishGameWithScenario(EndingScenario.FaithVictory);
```

### 6. **Testing**

#### Inspector Testing Buttons:
- **Test Trust Victory**: Show trust victory ending
- **Test Faith Victory**: Show faith victory ending
- **Test Hostility Victory**: Show hostility victory ending
- **Test All Maps Completed**: Show completion ending
- **Hide Panel**: Hide the ending panel

#### GameManager Testing Buttons:
- **Finish with Trust Victory**: Complete game with trust victory
- **Finish with Faith Victory**: Complete game with faith victory
- **Finish with Hostility Victory**: Complete game with hostility victory
- **Finish with Balanced Victory**: Complete game with balanced victory
- **Finish with All Maps Completed**: Complete game with all maps

### 7. **Animation Sequence**

#### Standard Ending Flow:
1. **Fade In** (1.5s): Panel fades in with background color
2. **Typewriter** (variable): Description text appears character by character
3. **Button Reveal** (0.5s delay): Action buttons appear with scale animation
4. **User Interaction**: Player clicks continue, restart, or main menu

#### Scenario Button Flow:
1. **Clear Existing**: Remove any existing scenario buttons
2. **Create Buttons**: Generate buttons for all available scenarios
3. **Animate In**: Buttons appear with staggered scale animations
4. **User Selection**: Player selects desired scenario

### 8. **Customization Options**

#### Visual Customization:
- **Background Colors**: Each scenario can have unique background colors
- **Text Colors**: Customizable title and description text colors
- **Images**: Optional background images for each scenario
- **Audio**: Optional music and sound effects

#### Animation Customization:
- **Fade Durations**: Adjustable fade in/out times
- **Typewriter Speed**: Character display timing
- **Button Delays**: Timing between button appearances
- **Button Spacing**: Distance between scenario buttons

## Troubleshooting

### Common Issues:

1. **Scenario Buttons Not Appearing**:
   - Check if `scenarioButtonPrefab` is assigned
   - Verify `scenarioButtonsParent` is set
   - Ensure prefab has Button and TextMeshPro components

2. **Animations Not Working**:
   - Verify CanvasGroup component is present
   - Check if DOTween is imported
   - Ensure UI references are properly assigned

3. **GameManager Integration Issues**:
   - Confirm GameManager.Instance exists
   - Check if EndingScenario enum is accessible
   - Verify method signatures match

4. **Text Not Displaying**:
   - Check TextMeshPro component references
   - Verify ending data collection is populated
   - Ensure text components are active

### Debug Information:
- All major operations include Debug.Log statements
- Use inspector buttons for testing
- Check console for error messages
- Verify all required components are assigned

## Performance Notes

- **CanvasGroup**: More efficient than GameObject.SetActive for UI
- **Dynamic Buttons**: Created/destroyed as needed to save memory
- **Animation Caching**: DOTween handles animation pooling automatically
- **Event Cleanup**: Proper event unsubscription prevents memory leaks

This system provides a robust, flexible, and visually appealing ending experience that integrates seamlessly with your existing game systems.
