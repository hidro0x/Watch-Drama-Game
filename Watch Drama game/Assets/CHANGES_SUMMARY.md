# ConditionalDialogueNode Kaldırma ve Global Effects Sistemi

## Yapılan Değişiklikler

### 1. ConditionalDialogueNode Tamamen Kaldırıldı
- `Assets/ConditionalDialogueNode.cs` dosyası silindi
- `Assets/ConditionalDialogueNode.cs.meta` dosyası silindi
- Tüm referanslar temizlendi

### 2. Yeni Global Effects Sistemi Eklendi

#### GlobalDialogueEffect.cs
- Global diyalog etkilerini yönetmek için yeni sınıf
- Her ülke için ayrı bar etkileri tanımlanabilir
- Örnek: Varnan +5 +5 -3, Astrahil +2 -1 +0

#### BarValues Struct
- Her ülke için Trust, Faith, Hostility değişimlerini tutar
- Dictionary<MapType, BarValues> ile ülke ve değerler eşleştirilir

### 3. DialogueDatabase Güncellendi
- `globalConditionalDialogues` → `globalDialogueEffects`
- Yeni GlobalDialogueEffect listesi eklendi
- TableList ile düzenli görünüm

### 4. GameManager Güncellendi
- `ApplyGlobalDialogueEffect()` metodu eklendi
- Tüm ülkelerin bar değerlerini aynı anda güncelleyebilir
- `GetTrustForCountry()`, `GetFaithForCountry()`, `GetHostilityForCountry()` metodları eklendi

### 5. DialogueNode Güncellendi
- DialogueChoice'a global effects desteği eklendi
- `globalEffects` listesi eklendi
- `hasGlobalEffects` property eklendi

### 6. ChoiceButtonSlot Güncellendi
- Global effects uygulama desteği eklendi
- Hem yerel hem global etkiler aynı anda uygulanabilir

## Kullanım Örneği

### Global Effect Tanımlama
```csharp
var globalEffect = new GlobalDialogueEffect();
globalEffect.countryEffects = new Dictionary<MapType, BarValues>
{
    { MapType.Varnan,   new BarValues { trust = 5, faith = 5, hostility = -3 } },
    { MapType.Astrahil, new BarValues { trust = 2, faith = -1, hostility = 0 } },
    { MapType.Solarya,  new BarValues { trust = 0, faith = 3, hostility = 1 } }
};
```

### Global Effect Uygulama
```csharp
GameManager.Instance.ApplyGlobalDialogueEffect(globalEffect);
```

### Dialogue Choice'ta Global Effects
- Inspector'da DialogueChoice'a global effects eklenebilir
- Her ülke için ayrı değerler ayarlanabilir
- Normal choice + global effects aynı anda çalışır

## Avantajlar

1. **Esneklik**: Her ülke için farklı değerler
2. **Kolay Kullanım**: Inspector'da düzenleme
3. **Performans**: Tek seferde tüm ülkeleri güncelleme
4. **Geriye Uyumluluk**: Mevcut sistemler çalışmaya devam eder

## Test

`Assets/GlobalEffectsExample.cs` dosyası test için oluşturuldu.
Context Menu'den "Test Global Effect" ile test edilebilir. 