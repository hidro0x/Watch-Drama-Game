# GlobalDialogue JSON Import Sistemi

## 📋 Genel Bakış

Bu sistem JSON formatındaki GlobalDialogue verilerini Unity'deki GlobalDialogueNode formatına dönüştürür ve DialogueDatabase'e ekler. GlobalDialogues, tüm ülkeleri etkileyen özel diyaloglardır.

## 🎯 Desteklenen JSON Formatı

### Tek GlobalDialogue Formatı
```json
{
  "id": "global_peace_decision",
  "type": "global",
  "speaker": "Kahin",
  "text": "Tüm ülkeler için önemli bir karar vermen gerekiyor. Bu karar tüm dünyayı etkileyecek.",
  "choices": [
    {
      "text": "Barış yolunu seç",
      "globalEffects": [
        { "country": "Agnari", "trustChange": 5, "faithChange": 3, "hostilityChange": -2 },
        { "country": "Astrahil", "trustChange": 3, "faithChange": 4, "hostilityChange": -1 },
        { "country": "Varnan", "trustChange": 4, "faithChange": 2, "hostilityChange": -3 },
        { "country": "Theon", "trustChange": 2, "faithChange": 5, "hostilityChange": -1 },
        { "country": "Solarya", "trustChange": 3, "faithChange": 3, "hostilityChange": -2 }
      ]
    },
    {
      "text": "Savaş yolunu seç",
      "globalEffects": [
        { "country": "Agnari", "trustChange": -3, "faithChange": -2, "hostilityChange": 5 },
        { "country": "Astrahil", "trustChange": -2, "faithChange": -1, "hostilityChange": 4 },
        { "country": "Varnan", "trustChange": -4, "faithChange": -3, "hostilityChange": 6 },
        { "country": "Theon", "trustChange": -1, "faithChange": -4, "hostilityChange": 3 },
        { "country": "Solarya", "trustChange": -2, "faithChange": -2, "hostilityChange": 4 }
      ]
    }
  ]
}
```

### Çoklu GlobalDialogue Formatı (Array)
```json
[
  {
    "id": "global_peace_decision",
    "type": "global",
    "speaker": "Kahin",
    "text": "Barış kararı...",
    "choices": [
      {
        "text": "Barış yolunu seç",
        "globalEffects": [
          { "country": "Agnari", "trustChange": 5, "faithChange": 3, "hostilityChange": -2 }
        ]
      }
    ]
  },
  {
    "id": "global_trade_agreement",
    "type": "global",
    "speaker": "Ticaret Ustası",
    "text": "Ticaret anlaşması...",
    "choices": [
      {
        "text": "Anlaşmayı destekle",
        "globalEffects": [
          { "country": "Agnari", "trustChange": 3, "faithChange": 2, "hostilityChange": -1 }
        ]
      }
    ]
  }
]
```

### Alan Açıklamaları

| Alan | Tip | Açıklama |
|------|-----|----------|
| `id` | string | GlobalDialogue benzersiz kimliği |
| `type` | string | Diyalog türü (her zaman "global") |
| `speaker` | string | Konuşan karakter adı |
| `text` | string | Diyalog metni |
| `choices` | array | Seçenekler listesi |
| `globalEffects` | array | Tüm ülkeler için bar etkileri |

### Seçenek Formatı
```json
{
  "text": "Seçenek metni",
  "globalEffects": [
    {
      "country": "Agnari",
      "trustChange": 5,
      "faithChange": 3,
      "hostilityChange": -2
    }
  ]
}
```

### Ülke Bar Etkisi Formatı
```json
{
  "country": "Agnari",
  "trustChange": 5,
  "faithChange": 3,
  "hostilityChange": -2
}
```

## ⚙️ Kurulum

### 1. JSON Dosyalarını Hazırlama
1. `Assets/Resources/Dialogues/` klasörü oluşturun
2. GlobalDialogue JSON dosyalarını bu klasöre koyun
3. Dosya adları `_global.json` ile bitmeli veya "global" içermeli

### 2. Import Sistemi Kullanımı

#### **A. Unity Editor Window**
1. `Kahin > Global Dialogue JSON Import` menüsünü açın
2. JSON dosyasını seçin
3. Hedef Database'i seçin
4. "Seçili JSON'u Global Dialogue Import Et" butonuna tıklayın

#### **B. Runtime Import**
```csharp
// JsonGlobalDialogueImporter bileşenini kullanın
JsonGlobalDialogueImporter importer = FindObjectOfType<JsonGlobalDialogueImporter>();
importer.ImportGlobalDialogueFromJson();
```

#### **C. Toplu Import**
1. `Kahin > Global Dialogue JSON Import` menüsünü açın
2. "Tüm Global Dialogue JSON Dosyalarını Import Et" butonuna tıklayın
3. `_global.json` ile biten tüm dosyalar otomatik import edilir

## 🔧 Kullanım Örnekleri

### 1. Tek Dosya Import
```csharp
// Inspector'da JSON dosyasını seçin
// Context Menu'den "Global Dialogue JSON'dan Import Et" çağırın
```

### 2. Toplu Import
```csharp
// Context Menu'den "Tüm Global Dialogue JSON Dosyalarını Import Et" çağırın
// Resources/Dialogues klasöründeki tüm global JSON dosyaları import edilir
```

### 3. Runtime Import
```csharp
[ContextMenu("Global Dialogue JSON'dan Import Et")]
public void ImportGlobalDialogueFromJson()
{
    // Otomatik import işlemi
}
```

## 📁 Dosya Yapısı

```
Assets/
├── Resources/
│   └── Dialogues/
│       ├── global_dialogues_example.json
│       ├── peace_global.json
│       ├── trade_global.json
│       └── religious_global.json
├── JsonGlobalDialogueImporter.cs
├── JsonGlobalDialogueImportWindow.cs
└── GLOBAL_DIALOGUE_JSON_IMPORT_SYSTEM.md
```

## 🎮 Oyun İçi Kullanım

### GlobalDialogue Tetikleme
GlobalDialogues otomatik olarak belirli turn aralıklarında tetiklenir:

```csharp
// DialogueDatabase'de ayarlanır
public int globalDialogueInterval = 16; // Her 16 turn'de global diyalog
```

### GlobalDialogue Seçimi
```csharp
// MapManager'da otomatik seçim
if (currentTurn % dialogueDatabase.globalDialogueInterval == 0)
{
    var globalDialogue = dialogueDatabase.globalDialogueEffects[Random.Range(0, dialogueDatabase.globalDialogueEffects.Count)];
    return ConvertGlobalDialogueToDialogue(globalDialogue);
}
```

## 🔄 Dönüştürme İşlemi

### JSON → GlobalDialogueNode
```csharp
GlobalDialogueNode globalDialogueNode = new GlobalDialogueNode
{
    id = jsonData.id,
    text = $"{jsonData.speaker}: {jsonData.text}",
    choices = new List<GlobalDialogueChoice>()
};

foreach (var choice in jsonData.choices)
{
    GlobalDialogueChoice globalChoice = new GlobalDialogueChoice
    {
        text = choice.text,
        globalEffects = new List<CountryBarEffect>()
    };
    
    foreach (var effect in choice.globalEffects)
    {
        CountryBarEffect countryEffect = new CountryBarEffect
        {
            country = ParseMapType(effect.country),
            trustChange = effect.trustChange,
            faithChange = effect.faithChange,
            hostilityChange = effect.hostilityChange
        };
        
        globalChoice.globalEffects.Add(countryEffect);
    }
    
    globalDialogueNode.choices.Add(globalChoice);
}
```

### GlobalDialogueNode → DialogueNode
```csharp
DialogueNode dialogueNode = new DialogueNode
{
    id = globalDialogue.id,
    text = globalDialogue.text,
    choices = new List<DialogueChoice>(),
    isGlobalDialogue = true
};

foreach (var globalChoice in globalDialogue.choices)
{
    DialogueChoice choice = new DialogueChoice
    {
        text = globalChoice.text,
        trustChange = 0, // Global diyaloglarda yerel değişiklik yok
        faithChange = 0,
        hostilityChange = 0,
        isGlobalChoice = true
    };
    
    dialogueNode.choices.Add(choice);
}
```

## 🎯 Özellikler

### ✅ Desteklenen Özellikler
- ✅ Tek ve çoklu GlobalDialogue import
- ✅ Tüm ülkeler için bar etkileri
- ✅ Unity Editor Window arayüzü
- ✅ Runtime import desteği
- ✅ Otomatik dosya tarama
- ✅ Hata yönetimi
- ✅ Örnek JSON oluşturma

### 🔧 Teknik Detaylar
- **Dosya Formatı**: JSON
- **Encoding**: UTF-8
- **Dependency**: Newtonsoft.Json
- **Unity Version**: 2022.3+
- **Platform**: Windows, macOS, Linux

## 🚀 Hızlı Başlangıç

### 1. Örnek JSON Oluştur
```csharp
// Unity Editor'da
// Kahin > Global Dialogue JSON Import
// "Örnek Global Dialogue JSON Oluştur" butonuna tıkla
```

### 2. JSON Dosyasını Import Et
```csharp
// Unity Editor'da
// Kahin > Global Dialogue JSON Import
// JSON dosyasını seç ve "Import Et" butonuna tıkla
```

### 3. Oyun İçi Test
```csharp
// GlobalDialogue'lar otomatik olarak belirli turn'lerde tetiklenir
// Test için globalDialogueInterval değerini düşürebilirsiniz
```

## 📝 Notlar

### Önemli Noktalar
1. **Ülke İsimleri**: JSON'da ülke isimleri MapType enum değerleriyle eşleşmeli
2. **Bar Değerleri**: trustChange, faithChange, hostilityChange değerleri -100 ile +100 arasında olmalı
3. **Dosya Adlandırma**: GlobalDialogue dosyaları `_global.json` ile bitmeli
4. **Database**: Import edilen GlobalDialogues DialogueDatabase'e otomatik eklenir

### Hata Durumları
- **Bilinmeyen Ülke**: Varsayılan olarak Agnari kullanılır
- **Geçersiz JSON**: Hata mesajı gösterilir
- **Eksik Alanlar**: Varsayılan değerler kullanılır

## 🔗 İlgili Dosyalar

- `JsonGlobalDialogueImporter.cs` - Runtime import sistemi
- `JsonGlobalDialogueImportWindow.cs` - Unity Editor Window
- `DialogueDatabase.cs` - GlobalDialogue storage
- `GlobalDialogueNode.cs` - GlobalDialogue data structure
- `MapManager.cs` - GlobalDialogue triggering system 