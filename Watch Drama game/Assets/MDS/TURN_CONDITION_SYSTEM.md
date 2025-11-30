# Turn Condition Sistemi

## Genel Bakış

Turn Condition Sistemi, her turn sonunda otomatik olarak çalışan modüler condition'ları yönetir. Bu sistem sayesinde oyun dinamiklerini kolayca kontrol edebilir ve özel aksiyonlar tetikleyebilirsiniz.

## Özellikler

### ✅ Modüler Yapı
- Inspector'dan kolayca condition ekleme/çıkarma
- Her condition bağımsız olarak aktif/pasif yapılabilir
- Yeni condition türleri kolayca eklenebilir

### ✅ Çeşitli Condition Türleri
- **BarValue**: Belirli bir ülkenin bar değeri kontrolü
- **TurnCount**: Turn sayısı kontrolü
- **Custom**: Özel condition'lar için genişletilebilir

### ✅ Çeşitli Aksiyon Türleri
- **ShowDialogue**: Özel diyalog göster
- **ApplyGlobalEffect**: Global effect uygula
- **ShowMessage**: Mesaj göster
- **CustomAction**: Özel aksiyon

### ✅ Karşılaştırma Seçenekleri
- GreaterThan (>)
- LessThan (<)
- EqualTo (=)
- GreaterThanOrEqual (>=)
- LessThanOrEqual (<=)

## Kullanım

### 1. TurnConditionSystem Ekleme
1. Sahnede boş bir GameObject oluşturun
2. `TurnConditionSystem` component'ini ekleyin
3. Inspector'da condition'ları yönetin

### 2. Condition Ekleme
```csharp
// Kod ile condition ekleme
var condition = new TurnCondition
{
    conditionName = "Varnan Yüksek Trust",
    IsActive = true,
    conditionType = ConditionType.BarValue,
    targetCountry = MapType.Varnan,
    targetValue = ValueType.Trust,
    comparison = ComparisonType.GreaterThan,
    thresholdValue = 80,
    actionType = ActionType.ShowMessage,
    customMessage = "Varnan'ın güveni çok yüksek!"
};

turnConditionSystem.turnConditions.Add(condition);
```

### 3. Özel Condition'lar
`CustomConditionBase` sınıfını extend ederek özel condition'lar oluşturabilirsiniz:

```csharp
public class MyCustomCondition : CustomConditionBase
{
    public override bool CheckCustomCondition(MapType currentMap)
    {
        // Özel kontrol mantığı
        return true;
    }
    
    public override void ExecuteCustomAction(MapType currentMap)
    {
        // Özel aksiyon
        Debug.Log("Özel condition tetiklendi!");
    }
}
```

## Örnekler

### Örnek 1: Varnan'ın Trust'ı 80'in üstündeyse uyarı
```csharp
var condition = new TurnCondition
{
    conditionName = "Varnan Yüksek Trust",
    conditionType = ConditionType.BarValue,
    targetCountry = MapType.Varnan,
    targetValue = ValueType.Trust,
    comparison = ComparisonType.GreaterThan,
    thresholdValue = 80,
    actionType = ActionType.ShowMessage,
    customMessage = "Varnan'ın güveni çok yüksek!"
};
```

### Örnek 2: Turn 10'dan büyükse global effect
```csharp
var condition = new TurnCondition
{
    conditionName = "Turn 10+ Global Effect",
    conditionType = ConditionType.TurnCount,
    comparison = ComparisonType.GreaterThan,
    thresholdValue = 10,
    actionType = ActionType.ApplyGlobalEffect,
    globalEffect = myGlobalEffect
};
```

### Örnek 3: Astrahil'in Faith'i 30'un altındaysa uyarı
```csharp
var condition = new TurnCondition
{
    conditionName = "Astrahil Düşük Faith",
    conditionType = ConditionType.BarValue,
    targetCountry = MapType.Astrahil,
    targetValue = ValueType.Faith,
    comparison = ComparisonType.LessThan,
    thresholdValue = 30,
    actionType = ActionType.ShowMessage,
    customMessage = "Astrahil'in inancı çok düşük!"
};
```

## Editor Araçları

### Turn Condition Editor
- **Menü**: Kahin > Turn Condition Editor
- **Özellikler**:
  - Hızlı condition ekleme
  - Tüm condition'ları test etme
  - Condition'ları temizleme
  - Örnek condition'lar ekleme

### Test Araçları
- `TurnConditionExample` script'i ile test
- Context Menu'den "Test Turn Conditions"
- Context Menu'den "Manuel Turn İlerlet"

## Dosya Yapısı

```
Assets/
├── TurnConditionSystem.cs          # Ana sistem
├── CustomConditionBase.cs          # Özel condition base class
├── TurnConditionExample.cs         # Örnek kullanım
├── TurnConditionEditorWindow.cs    # Editor window
└── TURN_CONDITION_SYSTEM.md       # Bu dokümantasyon
```

## Entegrasyon

### MapManager Entegrasyonu
- `StartNextDialogue()` metodunda otomatik kontrol
- Her turn sonunda condition'lar kontrol edilir

### GameManager Entegrasyonu
- Bar değerleri için getter metodları
- Global effect uygulama desteği

## Genişletme

### Yeni Condition Türü Ekleme
1. `ConditionType` enum'ına yeni tür ekleyin
2. `TurnCondition.CheckCondition()` metodunda yeni case ekleyin
3. Gerekli kontrol mantığını implement edin

### Yeni Aksiyon Türü Ekleme
1. `ActionType` enum'ına yeni tür ekleyin
2. `TurnCondition.ExecuteAction()` metodunda yeni case ekleyin
3. Gerekli aksiyon mantığını implement edin

## Avantajlar

1. **Modülerlik**: Her condition bağımsız
2. **Esneklik**: Çeşitli condition ve aksiyon türleri
3. **Kolay Kullanım**: Inspector'dan yönetim
4. **Genişletilebilirlik**: Yeni türler kolayca eklenebilir
5. **Test Edilebilirlik**: Editor araçları ile test
6. **Performans**: Sadece turn sonunda kontrol

## Notlar

- Condition'lar her turn sonunda kontrol edilir
- Birden fazla condition aynı anda tetiklenebilir
- Custom condition'lar için GameObject gerekli
- Global effect'ler tüm ülkeleri etkiler 