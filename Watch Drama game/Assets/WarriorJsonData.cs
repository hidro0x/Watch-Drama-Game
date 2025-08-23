using System;
using System.Collections.Generic;

[Serializable]
public class WarriorJsonData
{
    public string country; // MapType string olarak
    public string warriorName;
    public string allianceProposalText;
    public string joinWarriorSideText;
    public string helpCurrentCountryText;
    public WarriorEffects joinWarriorEffects;
    public WarriorEffects helpCurrentEffects;
}

[Serializable]
public class WarriorEffects
{
    public int trust;
    public int faith;
    public int hostility;
    public int opponentTrust;
    public int opponentFaith;
    public int opponentHostility;
}

[Serializable]
public class WarriorImportData
{
    public List<WarriorJsonData> warriors;
}
