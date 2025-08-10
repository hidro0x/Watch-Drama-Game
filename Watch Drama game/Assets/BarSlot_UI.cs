using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarSlot_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countryNameText;
    [SerializeField] private Slider trustSlider;
    [SerializeField] private Slider faithSlider;
    [SerializeField] private Slider hostilitySlider;

    private MapType mapType;


    public void Initialize(MapType mapType)
    {
        this.mapType = mapType;
        countryNameText.text = mapType.ToString();
        trustSlider.value = GameManager.Instance.GetBarValuesForCountry(mapType).Trust / 100f;
        faithSlider.value = GameManager.Instance.GetBarValuesForCountry(mapType).Faith / 100f;
        hostilitySlider.value = GameManager.Instance.GetBarValuesForCountry(mapType).Hostility / 100f;
    }

    public void Refresh()
    {
        trustSlider.value = GameManager.Instance.GetBarValuesForCountry(mapType).Trust / 100f;
        faithSlider.value = GameManager.Instance.GetBarValuesForCountry(mapType).Faith / 100f;
        hostilitySlider.value = GameManager.Instance.GetBarValuesForCountry(mapType).Hostility / 100f;
    }
}
