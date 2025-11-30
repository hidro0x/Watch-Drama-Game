# JSON Diyalog Import Sistemi

## 📋 Genel Bakış

Bu sistem JSON formatındaki diyalog verilerini Unity'deki DialogueNode formatına dönüştürür ve DialogueDatabase'e ekler.

## 🎯 Desteklenen JSON Formatı

### Tek Diyalog Formatı
```json
{
  "id": "n15",
  "type": "normal",
  "speaker": "Kayıp Ruh Avcısı",
  "text": "Bazı ruhlar geri dönmez. Sen geri dönmeyi düşünüyor musun?",
  "options": [
    { "text": "Hayır, yolum ileri.", "effects": { "faith": 4, "trust": 3, "hostility": 1 } },
    { "text": "Belki bir gün, ama henüz değil.", "effects": { "faith": 5, "trust": 4, "hostility": 0 } },
    { "text": "Dönüşüm yok. Sonuna kadar gideceğim.", "effects": { "faith": 2, "trust": 2, "hostility": 3 } }
  ]
}
```

### Çoklu Diyalog Formatı (Array)
```json
[
  {
    "id": "n15",
    "type": "normal",
    "speaker": "Kayıp Ruh Avcısı",
    "text": "Bazı ruhlar geri dönmez...",
    "options": [
      { "text": "Hayır, yolum ileri.", "effects": { "faith": 4, "trust": 3, "hostility": 1 } }
    ]
  },
  {
    "id": "n16",
    "type": "normal",
    "speaker": "Başka Karakter",
    "text": "Başka bir diyalog...",
    "options": [
      { "text": "Seçenek 1", "effects": { "faith": 2, "trust": 1, "hostility": 0 } }
    ]
  }
]
```

### Map'e Özel Diyalog Formatı
```json
{
  "mapType": "Varnan",
  "dialogues": [
    {
      "id": "varnan_01",
      "type": "normal",
      "speaker": "Varnan Gözcüsü",
      "text": "Varnan'ın gölgeleri seni izliyor. Neden geldin?",
      "options": [
        {
          "text": "Barış için geldim.",
          "effects": {
            "faith": 4,
            "trust": 3,
            "hostility": 0
          }
        },
        {
          "text": "Savaş için geldim.",
          "effects": {
            "faith": 2,
            "trust": 1,
            "hostility": 5
          }
        }
      ]
    }
  ]
}
```

### Alan Açıklamaları

| Alan | Tip | Açıklama |
|------|-----|----------|
| `id` | string | Diyalog benzersiz kimliği |
| `type` | string | Diyalog türü (normal, global, vb.) |
| `speaker` | string | Konuşan karakter adı |
| `text` | string | Diyalog metni |
| `options` | array | Seçenekler listesi |
| `mapType` | string | Map türü (sadece map'e özel formatlarda) |
| `dialogues` | array | Diyalog listesi (sadece map'e özel formatlarda) |

### Seçenek Formatı
```json
{
  "text": "Seçenek metni",
  "effects": {
    "faith": 4,
    "trust": 3,
    "hostility": 1
  }
}
```

## ⚙️ Kurulum

### 1. JSON Dosyalarını Hazırlama
1. `Assets/Resources/Dialogues/` klasörü oluşturun
2. JSON dosyalarını bu klasöre koyun
3. Dosya adları `.json` uzantılı olmalı

### 2. Import Sistemi Kullanımı

#### **A. Unity Editor Window**
1. `Kahin > JSON Diyalog Import` menüsünü açın
2. JSON dosyasını seçin
3. Hedef Database'i seçin
4. "Seçili JSON'u Import Et" butonuna tıklayın

#### **B. Runtime Import**
```csharp
// JsonDialogueImporter bileşenini kullanın
JsonDialogueImporter importer = FindObjectOfType<JsonDialogueImporter>();
importer.ImportFromJson();
```

#### **C. Map'e Özel Import**
1. `Kahin > JSON Diyalog Import` menüsünü açın
2. Map'e özel JSON dosyasını seçin
3. "Map'e Özel Diyalog Import Et" butonuna tıklayın

## 🔧 Kullanım Örnekleri

### 1. Tek Dosya Import
```csharp
// Inspector'da JSON dosyasını seçin
// Context Menu'den "JSON'dan Import Et" çağırın
```

### 2. Toplu Import
```csharp
// Context Menu'den "Tüm JSON Dosyalarını Import Et" çağırın
// Resources/Dialogues klasöründeki tüm JSON dosyaları import edilir
```

### 3. Runtime Import
```csharp
[ContextMenu("JSON'dan Import Et")]
public void ImportFromJson()
{
    // Otomatik import işlemi
}
```

## 📁 Dosya Yapısı

```
Assets/
├── Resources/
│   └── Dialogues/
│       ├── dialogue_1.json
│       ├── dialogue_2.json
│       └── dialogue_3.json
├── JsonDialogueImporter.cs
├── JsonDialogueImportWindow.cs
└── DialogueDatabase.cs
```

## 🎮 Özellikler

### 1. Otomatik Dönüştürme
- JSON formatı → DialogueNode formatı
- Speaker + Text → Birleştirilmiş metin
- Effects → Bar değişiklikleri

### 2. Hata Yönetimi
- JSON parse hatalarını yakalar
- Eksik alanlar için varsayılan değerler
- Detaylı hata mesajları

### 3. Editor Desteği
- Unity Editor window
- Drag & drop dosya seçimi
- Import sonuçlarını görüntüleme

### 4. Batch Import
- Tüm JSON dosyalarını tek seferde import
- Progress tracking
- Hata raporlama

## 📝 Dönüştürme Kuralları

### Text Dönüştürme
```csharp
// JSON
"speaker": "Kayıp Ruh Avcısı",
"text": "Bazı ruhlar geri dönmez..."

// DialogueNode
text = "Kayıp Ruh Avcısı: Bazı ruhlar geri dönmez..."
```

### Effects Dönüştürme
```csharp
// JSON
"effects": { "faith": 4, "trust": 3, "hostility": 1 }

// DialogueChoice
faithChange = 4,
trustChange = 3,
hostilityChange = 1
```

## ⚠️ Önemli Notlar

1. **Dosya Konumu**: JSON dosyaları `Resources/Dialogues/` klasöründe olmalı
2. **Dosya Uzantısı**: `.json` uzantısı gerekli
3. **JSON Formatı**: Geçerli JSON formatı olmalı
4. **Database**: Import edilen diyaloglar DialogueDatabase'e eklenir
5. **Benzersiz ID**: Her diyalog için benzersiz ID kullanın

## 🔧 Özelleştirme

### Yeni Alan Ekleme
```csharp
[System.Serializable]
public class JsonDialogueData
{
    public string id;
    public string type;
    public string speaker;
    public string text;
    public List<JsonDialogueOption> options;
    public string newField; // Yeni alan
}
```

### Özel Dönüştürme
```csharp
private DialogueNode ConvertJsonToDialogueNode(JsonDialogueData jsonData)
{
    // Özel dönüştürme mantığı
    DialogueNode dialogueNode = new DialogueNode();
    
    // Özel alanları işle
    if (!string.IsNullOrEmpty(jsonData.newField))
    {
        // Özel işlem
    }
    
    return dialogueNode;
}
```

## 🎯 Kullanım Senaryoları

### 1. Yazarlar İçin
- JSON formatında diyalog yazın
- Unity Editor'da import edin
- Anında test edin

### 2. Çevirmenler İçin
- JSON dosyalarını çevirin
- Toplu import yapın
- Çoklu dil desteği

### 3. Geliştiriciler İçin
- Programatik import
- Runtime diyalog yükleme
- Dinamik içerik

Bu sistem sayesinde JSON formatındaki diyaloglarınızı kolayca Unity'ye import edebilirsiniz! 🚀 