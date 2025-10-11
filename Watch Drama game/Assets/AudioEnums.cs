using UnityEngine;

/// <summary>
/// Enum for different types of sound effects that can be played
/// </summary>
public enum SoundEffectType
{
    ButtonClick,
    ButtonHover,
    DialogueNext,
    ChoiceSelect,
    Notification,
    Error,
    Success,
    Transition,
    Footstep,
    Ambient,
    UI_Open,
    UI_Close,
    UI_Confirm,
    UI_Cancel
}

/// <summary>
/// Enum for different background music tracks
/// </summary>
public enum BackgroundMusicType
{
    MainMenu,
    Gameplay,
    Dialogue,
    Tension,
    Victory,
    Defeat,
    Ambient
}

/// <summary>
/// Audio settings configuration
/// </summary>
[System.Serializable]
public class AudioSettings
{
    [Range(0f, 1f)]
    public float masterVolume = 1f;
    
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    
    [Range(0f, 1f)]
    public float musicVolume = 1f;
    
    public bool masterMuted = false;
    public bool sfxMuted = false;
    public bool musicMuted = false;
}
