using UnityEngine;
using UnityEngine.UI;

public class BarUIController : MonoBehaviour
{
    [Header("Barlar")]
    public BarSlot_UI bar;

    private void Start()
    {
        
        // Evente abone ol
        GameManager.OnChoiceMade += OnChoiceMadeHandler;
        MapManager.OnMapSelected += OnMapSelectedHandler;
    }

    private void OnDestroy()
    {
        GameManager.OnChoiceMade -= OnChoiceMadeHandler;
        MapManager.OnMapSelected -= OnMapSelectedHandler;
    }

    private void OnChoiceMadeHandler(ChoiceEffect effect)
    {
        bar.Refresh();
    }

    private void OnMapSelectedHandler(MapType mapType)
    {
        if (bar != null)
        {
            bar.Initialize(mapType);
        }
    }
    
    public void UpdateBars()
    {
        if (bar == null) return;
        var activeMap = MapManager.Instance != null ? MapManager.Instance.GetCurrentMap() : (MapType?)null;
        if (activeMap.HasValue)
        {
            bar.Initialize(activeMap.Value);
        }
        bar.Refresh();
    }

} 