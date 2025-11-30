using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Attach this to any UI Text or TextMeshProUGUI to make it localizable
/// Set the Key field to match a key in your localization JSON files
/// </summary>
[AddComponentMenu("UI/Localizable Text")]
public class LocalizableText : MonoBehaviour
{
    [Header("Localization")]
    [Tooltip("The key to look up in localization files (e.g., 'ui_start')")]
    [SerializeField] private string key;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    // Cached components
    private TextMeshProUGUI tmpText;
    private Text uiText;
    
    void Awake()
    {
        // Cache text components
        tmpText = GetComponent<TextMeshProUGUI>();
        uiText = GetComponent<Text>();
        
        if (tmpText == null && uiText == null)
        {
            Debug.LogWarning($"[LocalizableText] No Text or TMP component on '{gameObject.name}'");
        }
    }

    void Start()
    {
        // Register with localizer
        if (SimpleRuntimeLocalizer.Instance != null)
        {
            SimpleRuntimeLocalizer.Instance.Register(this);
        }
        
        // Initial update
        UpdateText();
    }

    void OnEnable()
    {
        // Update when enabled
        UpdateText();
    }

    void OnDestroy()
    {
        // Unregister
        if (SimpleRuntimeLocalizer.Instance != null)
        {
            SimpleRuntimeLocalizer.Instance.Unregister(this);
        }
    }

    /// <summary>
    /// Update the text with localized value
    /// </summary>
    public void UpdateText()
    {
        if (string.IsNullOrEmpty(key))
        {
            Debug.Log($"[LocalizableText] No key set on '{gameObject.name}'");
            return;
        }
        
        // Get localized text
        string localizedText = key; // Default to key
        
        if (SimpleRuntimeLocalizer.Instance != null)
        {
            localizedText = SimpleRuntimeLocalizer.Instance.GetText(key);
            Debug.Log($"[LocalizableText] '{gameObject.name}': key='{key}' -> result='{localizedText}'");
        }
        else
        {
            Debug.LogWarning($"[LocalizableText] No SimpleRuntimeLocalizer instance for '{gameObject.name}'!");
        }
        
        // Apply to text component
        if (tmpText != null)
        {
            string before = tmpText.text;
            tmpText.text = localizedText;
            Debug.Log($"[LocalizableText] TMP '{gameObject.name}': '{before}' -> '{localizedText}'");
        }
        else if (uiText != null)
        {
            string before = uiText.text;
            uiText.text = localizedText;
            Debug.Log($"[LocalizableText] Text '{gameObject.name}': '{before}' -> '{localizedText}'");
        }
        else
        {
            Debug.LogWarning($"[LocalizableText] No text component on '{gameObject.name}'!");
        }
    }

    /// <summary>
    /// Set the localization key at runtime
    /// </summary>
    public void SetKey(string newKey)
    {
        key = newKey;
        UpdateText();
    }

    /// <summary>
    /// Get the current key
    /// </summary>
    public string GetKey()
    {
        return key;
    }
}
