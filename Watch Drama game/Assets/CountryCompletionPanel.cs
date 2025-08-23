using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

public class CountryCompletionPanel : MonoBehaviour
{
    [Title("Canvas Referansları")]
    [SerializeField] private Canvas completionCanvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image countryIcon;
    [SerializeField] private TextMeshProUGUI countryNameText;

    
    [Title("Bar Değerleri")]
    [SerializeField] private Slider trustSlider;
    [SerializeField] private Slider faithSlider;
    [SerializeField] private Slider hostilitySlider;

    
    [Title("Buton")]
    [SerializeField] private Button completeButton;
    

    
    [Title("Animasyon Ayarları")]
    [SerializeField] private float panelShowDuration = 0.5f;
    [SerializeField] private float barAnimationDuration = 1f;
    [SerializeField] private float delayBetweenBars = 0.2f;
    
    private MapType currentCountry;
    private BarValues finalValues;
    private System.Action onCompleteCallback;
    
    private void Awake()
    {
        // Canvas'ı başlangıçta gizle
        if (completionCanvas != null)
        {
            completionCanvas.enabled = false;
        }
        else
        {
            Debug.LogError("CompletionCanvas null! Inspector'da atanmamış!");
        }
            
        // CanvasGroup'u başlangıçta gizle ve non-interaktif yap
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else
        {
            Debug.LogWarning("CanvasGroup null! Inspector'da atanmamış!");
        }
            
        // Buton listener'ı ekle
        if (completeButton != null)
        {
            completeButton.onClick.AddListener(OnCompleteButtonClicked);
        }
        else
        {
            Debug.LogWarning("CompleteButton null! Inspector'da atanmamış!");
        }
        

    }
    
    private void Start()
    {
        // MapManager'dan turn bitiş event'ini dinle
        if (MapManager.Instance != null)
        {
            // Event sistemi yoksa manuel kontrol edeceğiz
        }
    }
    
    public void ShowCountryCompletion(MapType country, BarValues finalBarValues, System.Action onComplete = null)
    {
        currentCountry = country;
        finalValues = finalBarValues;
        onCompleteCallback = onComplete;
        
        // UI'yi güncelle
        UpdateUI();
        
        // Panel'i göster ve animasyonu başlat
        ShowPanelWithAnimation();
    }
    
    private void UpdateUI()
    {
        // Ülke adını güncelle
        if (countryNameText != null)
            countryNameText.text = GetCountryDisplayName(currentCountry);
            
            
        // Ülke ikonunu güncelle (eğer varsa)
        if (countryIcon != null)
        {
            // Ülke ikonlarını yükle - bu kısmı daha sonra ekleyebiliriz
            // countryIcon.sprite = GetCountryIcon(currentCountry);
        }
        
        // Bar değerlerini güncelle
        UpdateBarValues();
        
    }
    
    private void UpdateBarValues()
    {
        // Bar değerlerini animasyonlu şekilde güncelle
        if (trustSlider != null)
        {
            trustSlider.value = 0f;
            trustSlider.DOValue(finalValues.trust / 100f, barAnimationDuration)
                .SetDelay(delayBetweenBars * 0f)
                .SetEase(Ease.OutQuad);
        }
        
        if (faithSlider != null)
        {
            faithSlider.value = 0f;
            faithSlider.DOValue(finalValues.faith / 100f, barAnimationDuration)
                .SetDelay(delayBetweenBars * 1f)
                .SetEase(Ease.OutQuad);
        }
        
        if (hostilitySlider != null)
        {
            hostilitySlider.value = 0f;
            hostilitySlider.DOValue(finalValues.hostility / 100f, barAnimationDuration)
                .SetDelay(delayBetweenBars * 2f)
                .SetEase(Ease.OutQuad);
        }
        

    }
    
    private void ShowPanelWithAnimation()
    {
        if (completionCanvas == null)
        {
            Debug.LogError("CompletionCanvas null!");
            return;
        }
        
        // Canvas'ı aktif et
        completionCanvas.enabled = true;
        
        // CanvasGroup fade animasyonu
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            // CanvasGroup'u interaktif yap (buton tıklanabilir olsun)
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.DOFade(1f, panelShowDuration).SetEase(Ease.OutQuad);
        }
        else
        {
            Debug.LogWarning("CanvasGroup null!");
        }
        
        // Scale animasyonu
        completionCanvas.transform.localScale = Vector3.zero;
        completionCanvas.transform.DOScale(Vector3.one, panelShowDuration)
            .SetEase(Ease.OutBack);
    }
    
    private void OnCompleteButtonClicked()
    {
        // Panel'i gizle
        HidePanelWithAnimation();
        
        // Callback'i çağır
        onCompleteCallback?.Invoke();
    }
    
    private void HidePanelWithAnimation()
    {
        if (completionCanvas == null) return;
        
        // CanvasGroup'u non-interaktif yap (buton tıklanamaz olsun)
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.DOFade(0f, panelShowDuration * 0.5f).SetEase(Ease.InQuad);
        }
        
        // Scale animasyonu
        completionCanvas.transform.DOScale(Vector3.zero, panelShowDuration * 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                completionCanvas.enabled = false;
            });
    }
    
    private string GetCountryDisplayName(MapType country)
    {
        switch (country)
        {
            case MapType.Astrahil: return "Astrahil";
            case MapType.Varnan: return "Varnan";
            case MapType.Solarya: return "Solarya";
            case MapType.Theon: return "Theon";
            case MapType.Agnari: return "Agnari";
            default: return country.ToString();
        }
    }
    
    // Manuel kontrol için public method
    public void CheckForCountryCompletion()
    {
        if (MapManager.Instance == null) return;
        
        var currentMap = MapManager.Instance.GetCurrentMap();
        if (currentMap == null) return;
        
        var mapTurns = MapManager.Instance.GetMapTurns();
        if (!mapTurns.ContainsKey(currentMap.Value)) return;
        
        // Eğer bu ülke için turn sayısı 0'a düştüyse
        if (mapTurns[currentMap.Value] <= 0)
        {
            // GameManager'dan final değerleri al
            var finalValues = GameManager.Instance.GetMapValues(currentMap.Value);
            ShowCountryCompletion(currentMap.Value, finalValues, () => {
                // Tamamlandığında yapılacak işlemler
                Debug.Log($"{currentMap.Value} ülkesi tamamlandı!");
            });
        }
    }
    
    private void OnTestButtonClicked()
    {
        // Test için manuel olarak panel'i göster
        var testValues = new BarValues { trust = 75, faith = 60, hostility = 25 };
        ShowCountryCompletion(MapType.Astrahil, testValues, () => {
            Debug.Log("Test panel'i tamamlandı!");
        });
    }
    
    private void OnDestroy()
    {
        if (completeButton != null)
            completeButton.onClick.RemoveListener(OnCompleteButtonClicked);
            
    }
}
