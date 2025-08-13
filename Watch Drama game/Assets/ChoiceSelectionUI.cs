using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;

public class ChoiceSelectionUI : MonoBehaviour
{
    [SerializeField] private Image choiceSelectionImage;
    [SerializeField] private List<ChoiceButtonSlot> choiceButtonSlotList = new List<ChoiceButtonSlot>();

    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [SerializeField] private GameObject barPanel;

    [SerializeField] private Image characterImage;
    [SerializeField] private Image backgroundImage;

    private Node dialogueNode;
    private DialogueNode pendingDialogueNode; // Gelecek diyalogu geçiş sırasında uygulamak için
    private bool isTransitioning = false; // Çıkış/giriş animasyonu sırasında kilit
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private RectTransform bottomPanel;

    [SerializeField] private RectTransform choicesPanel;

    public static event Action OnDialogueChoiceMade;

    BarUIController barUIController;

    // Animation settings
    private const float ANIMATION_DURATION = 0.5f;
    private const float SCREEN_OFFSET_MULTIPLIER = 1.5f;
    private const float TEXT_REVEAL_DURATION = 1.5f;
    private const float CHARACTER_NAME_DELAY = 0.3f;

    private bool hasBeenInitialized = false; // Panel'in daha önce açılıp açılmadığını takip etmek için

    // Typewriter kontrolü için coroutine referansları
    private Coroutine nameTypewriterCoroutine;
    private Coroutine descriptionTypewriterCoroutine;
    private void UpdateCharacterImageForNode(DialogueNode node)
    {
        if (characterImage == null) return;
        if (node != null && node.isGlobalDialogue)
        {
            // Global diyalogda karakter görselini göstermeyelim
            characterImage.enabled = false;
        }
        else
        {
            characterImage.sprite = node != null ? node.sprite : null;
            characterImage.enabled = (characterImage.sprite != null);
        }
    }


    void Start()
    {
        barUIController = barPanel.transform.parent.GetComponent<BarUIController>();
        // rectTransform ekranın sağ dışında başlasın
        float screenWidth = Screen.width * SCREEN_OFFSET_MULTIPLIER;
        rectTransform.anchoredPosition = new Vector2(screenWidth, 0);
        // bottomPanel ekranın üstünde başlasın (görünür)
        if (bottomPanel != null)
            bottomPanel.anchoredPosition = new Vector2(0, 0);
    }

    public void ShowUI(DialogueNode newDialogueNode)
    {
        bool isFirstTime = !hasBeenInitialized;
        
        gameObject.SetActive(true);
        // İlk açılışta içerik doğrudan set edilir (zaten ekran dışından giriyor)
        if (isFirstTime)
        {
            this.dialogueNode = newDialogueNode;
            choiceSelectionImage.sprite = newDialogueNode.sprite;
            UpdateCharacterImageForNode(newDialogueNode);
            barPanel.SetActive(true);
            if (barUIController != null)
            {
                var activeMap = MapManager.Instance != null ? MapManager.Instance.GetCurrentMap() : (MapType?)null;
                if (activeMap.HasValue)
                {
                    barUIController.bar.Initialize(activeMap.Value);
                }
                barUIController.bar.Refresh();
            }
            
            // Arkaplan seçimi
            ApplyBackgroundForDialogue(newDialogueNode);
        }
        else
        {
            // Sonraki içerik hazırda bekletilir, animasyon sırasında uygulanır
            pendingDialogueNode = newDialogueNode;
        }
        
        // Aktif typewriter'ları durdur ve metinleri sıfırla (skip + reset)
        StopTypewriterCoroutines();
        ResetTypewriterTexts();
        
        // Global diyalog kontrolü
        if (!isFirstTime && pendingDialogueNode != null && pendingDialogueNode.isGlobalDialogue)
        {
            Debug.Log("Global diyalog gösteriliyor...");
            // Global diyalog için özel işlemler burada yapılabilir
        }
        
        if (isFirstTime)
        {
            SetChoices(((DialogueNode)dialogueNode).choices);
        }
        
        if (isFirstTime)
        {
            // İlk kez açılıyorsa bottomPanel animasyonu ve entrance animasyonu
            hasBeenInitialized = true;
            if (bottomPanel != null)
            {
                float screenHeight = Screen.height * SCREEN_OFFSET_MULTIPLIER;
                bottomPanel.DOAnchorPos(new Vector2(0, -screenHeight), ANIMATION_DURATION).SetEase(Ease.InQuad);
            }
            AnimateEntrance();
        }
        else
        {
            // Panel zaten açıksa: Animasyon dışarı -> içerik güncelle -> içeri
            if (!isTransitioning)
            {
                AnimateChoicePanel();
            }
        }
    }

    private void ApplyBackgroundForDialogue(DialogueNode node)
    {
        if (backgroundImage == null) return;
        
        // Global diyaloglarda tek görsel kullanılacak: mevcut background'ı koru
        if (node.isGlobalDialogue)
        {
            return;
        }
        
        // Aktif map ve database
        var mapManager = MapManager.Instance;
        if (mapManager == null || mapManager.GetCurrentMap() == null) return;
        var mapType = mapManager.GetCurrentMap().Value;
        var db = mapManager.dialogueDatabase;
        if (db == null) return;
        
        // Özel ülke diyalogu: specialGeneralDialoguesByMap içinde olanlar -> node.backgroundSprite kullan
        bool isSpecialForMap = db.specialGeneralDialoguesByMap.ContainsKey(mapType) &&
                               db.specialGeneralDialoguesByMap[mapType] != null &&
                               db.specialGeneralDialoguesByMap[mapType].Contains(node);
        if (isSpecialForMap)
        {
            if (node.backgroundSprite != null)
            {
                backgroundImage.sprite = node.backgroundSprite;
            }
            // node'da arkaplan yoksa mevcut background korunur
            return;
        }
        
        // Genel diyalog: map'e özel arkaplan kullan (mapSpecificDialogueBackgrounds)
        if (db.mapSpecificDialogueBackgrounds != null && db.mapSpecificDialogueBackgrounds.TryGetValue(mapType, out var mapBg) && mapBg != null)
        {
            backgroundImage.sprite = mapBg;
        }
    }

    private void AnimateEntrance()
    {
        // Calculate screen dimensions with offset
        float screenWidth = Screen.width * SCREEN_OFFSET_MULTIPLIER;
        float screenHeight = Screen.height * SCREEN_OFFSET_MULTIPLIER;
        
        // Set initial positions
        rectTransform.anchoredPosition = new Vector2(-screenWidth, 0);
        choicesPanel.anchoredPosition = new Vector2(0, -screenHeight);
        characterImage.rectTransform.anchoredPosition = new Vector2(screenWidth *1.5f, 0);
        
        // Clear text initially
        characterNameText.text = "";
        descriptionText.text = "";
        
        // 1. Main panel slides in from left
        rectTransform.DOAnchorPos(new Vector2(0, 0), ANIMATION_DURATION).SetEase(Ease.OutQuad)
            .OnComplete(() => {
                // 2. Choices panel slides up from bottom
                choicesPanel.DOAnchorPos(new Vector2(0, 0), ANIMATION_DURATION).SetEase(Ease.OutQuad)
                    .OnComplete(() => {
                        // 3. Character image slides from right to center
        if (dialogueNode is DialogueNode dn && dn.isGlobalDialogue)
        {
            // Global diyalogda karakter görselini içeri getirmeden metinleri başlat
            AnimateTextReveal();
        }
        else
        {
            characterImage.rectTransform.DOAnchorPos(new Vector2(0, 0), ANIMATION_DURATION).SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    // 4. Start text reveal animations
                    AnimateTextReveal();
                });
        }
                    });
            });
    }

    private void AnimateTextReveal()
    {
        // Yeni diyalog başlamadan önce: stop + reset
        StopTypewriterCoroutines();
        ResetTypewriterTexts();
        
        // Typewriter başlat (name ve description)
        nameTypewriterCoroutine = StartCoroutine(TypewriterEffect(characterNameText, dialogueNode.name, TEXT_REVEAL_DURATION * 0.6f, CHARACTER_NAME_DELAY));
        descriptionTypewriterCoroutine = StartCoroutine(TypewriterEffect(descriptionText, dialogueNode.text, TEXT_REVEAL_DURATION, CHARACTER_NAME_DELAY + 0.2f));
    }

    private void StopTypewriterCoroutines()
    {
        if (nameTypewriterCoroutine != null)
        {
            StopCoroutine(nameTypewriterCoroutine);
            nameTypewriterCoroutine = null;
        }
        if (descriptionTypewriterCoroutine != null)
        {
            StopCoroutine(descriptionTypewriterCoroutine);
            descriptionTypewriterCoroutine = null;
        }
    }

    private void ResetTypewriterTexts()
    {
        if (characterNameText != null) characterNameText.text = "";
        if (descriptionText != null) descriptionText.text = "";
    }

    private IEnumerator TypewriterEffect(TextMeshProUGUI textComponent, string fullText, float duration, float delay)
    {
        // Wait for delay
        yield return new WaitForSeconds(delay);
        
        if (textComponent == null)
            yield break;
        
        if (string.IsNullOrEmpty(fullText))
        {
            textComponent.text = fullText ?? "";
            yield break;
        }
        
        textComponent.text = "";
        int totalCharacters = fullText.Length;
        float timePerCharacter = duration / totalCharacters;
        
        for (int i = 0; i <= totalCharacters; i++)
        {
            textComponent.text = fullText.Substring(0, i);
            yield return new WaitForSeconds(timePerCharacter);
        }
    }

    public void AnimateChoicePanel()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        // Yeni panele geçmeden önce: stop + reset
        StopTypewriterCoroutines();
        ResetTypewriterTexts();
        
        // 1. Choices panel slides down
        float screenHeight = Screen.height * SCREEN_OFFSET_MULTIPLIER;
        
        choicesPanel.DOAnchorPos(new Vector2(0, -screenHeight), ANIMATION_DURATION).SetEase(Ease.InQuad)
            .OnComplete(() => {
                // 2. Character image slides out to right
                float screenWidth = Screen.width * SCREEN_OFFSET_MULTIPLIER;
                characterImage.rectTransform.DOAnchorPos(new Vector2(screenWidth, 0), ANIMATION_DURATION).SetEase(Ease.InQuad)
                    .OnComplete(() => {
                        // İçerik değişimini EKRAN DIŞINDAYKEN yap
                        if (pendingDialogueNode != null)
                        {
                            this.dialogueNode = pendingDialogueNode;
                            pendingDialogueNode = null;

                            // Sprite ve görünürlük
                            choiceSelectionImage.sprite = ((DialogueNode)dialogueNode).sprite;
                            UpdateCharacterImageForNode((DialogueNode)dialogueNode);

                            // Arkaplan ve barlar
                            ApplyBackgroundForDialogue((DialogueNode)dialogueNode);
                            if (barUIController != null)
                            {
                                var activeMap = MapManager.Instance != null ? MapManager.Instance.GetCurrentMap() : (MapType?)null;
                                if (activeMap.HasValue)
                                {
                                    barUIController.bar.Initialize(activeMap.Value);
                                }
                                barUIController.bar.Refresh();
                            }

                            // Yeni seçimleri set et
                            if (dialogueNode is DialogueNode dn)
                            {
                                SetChoices(dn.choices);
                            }
                        }

                        // 3. Character image teleports to left and slides back to center (global değilse)
                        if (dialogueNode is DialogueNode dn2 && dn2.isGlobalDialogue)
                        {
                            // Görseli içeri getirmeden metinleri başlat ve paneli yukarı çıkar
                            AnimateTextReveal();
                            choicesPanel.DOAnchorPos(new Vector2(0, 0), ANIMATION_DURATION).SetEase(Ease.OutQuad);
                            isTransitioning = false;
                        }
                        else
                        {
                            characterImage.rectTransform.anchoredPosition = new Vector2(-screenWidth, 0);
                            characterImage.rectTransform.DOAnchorPos(new Vector2(0, 0), ANIMATION_DURATION).SetEase(Ease.OutQuad)
                                .OnComplete(() => {
                                    // 4. Choices panel slides back up
                                    // Typewriter efektini choices panel yukarı çıkarken eşzamanlı başlat
                                    AnimateTextReveal();
                                    choicesPanel.DOAnchorPos(new Vector2(0, 0), ANIMATION_DURATION).SetEase(Ease.OutQuad);
                                    isTransitioning = false;
                                });
                        }
                    });
            });
    }

    private void SetChoices(List<DialogueChoice> choices)
    {
        for (int i = 0; i < choices.Count; i++)
        {
            choiceButtonSlotList[i].SetChoice(choices[i]);
        }
    }

    public void OnPanelClosed()
    {
        // Typewriter'ları durdur
        StopTypewriterCoroutines();
        
        // Animate exit to left
        float screenWidth = Screen.width * SCREEN_OFFSET_MULTIPLIER;
        
        rectTransform.DOAnchorPos(new Vector2(-screenWidth, 0), ANIMATION_DURATION).SetEase(Ease.InQuad)
            .OnComplete(() => {
                gameObject.SetActive(false);
                barPanel.SetActive(false);
                hasBeenInitialized = false; // Panel kapatıldığında flag'i sıfırla
            });
        // bottomPanel tekrar yukarı çıksın
        if (bottomPanel != null)
        {
            bottomPanel.DOAnchorPos(new Vector2(0, 0), ANIMATION_DURATION).SetEase(Ease.OutQuad);
        }
    }

    public void TriggerDialogueChoiceEvent()
    {
        OnDialogueChoiceMade?.Invoke();
        // Choice selection sonrası text'leri temizle
        if (characterNameText != null) characterNameText.text = "";
        if (descriptionText != null) descriptionText.text = "";
    }
}
