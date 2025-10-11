# GlobalDialogueUI Scene Setup Guide

## Overview
This guide explains how to set up the GlobalDialogueUI GameObject in your Unity scene to enable the special cinematic dialogue sequence for global dialogues.

## Required GameObject Hierarchy

Create the following hierarchy in your GameScene:

```
Canvas
└── GlobalDialogueUI (GameObject)
    ├── CanvasGroup (Component)
    ├── GlobalDialogueUI (Script Component)
    ├── BlackOverlay (Image)
    ├── FullscreenDialogueImage (Image)
    ├── DialogueText (TextMeshProUGUI)
    └── ChoicesPanel (RectTransform)
        ├── ChoiceButtonSlot1 (GameObject)
        │   ├── ChoiceButtonSlot (Script Component)
        │   └── ChoiceText (TextMeshProUGUI)
        ├── ChoiceButtonSlot2 (GameObject)
        │   ├── ChoiceButtonSlot (Script Component)
        │   └── ChoiceText (TextMeshProUGUI)
        └── ChoiceButtonSlot3 (GameObject)
            ├── ChoiceButtonSlot (Script Component)
            └── ChoiceText (TextMeshProUGUI)
```

## Component Setup Instructions

### 1. GlobalDialogueUI GameObject
- **Position**: Center of screen (0, 0, 0)
- **Scale**: (1, 1, 1)
- **Active**: Enabled (checked in inspector)
- **CanvasGroup**: Required component for smooth show/hide animations

### 2. BlackOverlay (Image Component)
- **Image Type**: Simple
- **Color**: Black (0, 0, 0, 1)
- **Anchor**: Stretch to fill entire screen
- **Position**: (0, 0, 0)
- **Size**: Full screen size
- **Raycast Target**: Checked (for click detection)

### 3. FullscreenDialogueImage (Image Component)
- **Image Type**: Simple
- **Color**: White (1, 1, 1, 1)
- **Anchor**: Stretch to fill entire screen
- **Position**: (0, 0, 0)
- **Size**: Full screen size
- **Raycast Target**: Unchecked
- **Note**: This will display the background sprite from GlobalDialogueNode data

### 4. DialogueText (TextMeshProUGUI Component)
- **Font**: Your preferred dialogue font
- **Font Size**: 24
- **Color**: White (1, 1, 1, 1)
- **Alignment**: Center
- **Anchor**: Center
- **Position**: (0, 0, 0)
- **Size**: (1000, 400)

### 5. ChoicesPanel (RectTransform)
- **Anchor**: Bottom stretch
- **Position**: (0, -100, 0)
- **Size**: Full width, height 300

### 6. ChoiceButtonSlot GameObjects
For each choice button slot (typically 3):
- **Image Component**: Your choice button background
- **ChoiceButtonSlot Script**: Attach the existing script
- **ChoiceText (TextMeshProUGUI)**: Child object with choice text
- **Layout Group**: Use Vertical Layout Group on ChoicesPanel for automatic positioning

## Script Component Assignments

In the GlobalDialogueUI script component, assign the following references:

### UI Components
- **Black Overlay**: Drag the BlackOverlay GameObject
- **Fullscreen Dialogue Image**: Drag the FullscreenDialogueImage GameObject
- **Dialogue Text**: Drag the DialogueText GameObject
- **Canvas Group**: Drag the CanvasGroup component (auto-assigned if missing)

### Choices Panel
- **Choices Panel**: Drag the ChoicesPanel GameObject
- **Choice Button Slots**: Drag all ChoiceButtonSlot GameObjects to the list

### Animation Settings (Optional)
- **Fade Out Duration**: 1.0 (seconds) - Black overlay fade out
- **Fade In Duration**: 0.8 (seconds) - General fade in duration
- **Image Fade In Duration**: 1.5 (seconds) - Dialogue image fade in duration
- **Typewriter Speed**: 0.03 (seconds per character)
- **Choices Panel Slide Duration**: 0.6 (seconds)

## Canvas Settings
Ensure your Canvas has the following settings:
- **Render Mode**: Screen Space - Overlay
- **UI Scale Mode**: Scale With Screen Size
- **Reference Resolution**: Your target resolution (e.g., 1920x1080)
- **Screen Match Mode**: Match Width Or Height
- **Match**: 0.5

## Setting Up Global Dialogue Data

To make sure global dialogues have background images:

1. **Open your Dialogue Database** (New Dialogue Database.asset)
2. **Go to Global Dialogue Effects** section
3. **For each GlobalDialogueNode**:
   - Set the **Sprite** field to your desired background image
   - This sprite will be used as the fullscreen background in GlobalDialogueUI
4. **Make sure sprites are assigned** - if no sprite is assigned, the background will be empty

## Testing
1. Play the scene
2. Wait for a global dialogue to trigger (every 16 turns by default)
3. Verify the cinematic sequence works:
   - Screen starts with black overlay at full alpha (black screen)
   - Fullscreen dialogue image is set from global dialogue data with alpha = 0 (invisible)
   - Black overlay fades out (revealing empty space)
   - Dialogue image slowly fades in from alpha 0 to 1
   - Dialogue text appears with typewriter effect
   - Click to skip typewriter (optional)
   - Choices panel slides up from bottom (over the visible image)
   - After choice selection, dialogue image fades out and transitions to map completion panel

## Troubleshooting

### GlobalDialogueUI Not Found
- Ensure the GlobalDialogueUI GameObject is in the scene
- Check that the GlobalDialogueUI script component is attached
- Verify the GameObject name matches exactly

### Choices Not Appearing
- Check that ChoicesPanel is assigned in the script
- Ensure ChoiceButtonSlot GameObjects are properly assigned
- Verify CanvasGroup is properly configured

### Animation Issues
- Ensure DOTween is imported and initialized
- Check that all UI elements have proper RectTransform components
- Verify anchor settings for proper positioning

### Map Completion Not Triggering
- Ensure MapCompletionPanelUI exists in the scene
- Check that GameManager has GetMapValues method
- Verify MapManager is properly initialized

## Notes
- The GlobalDialogueUI will only activate for dialogues with `isGlobalDialogue = true`
- Normal dialogues will continue using the existing ChoiceSelectionUI
- The system automatically detects and routes global dialogues to this new UI
- All existing functionality remains unchanged for non-global dialogues
- Uses CanvasGroup alpha for smooth show/hide animations instead of SetActive
- Both GlobalDialogueUI and MapCompletionPanelUI now use CanvasGroup for better performance
