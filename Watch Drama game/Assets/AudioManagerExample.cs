using UnityEngine;

/// <summary>
/// Example script showing how to use the AudioManager
/// </summary>
public class AudioManagerExample : MonoBehaviour
{
    [Header("Example Usage")]
    [SerializeField] private bool playExampleSounds = false;

    void Start()
    {
        // Example: Play background music when the scene starts
        AudioManager.Instance.PlayBackgroundMusic(BackgroundMusicType.MainMenu);
    }

    void Update()
    {
        if (playExampleSounds)
        {
            // Example: Play different sound effects with keyboard inputs
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                AudioManager.Instance.PlaySFX(SoundEffectType.ButtonClick);
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                AudioManager.Instance.PlaySFX(SoundEffectType.DialogueNext);
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                AudioManager.Instance.PlaySFX(SoundEffectType.ChoiceSelect);
            }
            
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                AudioManager.Instance.PlaySFX(SoundEffectType.Notification);
            }
            
            // Example: Change background music
            if (Input.GetKeyDown(KeyCode.M))
            {
                AudioManager.Instance.PlayBackgroundMusic(BackgroundMusicType.Gameplay);
            }
            
            // Example: Toggle mute
            if (Input.GetKeyDown(KeyCode.Space))
            {
                AudioManager.Instance.ToggleMasterMute();
            }
        }
    }

    // Example public methods that can be called from UI buttons
    public void PlayButtonClickSound()
    {
        AudioManager.Instance.PlaySFX(SoundEffectType.ButtonClick);
    }

    public void PlayDialogueSound()
    {
        AudioManager.Instance.PlaySFX(SoundEffectType.DialogueNext);
    }

    public void PlayChoiceSelectSound()
    {
        AudioManager.Instance.PlaySFX(SoundEffectType.ChoiceSelect);
    }

    public void ChangeToGameplayMusic()
    {
        AudioManager.Instance.PlayBackgroundMusic(BackgroundMusicType.Gameplay);
    }

    public void ChangeToDialogueMusic()
    {
        AudioManager.Instance.PlayBackgroundMusic(BackgroundMusicType.Dialogue);
    }

    public void ToggleMusicMute()
    {
        AudioManager.Instance.ToggleMusicMute();
    }

    public void ToggleMasterMute()
    {
        AudioManager.Instance.ToggleMasterMute();
    }
}
