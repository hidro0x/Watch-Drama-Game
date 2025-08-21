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
    private const float VALUE_ANIM_DURATION = 0.25f;
    [SerializeField] private float colorFlashDuration = 0.7f; // Renk geri dönüş süresi (daha belirgin görünüm için)

	// Yalnızca hostility barında artışta kırmızı, düşüşte yeşil göster
	[SerializeField] private bool invertHostilityColor = true;


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

        UpdateBarWithFeedback(trustSlider, ref previousTrust01, newTrust01, trustBaseColor);
        UpdateBarWithFeedback(faithSlider, ref previousFaith01, newFaith01, faithBaseColor);
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

            fillImage.color = highlight;
            // Değer animasyonu ile aynı anda çalışsın
            seq.Join(slider.DOValue(newValue01, VALUE_ANIM_DURATION).SetEase(Ease.OutQuad));
            // Değer animasyonu bittikten sonra renge geri dön
            seq.Append(fillImage.DOColor(baseColor, colorFlashDuration).SetEase(Ease.OutQuad));
            seq.Play();
        }
        else
        {
            // Sadece değer animasyonu
            slider.DOValue(newValue01, VALUE_ANIM_DURATION).SetEase(Ease.OutQuad);
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
}
