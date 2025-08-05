using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class IntroController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image blackScreen;
    [SerializeField] private Image[] introImages;
    [SerializeField] private TextMeshProUGUI[] introTexts;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float textMoveDuration = 1.0f;
    [SerializeField] private float holdDuration = 1.0f;
    [SerializeField] private float textStartY = -500f;
    [SerializeField] private float textEndY = 0f;

    void Start()
    {
        if (blackScreen == null || introImages.Length == 0 || introTexts.Length == 0 || introImages.Length != introTexts.Length)
        {
            Debug.LogError("IntroController is not set up correctly. Please assign the black screen, and ensure the introImages and introTexts arrays are populated and have the same number of elements.");
            gameObject.SetActive(false);
            return;
        }

        PlayIntroSequence();
    }

    private void PlayIntroSequence()
    {
        // Initial setup: Start with a fully black screen.
        blackScreen.color = new Color(0, 0, 0, 1);

        // Prepare all text elements by making them transparent and setting their start position.
        foreach (var txt in introTexts)
        {
            txt.gameObject.SetActive(false);
            var tempColor = txt.color;
            tempColor.a = 0;
            txt.color = tempColor;
            txt.rectTransform.anchoredPosition = new Vector2(txt.rectTransform.anchoredPosition.x, textStartY);
        }
        
        // Hide all images initially.
        foreach (var img in introImages)
        {
            img.gameObject.SetActive(false);
        }

        Sequence introSequence = DOTween.Sequence();

        for (int i = 0; i < introImages.Length; i++)
        {
            int currentIndex = i;

            // Setup the slide just before it animates.
            introSequence.AppendCallback(() => {
                introImages[currentIndex].gameObject.SetActive(true);
                introTexts[currentIndex].gameObject.SetActive(true);

                if (currentIndex > 0)
                {
                    introImages[currentIndex - 1].gameObject.SetActive(false);
                    introTexts[currentIndex - 1].gameObject.SetActive(false);
                }
            });

            // Animate the fade-in of the scene and text.
            introSequence.Append(blackScreen.DOFade(0, fadeDuration));
            introSequence.Join(introTexts[currentIndex].rectTransform.DOAnchorPosY(textEndY, textMoveDuration));
            introSequence.Join(introTexts[currentIndex].DOFade(1, textMoveDuration));

            // Hold the scene.
            introSequence.AppendInterval(holdDuration);

            // Animate the fade-out of the scene and text.
            introSequence.Append(blackScreen.DOFade(1, fadeDuration));
            introSequence.Join(introTexts[currentIndex].rectTransform.DOAnchorPosY(textStartY, textMoveDuration));
            introSequence.Join(introTexts[currentIndex].DOFade(0, textMoveDuration));
        }

        // On completion, disable the intro GameObject.
        introSequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        introSequence.Play();
    }
}
