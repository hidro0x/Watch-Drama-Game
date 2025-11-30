using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

/// <summary>
/// Main Menu Animation Controller - Fades canvas groups sequentially using DOTween
/// </summary>
public class MainMenuAnimationController : MonoBehaviour
{
    [Header("Canvas Groups to Animate")]
    [Tooltip("Add canvas groups in the order you want them to fade in")]
    [SerializeField] private List<CanvasGroup> canvasGroups = new List<CanvasGroup>();
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float delayBetweenElements = 0.2f;
    [SerializeField] private float initialDelay = 0.3f;
    [SerializeField] private Ease fadeEase = Ease.OutQuad;
    
    [Header("Auto Play")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool playOnEnable = false;
    
    private Sequence animationSequence;
    private bool isAnimating = false;
    
    void Start()
    {
        // Initialize all canvas groups to invisible
        InitializeCanvasGroups();
        
        if (playOnStart)
        {
            PlayFadeInSequence();
        }
    }
    
    void OnEnable()
    {
        if (playOnEnable && !playOnStart)
        {
            InitializeCanvasGroups();
            PlayFadeInSequence();
        }
    }
    
    void OnDisable()
    {
        StopAnimation();
    }
    
    /// <summary>
    /// Initialize all canvas groups to be invisible
    /// </summary>
    private void InitializeCanvasGroups()
    {
        foreach (var canvasGroup in canvasGroups)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }
    
    /// <summary>
    /// Play the fade-in sequence for all canvas groups
    /// </summary>
    public void PlayFadeInSequence()
    {
        if (isAnimating) return;
        
        StopAnimation();
        isAnimating = true;
        
        animationSequence = DOTween.Sequence();
        
        // Initial delay
        if (initialDelay > 0)
        {
            animationSequence.AppendInterval(initialDelay);
        }
        
        // Fade in each canvas group sequentially
        for (int i = 0; i < canvasGroups.Count; i++)
        {
            var canvasGroup = canvasGroups[i];
            if (canvasGroup == null) continue;
            
            // Add delay between elements (except for first element)
            if (i > 0 && delayBetweenElements > 0)
            {
                animationSequence.AppendInterval(delayBetweenElements);
            }
            
            // Fade in this canvas group
            animationSequence.Append(
                canvasGroup.DOFade(1f, fadeDuration)
                    .SetEase(fadeEase)
                    .OnStart(() => {
                        // Enable interaction when fade starts
                        canvasGroup.interactable = true;
                        canvasGroup.blocksRaycasts = true;
                    })
            );
        }
        
        animationSequence.OnComplete(() => {
            isAnimating = false;
        });
        
        animationSequence.Play();
    }
    
    /// <summary>
    /// Play the fade-out sequence for all canvas groups (reverse order)
    /// </summary>
    public void PlayFadeOutSequence(System.Action onComplete = null)
    {
        if (isAnimating) return;
        
        StopAnimation();
        isAnimating = true;
        
        animationSequence = DOTween.Sequence();
        
        // Fade out each canvas group in reverse order
        for (int i = canvasGroups.Count - 1; i >= 0; i--)
        {
            var canvasGroup = canvasGroups[i];
            if (canvasGroup == null) continue;
            
            // Add delay between elements (except for first element)
            if (i < canvasGroups.Count - 1 && delayBetweenElements > 0)
            {
                animationSequence.AppendInterval(delayBetweenElements);
            }
            
            // Fade out this canvas group
            animationSequence.Append(
                canvasGroup.DOFade(0f, fadeDuration)
                    .SetEase(fadeEase)
                    .OnComplete(() => {
                        // Disable interaction when fade completes
                        canvasGroup.interactable = false;
                        canvasGroup.blocksRaycasts = false;
                    })
            );
        }
        
        animationSequence.OnComplete(() => {
            isAnimating = false;
            onComplete?.Invoke();
        });
        
        animationSequence.Play();
    }
    
    /// <summary>
    /// Fade in a specific canvas group by index
    /// </summary>
    public void FadeInElement(int index, System.Action onComplete = null)
    {
        if (index < 0 || index >= canvasGroups.Count) return;
        
        var canvasGroup = canvasGroups[index];
        if (canvasGroup == null) return;
        
        canvasGroup.DOFade(1f, fadeDuration)
            .SetEase(fadeEase)
            .OnStart(() => {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            })
            .OnComplete(() => onComplete?.Invoke());
    }
    
    /// <summary>
    /// Fade out a specific canvas group by index
    /// </summary>
    public void FadeOutElement(int index, System.Action onComplete = null)
    {
        if (index < 0 || index >= canvasGroups.Count) return;
        
        var canvasGroup = canvasGroups[index];
        if (canvasGroup == null) return;
        
        canvasGroup.DOFade(0f, fadeDuration)
            .SetEase(fadeEase)
            .OnComplete(() => {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                onComplete?.Invoke();
            });
    }
    
    /// <summary>
    /// Stop any running animation
    /// </summary>
    public void StopAnimation()
    {
        if (animationSequence != null && animationSequence.IsActive())
        {
            animationSequence.Kill();
        }
        isAnimating = false;
    }
    
    /// <summary>
    /// Reset all canvas groups to invisible
    /// </summary>
    public void ResetAllToInvisible()
    {
        StopAnimation();
        InitializeCanvasGroups();
    }
    
    /// <summary>
    /// Set all canvas groups to visible immediately
    /// </summary>
    public void SetAllVisible()
    {
        StopAnimation();
        foreach (var canvasGroup in canvasGroups)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
    }
    
    /// <summary>
    /// Check if animation is currently playing
    /// </summary>
    public bool IsAnimating => isAnimating;
    
    /// <summary>
    /// Get the number of canvas groups
    /// </summary>
    public int ElementCount => canvasGroups.Count;
}

