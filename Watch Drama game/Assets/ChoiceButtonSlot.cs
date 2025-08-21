using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ChoiceButtonSlot : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private TextMeshProUGUI choiceText;

    private object choice; // Hem DialogueChoice hem GlobalDialogueChoice için

	private RectTransform rectTransform;
	private Vector2 initialAnchoredPos;
	private Vector2 pointerDownLocalPos;
	[SerializeField] private float selectDragThreshold = 150f; // Minimum yerel birim (Canvas)
	[SerializeField] private float selectDragThresholdRatio = 0.12f; // Ebeveyn yüksekliğinin oranı
	[SerializeField] private float minDpiMmThreshold = 10f; // En az 10mm sürükleme (mobil için)
	private Canvas cachedCanvas;
    private LayoutElement layoutElement;
    private Vector3 originalScale;
    private bool isDragging;

    public void SetChoice(DialogueChoice choice)
    {
        this.choice = choice;
        SetChoiceText(choice.text);
    }
    


	void Awake()
    {
		choiceText = GetComponentInChildren<TextMeshProUGUI>();
		rectTransform = GetComponent<RectTransform>();
		initialAnchoredPos = rectTransform.anchoredPosition;
		// Drag ile seçim kullanılacağı için Button bileşenine ihtiyaç yok
		cachedCanvas = GetComponentInParent<Canvas>();
        layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = gameObject.AddComponent<LayoutElement>();
        }
        originalScale = rectTransform.localScale;
    }
    
    public void SetChoiceText(string text)
    {
        choiceText.text = text;
    }

	private void OnChoiceButtonClicked()
	{
		if (choice == null) return;
		
		if (choice is DialogueChoice dialogueChoice)
		{
			Debug.Log($"Dialogue choice clicked: {dialogueChoice.text}");
			
			// Global choice kontrolü
			if (dialogueChoice.isGlobalChoice)
			{
				Debug.Log("Global choice seçildi!");
				OnGlobalChoiceSelected(dialogueChoice);
				return;
			}

			// Normal diyalog: Sadece aktif ülkeyi etkiler
			ChoiceEffect effect = new ChoiceEffect(
				dialogueChoice.trustChange, 
				dialogueChoice.faithChange, 
				dialogueChoice.hostilityChange
			);
			
			// Eğer nextNodeId doluysa, ilgili DialogueNode'u sonraki adımda zorunlu göstermek için MapManager'a (ÖNCE) bildir
			DialogueNode preparedNext = null;
			if (!string.IsNullOrEmpty(dialogueChoice.nextNodeId))
			{
				var db = DialogueManager.Instance.dialogueDatabase;
				if (db != null && db.generalDialogues != null)
				{
					preparedNext = db.generalDialogues.Find(n => n.id == dialogueChoice.nextNodeId);
					if (preparedNext != null)
					{
						MapManager.Instance.PrepareForcedNextDialogue(preparedNext);
					}
				}
			}

			// Seçimi uygula (tek olay kaynağı)
			GameManager.MakeChoice(effect);
			return;
		}
	}

	public void ResetVisualState()
	{
		if (rectTransform == null) return;
		rectTransform.DOKill();
		rectTransform.localScale = originalScale;
		if (layoutElement != null) layoutElement.ignoreLayout = false;
		rectTransform.anchoredPosition = Vector2.zero;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (rectTransform == null) return;
		rectTransform.DOKill();
        // Layout kontrolünü bırak ki serbestçe hareket edebilelim
        if (layoutElement != null) layoutElement.ignoreLayout = true;
        // Görsel olarak öne getir
        transform.SetAsLastSibling();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			(RectTransform)rectTransform.parent,
			eventData.position,
			eventData.pressEventCamera,
			out pointerDownLocalPos
		);
		initialAnchoredPos = rectTransform.anchoredPosition;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
        isDragging = true;
        // Biraz büyüt
        rectTransform.DOKill();
        rectTransform.DOScale(originalScale * 1.08f, 0.12f).SetEase(Ease.OutQuad);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (rectTransform == null) return;
		Vector2 currentLocal;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
			(RectTransform)rectTransform.parent,
			eventData.position,
			eventData.pressEventCamera,
			out currentLocal))
		{
			var delta = currentLocal - pointerDownLocalPos;
			// Serbest hareket: hem X hem Y ekseninde
			rectTransform.anchoredPosition = initialAnchoredPos + delta;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (rectTransform == null)
			return;
		Vector2 currentLocal;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
			(RectTransform)rectTransform.parent,
			eventData.position,
			eventData.pressEventCamera,
			out currentLocal))
		{
			var delta = currentLocal - pointerDownLocalPos;
			float threshold = GetEffectiveThresholdUnits();
			if (delta.magnitude >= threshold)
			{
				// Hızlıca kaybol (scale -> 0), ardından seçimi tetikle
				rectTransform.DOKill();
				rectTransform.DOScale(0f, 0.12f).SetEase(Ease.InQuad).OnComplete(() =>
				{
					OnChoiceButtonClicked();
					if (layoutElement != null) layoutElement.ignoreLayout = false;
				});
			}
			else
			{
				// Geri dön
				rectTransform.DOKill();
				rectTransform.DOAnchorPos(initialAnchoredPos, 0.2f).SetEase(Ease.OutQuad);
				rectTransform.DOScale(originalScale, 0.12f).SetEase(Ease.OutQuad);
				if (layoutElement != null) layoutElement.ignoreLayout = false;
			}
		}
        isDragging = false;
	}

	private float GetEffectiveThresholdUnits()
	{
		float baseUnits = selectDragThreshold;
		float parentHeight = rectTransform.parent is RectTransform pr ? pr.rect.height : 0f;
		float ratioUnits = parentHeight > 0f ? parentHeight * selectDragThresholdRatio : 0f;
		float dpiUnits = 0f;
		float dpi = Screen.dpi;
		if (dpi > 0f)
		{
			float inches = minDpiMmThreshold / 25.4f;
			float pixels = inches * dpi;
			float scale = cachedCanvas != null ? cachedCanvas.scaleFactor : 1f;
			dpiUnits = pixels / Mathf.Max(0.01f, scale);
		}
		return Mathf.Max(baseUnits, ratioUnits, dpiUnits);
	}

    
    private void OnGlobalChoiceSelected(DialogueChoice globalChoice)
    {
        // Global choice seçimi sonrası işlemler
        Debug.Log($"Global choice seçildi: {globalChoice.text}");
        
        // Global effects'i uygula (şimdilik boş, çünkü DialogueChoice'ta global effects yok)
        // Bu kısım için GlobalDialogueChoice'tan bilgi almamız gerekiyor
        ApplyGlobalEffectsFromDialogueChoice(globalChoice);
        
        // 1. Diyalog ekranını kapat
        var choiceSelectionUI = UnityEngine.Object.FindFirstObjectByType<ChoiceSelectionUI>();
        if (choiceSelectionUI != null)
        {
            choiceSelectionUI.OnPanelClosed();
        }
        
        // 2. Aktif ülkenin butonunu pasif yap
        var currentMap = MapManager.Instance.GetCurrentMap();
        if (currentMap.HasValue)
        {
            DisableMapButton(currentMap.Value);
        }
        
        // 3. Event'i tetikle
        var choiceUI = UnityEngine.Object.FindFirstObjectByType<ChoiceSelectionUI>();
        if (choiceUI != null)
        {
            choiceUI.TriggerDialogueChoiceEvent();
        }
    }
    
    private void ApplyGlobalEffectsFromDialogueChoice(DialogueChoice globalChoice)
    {
        // Global diyalog referansını al
        var currentGlobalDialogue = MapManager.Instance.GetCurrentGlobalDialogue();
        if (currentGlobalDialogue == null)
        {
            Debug.LogWarning("Global diyalog referansı bulunamadı!");
            return;
        }
        
        // Seçilen choice'ın index'ini bul
        int choiceIndex = -1;
        for (int i = 0; i < currentGlobalDialogue.choices.Count; i++)
        {
            if (currentGlobalDialogue.choices[i].text == globalChoice.text)
            {
                choiceIndex = i;
                break;
            }
        }
        
        if (choiceIndex >= 0)
        {
            // Global effects'i uygula
            var originalGlobalChoice = currentGlobalDialogue.choices[choiceIndex];
            ApplyGlobalEffectsFromGlobalChoice(originalGlobalChoice);
        }
        else
        {
            Debug.LogWarning("Global choice bulunamadı!");
        }
    }
    
    private void ApplyGlobalEffectsFromGlobalChoice(GlobalDialogueChoice globalChoice)
    {
        var globalEffect = new GlobalDialogueEffect();
        
        if (globalChoice.globalEffects != null && globalChoice.globalEffects.Count > 0)
        {
            // Özel global effects varsa onları kullan
            globalEffect.countryEffects = new Dictionary<MapType, BarValues>();
            foreach (var countryEffect in globalChoice.globalEffects)
            {
                globalEffect.countryEffects[countryEffect.country] = new BarValues
                {
                    trust = countryEffect.trustChange,
                    faith = countryEffect.faithChange,
                    hostility = countryEffect.hostilityChange
                };
            }
        }
        else
        {
            // Global effects yoksa hiçbir şey yapma
            Debug.LogWarning("GlobalDialogueChoice'ta global effects tanımlanmamış!");
            return;
        }
        
        GameManager.Instance.ApplyGlobalDialogueEffect(globalEffect);
    }
    

    
    private void OnGlobalDialogueChoiceCompleted()
    {
        // 1. Diyalog ekranını kapat
        var choiceSelectionUI = UnityEngine.Object.FindFirstObjectByType<ChoiceSelectionUI>();
        if (choiceSelectionUI != null)
        {
            choiceSelectionUI.gameObject.SetActive(false);
        }
        
        // 2. Aktif ülkenin butonunu pasif yap
        var currentMap = MapManager.Instance.GetCurrentMap();
        if (currentMap.HasValue)
        {
            DisableMapButton(currentMap.Value);
        }
        
        // 3. Event'i tetikle
        var choiceUI = UnityEngine.Object.FindFirstObjectByType<ChoiceSelectionUI>();
        if (choiceUI != null)
        {
            choiceUI.TriggerDialogueChoiceEvent();
        }
    }
    
    private void DisableMapButton(MapType mapType)
    {
        // Tüm MapSelectionButton'ları bul
        var mapButtons = UnityEngine.Object.FindObjectsOfType<MapSelectionButton>();
        foreach (var mapButton in mapButtons)
        {
            if (mapButton.GetMapType() == mapType)
            {
                mapButton.SetButtonInteractable(false);
                Debug.Log($"Map butonu pasif yapıldı: {mapType}");
                break;
            }
        }
    }
    
}
