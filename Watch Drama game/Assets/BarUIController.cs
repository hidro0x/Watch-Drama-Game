using UnityEngine;
using UnityEngine.UI;

public class BarUIController : MonoBehaviour
{
    [Header("Barlar")]
    public Slider trustSlider;
    public Slider faithSlider;
    public Slider hostilitySlider;

    private void Start()
    {
        
        // Başlangıçta değerleri güncelle
        UpdateBars();
        // Evente abone ol
        GameManager.OnChoiceMade += OnChoiceMadeHandler;
    }

    private void OnDestroy()
    {
        GameManager.OnChoiceMade -= OnChoiceMadeHandler;
    }

    private void OnChoiceMadeHandler(ChoiceEffect effect)
    {
        UpdateBars();
    }

    public void UpdateBars()
    {
        if (trustSlider != null)
            trustSlider.value = GameManager.Instance.GetTrust()/100f;
        if (faithSlider != null)
            faithSlider.value = GameManager.Instance.GetFaith()/100f;
        if (hostilitySlider != null)
            hostilitySlider.value = GameManager.Instance.GetHostility()/100f;
    }
} 