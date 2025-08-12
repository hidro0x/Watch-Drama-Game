using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class EndingPanelUI : MonoBehaviour
{
    [SerializeField] private Image endingImage;
    [SerializeField] private TextMeshProUGUI endingText;

    [SerializeField] private Button nextButton;

    private void Start()
    {
        nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    public void SetEnding(Sprite sprite, string text)
    {
        gameObject.SetActive(true);
        endingImage.sprite = sprite;
        endingText.text = text;
    }

    private void OnNextButtonClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
