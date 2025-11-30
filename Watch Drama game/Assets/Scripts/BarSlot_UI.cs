using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BarSlot_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countryNameText;
    [SerializeField] private Slider trustSlider;
    [SerializeField] private Slider faithSlider;
    [SerializeField] private Slider hostilitySlider;

    private MapType mapType;

    // Önceki değerleri takip etmek için
    private float previousTrust01;
    private float previousFaith01;
    private float previousHostility01;

    // Orijinal renkleri sakla
    private Color trustBaseColor;
    private Color faithBaseColor;
    private Color hostilityBaseColor;

    // Animasyon süreleri
    [Header("Animasyon Ayarları")]
    [SerializeField] private float valueAnimDuration = 0.8f;
    [SerializeField] private float colorFlashDuration = 1.2f;
    [SerializeField] private float scaleEffectDuration = 0.6f;
    [SerializeField] private float scaleEffectIntensity = 1.1f;

    // Yalnızca hostility barında artışta kırmızı, düşüşte yeşil göster
	[SerializeField] private bool invertHostilityColor = true;

    // Floating Text Settings
    [Header("Floating Text Settings")]
    [SerializeField] private bool enableFloatingText = true;
    [SerializeField] private GameObject trustFloatingText;
    [SerializeField] private GameObject faithFloatingText;
    [SerializeField] private GameObject hostilityFloatingText;
    [SerializeField] private float floatingTextDuration = 1.5f;
    [SerializeField] private float floatingTextDistance = 50f;
    [SerializeField] private float floatingTextStartOffset = 20f;


    public void Initialize(MapType mapType)
    {
        this.mapType = mapType;
        if (countryNameText != null)
        {
                   countryNameText.text = mapType.ToString();
        }
 

        // Başlangıç değerleri
        previousTrust01 = GameManager.Instance.GetBarValuesForCountry(mapType).Trust / 100f;
        previousFaith01 = GameManager.Instance.GetBarValuesForCountry(mapType).Faith / 100f;
        previousHostility01 = GameManager.Instance.GetBarValuesForCountry(mapType).Hostility / 100f;

        trustSlider.value = previousTrust01;
        faithSlider.value = previousFaith01;
        hostilitySlider.value = previousHostility01;

        // Base renkleri sakla (slider'ın fill'inden al)
        var trustFill = trustSlider != null && trustSlider.fillRect != null ? trustSlider.fillRect.GetComponent<Image>() : null;
        var faithFill = faithSlider != null && faithSlider.fillRect != null ? faithSlider.fillRect.GetComponent<Image>() : null;
        var hostilityFill = hostilitySlider != null && hostilitySlider.fillRect != null ? hostilitySlider.fillRect.GetComponent<Image>() : null;

        trustBaseColor = trustFill != null ? trustFill.color : Color.white;
        faithBaseColor = faithFill != null ? faithFill.color : Color.white;
        hostilityBaseColor = hostilityFill != null ? hostilityFill.color : Color.white;
    }

    public void Refresh()
    {
        float newTrust01 = GameManager.Instance.GetBarValuesForCountry(mapType).Trust / 100f;
        float newFaith01 = GameManager.Instance.GetBarValuesForCountry(mapType).Faith / 100f;
        float newHostility01 = GameManager.Instance.GetBarValuesForCountry(mapType).Hostility / 100f;

        // Değişimleri sırayla ve gecikmeli olarak güncelle
        StartCoroutine(RefreshWithDelay(newTrust01, newFaith01, newHostility01));
    }
    
    private System.Collections.IEnumerator RefreshWithDelay(float newTrust01, float newFaith01, float newHostility01)
    {
        // Kısa bir gecikme ile başla (değişimi daha belirgin yap)
        yield return new WaitForSeconds(0.1f);
        
        // Trust barını güncelle
        UpdateBarWithFeedback(trustSlider, ref previousTrust01, newTrust01, trustBaseColor);
        
        // Faith barı için kısa gecikme
        yield return new WaitForSeconds(0.15f);
        UpdateBarWithFeedback(faithSlider, ref previousFaith01, newFaith01, faithBaseColor);
        
        // Hostility barı için kısa gecikme
        yield return new WaitForSeconds(0.15f);
        UpdateBarWithFeedback(hostilitySlider, ref previousHostility01, newHostility01, hostilityBaseColor);
    }

	private void UpdateBarWithFeedback(Slider slider, ref float previousValue01, float newValue01, Color baseColor)
    {
        if (slider == null) return;

        const float EPSILON = 0.0001f;
        bool increased = newValue01 > previousValue01 + EPSILON;
        bool decreased = newValue01 < previousValue01 - EPSILON;
		bool isHostilityBar = slider == hostilitySlider;
		bool invertColors = isHostilityBar && invertHostilityColor;

        // Calculate the value change for floating text
        float valueChange = (newValue01 - previousValue01) * 100f; // Convert to actual values (0-100)
        if (Mathf.Abs(valueChange) > EPSILON * 100f && enableFloatingText)
        {
            GameObject floatingTextObj = null;
            if (slider == trustSlider) floatingTextObj = trustFloatingText;
            else if (slider == faithSlider) floatingTextObj = faithFloatingText;
            else if (slider == hostilitySlider) floatingTextObj = hostilityFloatingText;
            
            if (floatingTextObj != null)
            {
                ShowFloatingText(floatingTextObj, slider, valueChange, increased, invertColors);
            }
        }

        // Tweenleri tek bir Sequence altında senkronize et
        DOTween.Kill(slider); // aynı ID ile önceki sequence/tweenleri öldür
        var seq = DOTween.Sequence().SetId(slider);

		// Renk: animasyon boyunca highlight kalsın, bitince baz renge dönsün
		var fillImage = ResolveFillImage(slider);
		if (fillImage != null && (increased || decreased))
        {
			bool showGreen = (increased && !invertColors) || (decreased && invertColors);
			Color highlight = showGreen
				? new Color(0.2f, 1f, 0.2f, 1f)   // yeşil
				: new Color(1f, 0.25f, 0.25f, 1f); // kırmızı

            // Scale efekti için slider'ın transform'ını al
            Transform sliderTransform = slider.transform;
            Vector3 originalScale = sliderTransform.localScale;

            // 1. Scale efekti (hafif büyütme)
            seq.Append(sliderTransform.DOScale(originalScale * scaleEffectIntensity, scaleEffectDuration * 0.3f).SetEase(Ease.OutQuad));
            seq.Append(sliderTransform.DOScale(originalScale, scaleEffectDuration * 0.7f).SetEase(Ease.InQuad));
            
            // 2. Renk değişimi
            fillImage.color = highlight;
            
            // 3. Değer animasyonu (daha yavaş ve belirgin)
            seq.Join(slider.DOValue(newValue01, valueAnimDuration).SetEase(Ease.OutCubic));
            
            // 4. Renk geri dönüşü (daha uzun süre)
            seq.Append(fillImage.DOColor(baseColor, colorFlashDuration).SetEase(Ease.OutQuad));
            
            seq.Play();
        }
        else
        {
            // Sadece değer animasyonu (daha yavaş)
            slider.DOValue(newValue01, valueAnimDuration).SetEase(Ease.OutCubic);
        }

        previousValue01 = newValue01;
    }

	private Image ResolveFillImage(Slider slider)
	{
		if (slider == null) return null;
		// 1) Doğrudan fillRect üzerindeki Image
		if (slider.fillRect != null)
		{
			var img = slider.fillRect.GetComponent<Image>();
			if (img != null) return img;
		}

		// 2) Çocuklarda Image.Type == Filled olan bir Image ara (genelde Fill budur)
		var childImages = slider.GetComponentsInChildren<Image>(true);
		foreach (var ci in childImages)
		{
			if (ci.type == Image.Type.Filled) return ci;
		}

		// 3) İsim ipucusuna göre ("Fill") bir Image bul
		foreach (var ci in childImages)
		{
			if (ci.gameObject.name.ToLower().Contains("fill")) return ci;
		}

		// 4) Bulunamazsa null dön
		return null;
	}

    private void ShowFloatingText(GameObject floatingTextObj, Slider slider, float valueChange, bool increased, bool invertColors)
    {
        if (!enableFloatingText || floatingTextObj == null) return;

        // Get the text component from the reusable object
        var textComponent = floatingTextObj.GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            textComponent = floatingTextObj.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (textComponent == null)
        {
            Debug.LogWarning("Floating text object doesn't have a TextMeshProUGUI component!");
            return;
        }

        // Set the text content
        string sign = valueChange > 0 ? "+" : "";
        textComponent.text = sign + Mathf.RoundToInt(valueChange).ToString();

        // Set the text color
        bool showGreen = (increased && !invertColors) || (!increased && invertColors);
        Color textColor = showGreen 
            ? new Color(0.2f, 1f, 0.2f, 1f)   // Green for positive/increased
            : new Color(1f, 0.25f, 0.25f, 1f); // Red for negative/decreased
        textComponent.color = textColor;

        // Position the floating text above the slider
        RectTransform textRect = floatingTextObj.GetComponent<RectTransform>();
        if (textRect != null)
        {
            // Position above the slider with configurable offset
            Vector3 sliderPosition = slider.transform.position;
            Vector3 startPosition = sliderPosition + Vector3.up * floatingTextStartOffset;
            textRect.position = startPosition;
            
            // Ensure the object is visible
            floatingTextObj.SetActive(true);
            
            // Animate the floating text
            StartCoroutine(AnimateFloatingText(floatingTextObj, startPosition));
        }
    }

    private System.Collections.IEnumerator AnimateFloatingText(GameObject floatingTextObj, Vector3 startPosition)
    {
        RectTransform textRect = floatingTextObj.GetComponent<RectTransform>();
        var textComponent = floatingTextObj.GetComponent<TextMeshProUGUI>() ?? floatingTextObj.GetComponentInChildren<TextMeshProUGUI>();
        
        if (textRect == null || textComponent == null)
        {
            floatingTextObj.SetActive(false);
            yield break;
        }

        // Reset to initial state
        textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, 1f);
        Vector3 targetPosition = startPosition + Vector3.up * floatingTextDistance;

        // Create animation sequence
        var sequence = DOTween.Sequence();
        
        // Move upward
        sequence.Append(textRect.DOMove(targetPosition, floatingTextDuration).SetEase(Ease.OutQuad));
        
        // Fade out
        sequence.Join(textComponent.DOFade(0f, floatingTextDuration).SetEase(Ease.OutQuad));

        // Play the animation
        sequence.Play();

        // Wait for animation to complete
        yield return new WaitForSeconds(floatingTextDuration + 0.1f);

        // Hide the floating text object instead of destroying it
        floatingTextObj.SetActive(false);
    }
}
