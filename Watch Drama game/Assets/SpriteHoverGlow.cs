using UnityEngine;

public class SpriteHoverGlow : MonoBehaviour
{
    public Material selectionMaterial;
    private Material originalMaterial;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
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