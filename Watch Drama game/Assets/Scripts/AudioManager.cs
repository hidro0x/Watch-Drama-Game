using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Singleton AudioManager that persists across scenes and handles all audio playback
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region Singleton Pattern
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioManager>();
                if (_instance == null)
                {
                    GameObject audioManagerObject = new GameObject("AudioManager");
                    _instance = audioManagerObject.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
    }
    #endregion

    #region Audio Components
    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource ambientAudioSource;
    #endregion

    #region Audio Clips
    [Header("Sound Effects")]
    [Tooltip("Drag your sound effect audio clips here. Set the Sound Effect Type for each clip.")]
    [SerializeField] private List<SFXClipData> soundEffects = new List<SFXClipData>();
    
    [Header("Background Music")]
    [Tooltip("Drag your background music audio clips here. Set the Music Type for each clip.")]
    [SerializeField] private List<MusicClipData> backgroundMusic = new List<MusicClipData>();
    #endregion

    #region Settings
    [Header("Audio Settings")]
    [SerializeField] private AudioSettings audioSettings = new AudioSettings();
    
    [Header("Music Settings")]
    [SerializeField] private float musicFadeDuration = 1f;
    [SerializeField] private bool loopBackgroundMusic = true;
    [SerializeField] private bool autoPlayMusicOnStart = true;
    [SerializeField] private BackgroundMusicType defaultMusicType = BackgroundMusicType.MainMenu;
    #endregion

    #region Private Variables
    private Dictionary<SoundEffectType, AudioClip> sfxDictionary = new Dictionary<SoundEffectType, AudioClip>();
    private Dictionary<BackgroundMusicType, AudioClip> musicDictionary = new Dictionary<BackgroundMusicType, AudioClip>();
    private BackgroundMusicType currentMusicType = BackgroundMusicType.MainMenu;
    private bool isMusicFading = false;
    #endregion

    #region Unity Events
    void Awake()
    {
        // Singleton pattern implementation
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Ensure audio sources are properly configured
        ConfigureAudioSources();
        
        // Build audio dictionaries
        BuildAudioDictionaries();
        
        // Load saved settings
        LoadAudioSettings();
        
        // Auto-play background music if enabled
        if (autoPlayMusicOnStart)
        {
            PlayBackgroundMusic(defaultMusicType, false);
            Debug.Log($"[AudioManager] Auto-playing background music: {defaultMusicType}");
        }
    }
    #endregion

    #region Initialization
    private void InitializeAudioManager()
    {
        // Create audio sources if they don't exist
        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (musicAudioSource == null)
        {
            musicAudioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (ambientAudioSource == null)
        {
            ambientAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void ConfigureAudioSources()
    {
        // Configure SFX Audio Source
        sfxAudioSource.playOnAwake = false;
        sfxAudioSource.loop = false;
        sfxAudioSource.volume = audioSettings.sfxVolume;
        
        // Configure Music Audio Source
        musicAudioSource.playOnAwake = false;
        musicAudioSource.loop = loopBackgroundMusic;
        musicAudioSource.volume = audioSettings.musicVolume;
        
        // Configure Ambient Audio Source
        ambientAudioSource.playOnAwake = false;
        ambientAudioSource.loop = true;
        ambientAudioSource.volume = audioSettings.sfxVolume * 0.5f; // Ambient sounds are usually quieter
    }

    private void BuildAudioDictionaries()
    {
        // Build SFX dictionary
        sfxDictionary.Clear();
        foreach (var sfxData in soundEffects)
        {
            if (sfxData != null && sfxData.clip != null && !sfxDictionary.ContainsKey(sfxData.soundType))
            {
                sfxDictionary.Add(sfxData.soundType, sfxData.clip);
            }
        }
        
        // Build Music dictionary
        musicDictionary.Clear();
        foreach (var musicData in backgroundMusic)
        {
            if (musicData != null && musicData.clip != null && !musicDictionary.ContainsKey(musicData.musicType))
            {
                musicDictionary.Add(musicData.musicType, musicData.clip);
            }
        }
    }
    #endregion

    #region Public SFX Methods
    /// <summary>
    /// Play a one-shot sound effect
    /// </summary>
    /// <param name="soundType">The type of sound to play</param>
    /// <param name="volumeMultiplier">Volume multiplier (0-1)</param>
    public void PlaySFX(SoundEffectType soundType, float volumeMultiplier = 1f)
    {
        if (audioSettings.masterMuted || audioSettings.sfxMuted) return;
        
        if (sfxDictionary.ContainsKey(soundType))
        {
            float finalVolume = audioSettings.masterVolume * audioSettings.sfxVolume * volumeMultiplier;
            sfxAudioSource.PlayOneShot(sfxDictionary[soundType], finalVolume);
        }
        else
        {
            Debug.LogWarning($"Sound effect '{soundType}' not found in AudioManager!");
        }
    }

    /// <summary>
    /// Play a one-shot sound effect with pitch variation
    /// </summary>
    /// <param name="soundType">The type of sound to play</param>
    /// <param name="pitch">Pitch variation (0.5f to 2f recommended)</param>
    /// <param name="volumeMultiplier">Volume multiplier (0-1)</param>
    public void PlaySFX(SoundEffectType soundType, float pitch, float volumeMultiplier = 1f)
    {
        if (audioSettings.masterMuted || audioSettings.sfxMuted) return;
        
        if (sfxDictionary.ContainsKey(soundType))
        {
            float originalPitch = sfxAudioSource.pitch;
            sfxAudioSource.pitch = pitch;
            
            float finalVolume = audioSettings.masterVolume * audioSettings.sfxVolume * volumeMultiplier;
            sfxAudioSource.PlayOneShot(sfxDictionary[soundType], finalVolume);
            
            // Reset pitch after a short delay
            StartCoroutine(ResetPitchAfterDelay(originalPitch, 0.1f));
        }
        else
        {
            Debug.LogWarning($"Sound effect '{soundType}' not found in AudioManager!");
        }
    }
    #endregion

    #region Public Background Music Methods
    /// <summary>
    /// Play background music
    /// </summary>
    /// <param name="musicType">The type of music to play</param>
    /// <param name="fadeIn">Whether to fade in the music</param>
    public void PlayBackgroundMusic(BackgroundMusicType musicType, bool fadeIn = true)
    {
        if (audioSettings.masterMuted || audioSettings.musicMuted) return;
        
        if (musicDictionary.ContainsKey(musicType))
        {
            currentMusicType = musicType;
            
            if (fadeIn && musicAudioSource.isPlaying)
            {
                StartCoroutine(FadeToNewMusic(musicDictionary[musicType]));
            }
            else
            {
                musicAudioSource.clip = musicDictionary[musicType];
                musicAudioSource.Play();
            }
        }
        else
        {
            Debug.LogWarning($"Background music '{musicType}' not found in AudioManager!");
        }
    }

    /// <summary>
    /// Stop background music
    /// </summary>
    /// <param name="fadeOut">Whether to fade out the music</param>
    public void StopBackgroundMusic(bool fadeOut = true)
    {
        if (fadeOut)
        {
            StartCoroutine(FadeOutMusic());
        }
        else
        {
            musicAudioSource.Stop();
        }
    }

    /// <summary>
    /// Pause background music (can be resumed later)
    /// Use this when you want to temporarily stop music (e.g., during dialogue, cutscenes, pause menu)
    /// </summary>
    public void PauseBackgroundMusic()
    {
        if (musicAudioSource.isPlaying)
        {
            musicAudioSource.Pause();
        }
    }

    /// <summary>
    /// Resume background music after pausing
    /// </summary>
    public void ResumeBackgroundMusic()
    {
        if (!audioSettings.masterMuted && !audioSettings.musicMuted && musicAudioSource.clip != null)
        {
            musicAudioSource.UnPause();
        }
    }

    /// <summary>
    /// Check if background music is playing
    /// </summary>
    public bool IsBackgroundMusicPlaying()
    {
        return musicAudioSource.isPlaying;
    }

    /// <summary>
    /// Toggle pause/resume background music
    /// If music is playing, it will pause. If paused, it will resume.
    /// </summary>
    public void ToggleBackgroundMusic()
    {
        if (musicAudioSource.isPlaying)
        {
            PauseBackgroundMusic();
        }
        else if (musicAudioSource.clip != null)
        {
            ResumeBackgroundMusic();
        }
    }
    #endregion

    #region Public Settings Methods
    /// <summary>
    /// Set master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        audioSettings.masterVolume = Mathf.Clamp01(volume);
        UpdateAudioSourceVolumes();
    }

    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        audioSettings.sfxVolume = Mathf.Clamp01(volume);
        UpdateAudioSourceVolumes();
    }

    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        audioSettings.musicVolume = Mathf.Clamp01(volume);
        UpdateAudioSourceVolumes();
    }

    /// <summary>
    /// Toggle master mute
    /// </summary>
    public void ToggleMasterMute()
    {
        audioSettings.masterMuted = !audioSettings.masterMuted;
        UpdateAudioSourceVolumes();
        
        if (audioSettings.masterMuted)
        {
            PauseBackgroundMusic();
        }
        else if (!audioSettings.musicMuted)
        {
            ResumeBackgroundMusic();
        }
    }

    /// <summary>
    /// Toggle SFX mute
    /// </summary>
    public void ToggleSFXMute()
    {
        audioSettings.sfxMuted = !audioSettings.sfxMuted;
        UpdateAudioSourceVolumes();
    }

    /// <summary>
    /// Toggle music mute
    /// </summary>
    public void ToggleMusicMute()
    {
        audioSettings.musicMuted = !audioSettings.musicMuted;
        UpdateAudioSourceVolumes();
        
        if (audioSettings.musicMuted)
        {
            PauseBackgroundMusic();
        }
        else if (!audioSettings.masterMuted)
        {
            ResumeBackgroundMusic();
        }
    }

    /// <summary>
    /// Get current audio settings
    /// </summary>
    public AudioSettings GetAudioSettings()
    {
        return audioSettings;
    }

    /// <summary>
    /// Save audio settings to PlayerPrefs
    /// </summary>
    public void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", audioSettings.masterVolume);
        PlayerPrefs.SetFloat("SFXVolume", audioSettings.sfxVolume);
        PlayerPrefs.SetFloat("MusicVolume", audioSettings.musicVolume);
        PlayerPrefs.SetInt("MasterMuted", audioSettings.masterMuted ? 1 : 0);
        PlayerPrefs.SetInt("SFXMuted", audioSettings.sfxMuted ? 1 : 0);
        PlayerPrefs.SetInt("MusicMuted", audioSettings.musicMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
    #endregion

    #region Private Helper Methods
    private void UpdateAudioSourceVolumes()
    {
        float masterVol = audioSettings.masterMuted ? 0f : audioSettings.masterVolume;
        
        sfxAudioSource.volume = masterVol * (audioSettings.sfxMuted ? 0f : audioSettings.sfxVolume);
        musicAudioSource.volume = masterVol * (audioSettings.musicMuted ? 0f : audioSettings.musicVolume);
        ambientAudioSource.volume = masterVol * (audioSettings.sfxMuted ? 0f : audioSettings.sfxVolume) * 0.5f;
    }

    private void LoadAudioSettings()
    {
        audioSettings.masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        audioSettings.sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        audioSettings.musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        audioSettings.masterMuted = PlayerPrefs.GetInt("MasterMuted", 0) == 1;
        audioSettings.sfxMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
        audioSettings.musicMuted = PlayerPrefs.GetInt("MusicMuted", 0) == 1;
        
        UpdateAudioSourceVolumes();
    }

    private IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        isMusicFading = true;
        
        // Fade out current music
        float startVolume = musicAudioSource.volume;
        for (float t = 0; t < musicFadeDuration; t += Time.deltaTime)
        {
            musicAudioSource.volume = Mathf.Lerp(startVolume, 0, t / musicFadeDuration);
            yield return null;
        }
        
        // Change clip and fade in
        musicAudioSource.clip = newClip;
        musicAudioSource.Play();
        
        for (float t = 0; t < musicFadeDuration; t += Time.deltaTime)
        {
            musicAudioSource.volume = Mathf.Lerp(0, audioSettings.masterVolume * audioSettings.musicVolume, t / musicFadeDuration);
            yield return null;
        }
        
        musicAudioSource.volume = audioSettings.masterVolume * audioSettings.musicVolume;
        isMusicFading = false;
    }

    private IEnumerator FadeOutMusic()
    {
        isMusicFading = true;
        float startVolume = musicAudioSource.volume;
        
        for (float t = 0; t < musicFadeDuration; t += Time.deltaTime)
        {
            musicAudioSource.volume = Mathf.Lerp(startVolume, 0, t / musicFadeDuration);
            yield return null;
        }
        
        musicAudioSource.Stop();
        musicAudioSource.volume = audioSettings.masterVolume * audioSettings.musicVolume;
        isMusicFading = false;
    }

    private IEnumerator ResetPitchAfterDelay(float originalPitch, float delay)
    {
        yield return new WaitForSeconds(delay);
        sfxAudioSource.pitch = originalPitch;
    }
    #endregion

    #region Unity Editor Helpers
    #if UNITY_EDITOR
    [ContextMenu("Build Audio Dictionaries")]
    private void EditorBuildDictionaries()
    {
        BuildAudioDictionaries();
        Debug.Log("Audio dictionaries built successfully!");
    }
    #endif
    #endregion
}

/// <summary>
/// Data structure for sound effect clips
/// </summary>
[System.Serializable]
public class SFXClipData
{
    [Tooltip("The type of sound effect")]
    public SoundEffectType soundType;
    
    [Tooltip("The audio clip for this sound effect")]
    public AudioClip clip;
}

/// <summary>
/// Data structure for background music clips
/// </summary>
[System.Serializable]
public class MusicClipData
{
    [Tooltip("The type of background music")]
    public BackgroundMusicType musicType;
    
    [Tooltip("The audio clip for this background music")]
    public AudioClip clip;
}
