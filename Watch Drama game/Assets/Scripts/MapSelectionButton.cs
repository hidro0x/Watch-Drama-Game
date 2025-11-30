using UnityEngine;
using UnityEngine.UI;

public class MapSelectionButton : MonoBehaviour
{
    private Button button;
    [SerializeField]private MapType mapType;

    void Awake(){
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked(){
        // Play button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(SoundEffectType.ButtonClick);
        }
        
        MapManager.Instance.SelectMap(mapType);
    }
    
    public MapType GetMapType() => mapType;
    
    public void SetButtonInteractable(bool interactable)
    {
        button.interactable = interactable;
    }
}
