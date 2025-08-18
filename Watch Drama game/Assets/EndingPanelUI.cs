using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class EndingPanelUI : MonoBehaviour
{
    [Header("UI Bileşenleri")]
    [SerializeField] private Image endingImage;
    [SerializeField] private TextMeshProUGUI endingText;
    [SerializeField] private Button nextButton;
    
    [Header("Animasyon Ayarları")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private float buttonDelay = 0.5f;
    
    private string fullText;
    private bool isAnimating = false;

    private void Start()
    {
        nextButton.onClick.AddListener(OnNextButtonClicked);
        
        // CanvasGroup yoksa otomatik oluştur
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Başlangıçta paneli gizle
        canvasGroup.alpha = 0f;
        nextButton.gameObject.SetActive(false);
    }

    public void SetEnding(Sprite sprite, string text)
    {
        if (isAnimating) return; // Animasyon devam ediyorsa çık
        
        isAnimating = true;
        fullText = text;
        
        // Panel'i aktif et ve görseli ayarla
        gameObject.SetActive(true);
        endingImage.sprite = sprite;
        
        // Metni temizle
        endingText.text = "";
        
        // Butonu gizle
        nextButton.gameObject.SetActive(false);
        
        // Animasyonları başlat
        StartCoroutine(AnimateEnding());
    }

    private IEnumerator AnimateEnding()
    {
        // 1. Alpha fade-in animasyonu
        canvasGroup.DOFade(1f, fadeInDuration).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(fadeInDuration);
        
        // 2. Typewriter efekti
        yield return StartCoroutine(TypewriterEffect());
        
        // 3. Buton gecikmesi
        yield return new WaitForSeconds(buttonDelay);
        
        // 4. Butonu göster
        nextButton.gameObject.SetActive(true);
        nextButton.transform.localScale = Vector3.zero;
        nextButton.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        
        isAnimating = false;
    }
    
    private IEnumerator TypewriterEffect()
    {
        string currentText = "";
        
        for (int i = 0; i < fullText.Length; i++)
        {
            currentText += fullText[i];
            endingText.text = currentText;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
    
    private void OnNextButtonClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
