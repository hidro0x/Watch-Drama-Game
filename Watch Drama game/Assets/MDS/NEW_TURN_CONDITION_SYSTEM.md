# Yeni Turn Condition Sistemi

## 📋 Genel Bakış

Bu sistem her turn sonunda otomatik olarak belirli koşulları kontrol eder ve gerekli diyalogları tetikler.

## 🎯 Kontrol Edilen Durumlar

### 1. Trust 0'a Düşme
- **Koşul**: Herhangi bir ülkenin Trust değeri 0'a düşerse
- **Aksiyon**: `trustZeroDialogue` diyalogu tetiklenir
- **Öncelik**: İlk bulunan ülke için tetiklenir

### 2. Faith 0'a Düşme
- **Koşul**: Herhangi bir ülkenin Faith değeri 0'a düşerse
- **Aksiyon**: `faithZeroDialogue` diyalogu tetiklenir
- **Öncelik**: İlk bulunan ülke için tetiklenir

### 3. Hostility 100'e Çıkma
- **Koşul**: Herhangi bir ülkenin Hostility değeri 100'e çıkarsa
- **Aksiyon**: `hostilityMaxDialogue` diyalogu tetiklenir
- **Öncelik**: İlk bulunan ülke için tetiklenir

### 4. Maximum Turn Ulaşma
- **Koşul**: Oyuncu `maxTurnCount` sayısına ulaşırsa
- **Aksiyon**: `maxTurnDialogue` diyalogu tetiklenir
- **Öncelik**: Turn sayısı kontrolü

## ⚙️ Kurulum

### 1. TurnConditionSystem Bileşeni
```csharp
// Inspector'da ayarlanacak alanlar:
[Title("Bar Sıfırlanma Diyalogları")]
public DialogueNode trustZeroDialogue;
public DialogueNode faithZeroDialogue;
public DialogueNode hostilityMaxDialogue;

[Title("Turn Limit Diyalogu")]
public DialogueNode maxTurnDialogue;
```

### 2. DialogueManager Bileşeni
```csharp
[Header("Turn Ayarları")]
[SerializeField] private TextMeshProUGUI turnText;
[SerializeField] private int maxTurnCount = 10;
```

### 2. Diyalog Oluşturma
Her durum için özel diyaloglar oluşturun:

#### Trust 0 Diyalogu Örneği:
```csharp
var trustZeroDialogue = new DialogueNode
{
    id = "trust_zero",
    text = "Bir ülkenin Trust değeri 0'a düştü! Bu durum oyunu etkileyebilir.",
    choices = new List<DialogueChoice>
    {
        new DialogueChoice
        {
            text = "Durumu kabul et",
            trustChange = 0,
            faithChange = 0,
            hostilityChange = 0
        },
        new DialogueChoice
        {
            text = "Çözüm ara",
            trustChange = 5,
            faithChange = 3,
            hostilityChange = -2
        }
    }
};
```

## 🔄 Çalışma Mantığı

### Bar Değeri Kontrolleri (Her Turn Sonunda)
```csharp
public void CheckTurnConditions()
{
    // 1. Trust 0'a düşen ülkeleri kontrol et
    CheckTrustZeroConditions();
    
    // 2. Faith 0'a düşen ülkeleri kontrol et
    CheckFaithZeroConditions();
    
    // 3. Hostility 100'e çıkan ülkeleri kontrol et
    CheckHostilityMaxConditions();
}
```

### Turn Kontrolü (Turn Değiştiğinde)
```csharp
public void CheckTurnConditionsOnTurnChange()
{
    // Maximum turn kontrolü
    CheckMaxTurnCondition();
}
```

### Kontrol Sırası
1. **Bar Değerleri**: Her turn sonunda tüm ülkelerin bar değerleri kontrol edilir
2. **Turn Limit**: Turn değiştiğinde maximum turn kontrolü yapılır

### Tetikleme Noktaları
- **Bar Kontrolleri**: MapManager'da `CheckTurnConditions()` çağrılır
- **Turn Kontrolü**: DialogueManager'da turn değiştiğinde otomatik tetiklenir

### Tetikleme Mantığı
- Her kontrol türü için **sadece ilk bulunan** durum tetiklenir
- Birden fazla ülke aynı durumda olsa bile sadece biri tetiklenir
- Diyalog tetiklendikten sonra diğer kontroller devam eder

## 🧪 Test Araçları

### TurnConditionTest Script'i
```csharp
[ContextMenu("Test Trust 0")]
public void TestTrustZero()

[ContextMenu("Test Faith 0")]
public void TestFaithZero()

[ContextMenu("Test Hostility 100")]
public void TestHostilityMax()

[ContextMenu("Test Max Turn")]
public void TestMaxTurn()

[ContextMenu("Bar Değerlerini Göster")]
public void ShowBarValues()

[ContextMenu("Turn Sayısını Göster")]
public void ShowTurnCount()

[ContextMenu("Turn Condition'ları Manuel Tetikle")]
public void ManuallyTriggerTurnConditions()
```

## 📝 Kullanım Örneği

### 1. Inspector'da Ayarlama
1. **TurnConditionSystem** bileşenini bir GameObject'e ekleyin
2. Inspector'da diyalogları atayın:
   - `trustZeroDialogue`
   - `faithZeroDialogue`
   - `hostilityMaxDialogue`
   - `maxTurnDialogue`

3. **DialogueManager** bileşeninde:
   - `maxTurnCount` değerini ayarlayın (varsayılan: 10)

### 2. Otomatik Tetikleme
- Sistem her turn sonunda otomatik olarak çalışır
- MapManager'da `CheckTurnConditions()` çağrılır
- Koşullar sağlandığında diyaloglar otomatik tetiklenir

### 3. Manuel Test
- TurnConditionTest script'ini kullanarak manuel test yapabilirsiniz
- Context Menu'den test fonksiyonlarını çağırın

## ⚠️ Önemli Notlar

1. **Öncelik Sırası**: Trust → Faith → Hostility → Turn
2. **Tek Tetikleme**: Her durum için sadece bir kez tetiklenir
3. **Null Kontrolü**: Diyalog atanmamışsa kontrol atlanır
4. **Debug Log**: Tetiklenen durumlar Console'da loglanır

## 🔧 Özelleştirme

### Yeni Durum Ekleme
```csharp
private void CheckNewCondition()
{
    if (newConditionDialogue == null) return;
    
    // Koşul kontrolü
    if (conditionMet)
    {
        Debug.Log("Yeni durum tetiklendi!");
        ShowSpecialDialogue(newConditionDialogue, country);
    }
}
```

### Kontrol Sırasını Değiştirme
`CheckTurnConditions()` metodunda kontrol sırasını değiştirebilirsiniz.

## 🎮 Oyun Entegrasyonu

Bu sistem şu durumlarda kullanılabilir:
- **Oyun Sonu Koşulları**: Bar değerleri kritik seviyelere ulaştığında
- **Özel Olaylar**: Belirli turn sayılarında özel diyaloglar
- **Uyarı Sistemi**: Oyuncuyu kritik durumlar hakkında bilgilendirme
- **Alternatif Sonlar**: Farklı koşullara göre farklı oyun sonları 