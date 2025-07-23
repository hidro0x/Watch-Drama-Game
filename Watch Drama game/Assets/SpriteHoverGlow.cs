using UnityEngine;
using UnityEngine.UI;

public class SpriteHoverGlow : MonoBehaviour
{
    public Material selectionMaterial;
    private Material originalMaterial;
    private Image sr;

    void Start()
    {
        sr = GetComponent<Image>();
        originalMaterial = sr.material;
    }

    void OnMouseEnter()
    {
        if (selectionMaterial != null && sr != null)
            sr.material = selectionMaterial;
    }

    void OnMouseExit()
    {
        if (originalMaterial != null && sr != null)
            sr.material = originalMaterial;
    }
} 