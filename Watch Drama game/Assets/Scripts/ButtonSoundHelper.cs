using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple helper script to add sound effects to buttons automatically
/// Attach this to any GameObject with a Button component
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSoundHelper : MonoBehaviour
{
    [Header("Sound Settings")]
    [Tooltip("The sound effect to play when button is clicked")]
    [SerializeField] private SoundEffectType clickSound = SoundEffectType.ButtonClick;

    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        
        if (button != null)
        {
            // Add the click sound to the button
            button.onClick.AddListener(PlayClickSound);
        }
    }

    /// <summary>
    /// Plays the click sound when button is clicked
    /// </summary>
    private void PlayClickSound()
    {
        AudioManager.Instance.PlaySFX(clickSound);
    }

    void OnDestroy()
    {
        // Clean up listener when object is destroyed
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }
}

