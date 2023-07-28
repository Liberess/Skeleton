using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class EquipmentData
{
    public string equipName;
    [ResizableTextArea]
    public string equipDescription;
    public EEquipType equipType;
    public bool isEquipUnlock = false;
    [MinValue(1)]
    public int equipLv;
    [MinValue(10), MaxValue(99)]
    public int maxEquipLv;
    public int impactAmount;
    public int equipBuyCost;
    public int equipUpCost;

    [EnableIf("isProjectileSkill")]
    public Sprite equipIcon;

    public EquipmentData(EquipmentData data)
    {
        equipName = data.equipName;
        equipDescription = data.equipDescription;
        equipType = data.equipType;
        isEquipUnlock = data.isEquipUnlock;
        equipLv = data.equipLv;
        maxEquipLv = data.maxEquipLv;
        impactAmount = data.impactAmount;
        equipBuyCost = data.equipBuyCost;
        equipUpCost = data.equipUpCost;
        equipIcon = data.equipIcon;
    }
    
    public EquipmentData(EquipmentDataSO dataSO)
    {
        equipName = dataSO.EquipmentData.equipName;
        equipDescription = dataSO.EquipmentData.equipDescription;
        equipType = dataSO.EquipmentData.equipType;
        isEquipUnlock = dataSO.EquipmentData.isEquipUnlock;
        equipLv = dataSO.EquipmentData.equipLv;
        maxEquipLv = dataSO.EquipmentData.maxEquipLv;
        impactAmount = dataSO.EquipmentData.impactAmount;
        equipBuyCost = dataSO.EquipmentData.equipBuyCost;
        equipUpCost = dataSO.EquipmentData.equipUpCost;
        equipIcon = dataSO.EquipmentData.equipIcon;
    }
}
