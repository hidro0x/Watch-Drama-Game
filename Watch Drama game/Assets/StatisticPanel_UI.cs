using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StatisticPanel_UI : MonoBehaviour
{
    [SerializeField] BarSlot_UI prefab;
    private List<BarSlot_UI> barSlotList;

    [SerializeField] private RectTransform content;

    [SerializeField] private Button closeButton;
    [SerializeField] private Button openButton;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.4f;
    [SerializeField] private float screenOffsetMultiplier = 1.5f;

    private RectTransform rectTransform;

    private void OnEnable()
    {
        GameManager.OnChoiceMade += OnChoiceMadeHandler;
    }

    private void OnDisable()
    {
        GameManager.OnChoiceMade -= OnChoiceMadeHandler;
    }

    private void Start()
    {
        openButton.onClick.AddListener(OpenPanel);
        closeButton.onClick.AddListener(ClosePanel);

        rectTransform = GetComponent<RectTransform>();

        // Başlangıçta paneli ekranın sol dışında konumlandır
        float offX = Screen.width * screenOffsetMultiplier;
        rectTransform.anchoredPosition = new Vector2(-offX, rectTransform.anchoredPosition.y);

        // Slotları oluştur ve listeyi doldur
        barSlotList = new List<BarSlot_UI>();
        foreach (MapType mapType in Enum.GetValues(typeof(MapType)))
        {
            BarSlot_UI barSlotUI = Instantiate(prefab, content);
            barSlotUI.Initialize(mapType);
            barSlotList.Add(barSlotUI);
        }
    }

    private void OnChoiceMadeHandler(ChoiceEffect _)
    {
        if (gameObject.activeInHierarchy)
        {
            RefreshAll();
        }
    }

    private void RefreshAll()
    {
        if (barSlotList == null) return;
        foreach (BarSlot_UI barSlot in barSlotList)
        {
            if (barSlot != null) barSlot.Refresh();
        }
    }

    private void OpenPanel()
    {
        gameObject.SetActive(true);
        RefreshAll();

        // Animasyonla merkeze getir
        rectTransform.DOKill();
        rectTransform.DOAnchorPosX(0f, animationDuration).SetEase(Ease.OutQuad);
    }

    private void ClosePanel()
    {
        // Sola doğru dışarı taşı ve tamamlanınca paneli kapat
        float offX = Screen.width * screenOffsetMultiplier;
        rectTransform.DOKill();
        rectTransform
            .DOAnchorPosX(-offX, animationDuration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => { gameObject.SetActive(false); });
    }
}
