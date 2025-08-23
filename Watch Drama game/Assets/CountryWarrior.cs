using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class CountryWarrior
{
    [Title("Savaşçı Bilgileri")]
    [LabelWidth(100)]
    public MapType country;
    
    [LabelWidth(100)]
    public string warriorName;
    
    [LabelWidth(100)]
    [PreviewField(70)]
    public Sprite warriorSprite;
    
    [Title("Taraf Seçme Metni")]
    [TextArea(2, 3)]
    [LabelWidth(150)]
    public string allianceProposalText; // "Ben [Savaşçı Adı], [Ülke]'dan geliyorum. Seninle ittifak kurmak istiyorum!"
    
    [Title("Seçenek Metinleri")]
    [LabelWidth(150)]
    public string joinWarriorSideText = "Savaşçının tarafına katıl";
    
    [LabelWidth(150)]
    public string helpCurrentCountryText = "Mevcut ülkeye yardım et";
    
    [Title("Savaşçı Tarafına Katılma Etkileri")]
    [LabelWidth(150)]
    public int joinWarriorTrust = 5;
    [LabelWidth(150)]
    public int joinWarriorFaith = 3;
    [LabelWidth(150)]
    public int joinWarriorHostility = 8;
    
    [Title("Mevcut Ülkeye Yardım Etkileri")]
    [LabelWidth(150)]
    public int helpCurrentTrust = -2;
    [LabelWidth(150)]
    public int helpCurrentFaith = -1;
    [LabelWidth(150)]
    public int helpCurrentHostility = 2;
}
