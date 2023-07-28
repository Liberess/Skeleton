using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class EquipmentData
{
    public string equipName;
    [ReadOnly, SerializeField] private int equipID;
    public int EquipID => equipID;
    [ResizableTextArea] public string equipDescription;
    public EEquipType equipType;
    public bool isEquip;
    public bool isEquipUnlock;
    [MinValue(1)] public int equipLv;
    [MinValue(10), MaxValue(99)] public int maxEquipLv;
    public int impactAmount;
    public int equipBuyCost;
    [ReadOnly] public int equipUpCost;
    public int originEquipUpCost;

    [ShowAssetPreview] public Sprite equipIcon;

    public EquipmentData(EquipmentData data)
    {
        equipName = data.equipName;
        equipID = data.equipID;
        equipDescription = data.equipDescription;
        equipType = data.equipType;
        isEquip = data.isEquip;
        isEquipUnlock = data.isEquipUnlock;
        equipLv = data.equipLv;
        maxEquipLv = data.maxEquipLv;
        impactAmount = data.impactAmount;
        equipBuyCost = data.equipBuyCost;
        equipUpCost = data.originEquipUpCost;
        originEquipUpCost = data.originEquipUpCost;
        equipIcon = data.equipIcon;
    }
    
    public EquipmentData(EquipmentDataSO dataSO)
    {
        equipName = dataSO.EquipmentData.equipName;
        equipID = dataSO.EquipmentData.equipID;
        equipDescription = dataSO.EquipmentData.equipDescription;
        equipType = dataSO.EquipmentData.equipType;
        isEquip = dataSO.EquipmentData.isEquip;
        isEquipUnlock = dataSO.EquipmentData.isEquipUnlock;
        equipLv = dataSO.EquipmentData.equipLv;
        maxEquipLv = dataSO.EquipmentData.maxEquipLv;
        impactAmount = dataSO.EquipmentData.impactAmount;
        equipBuyCost = dataSO.EquipmentData.equipBuyCost;
        equipUpCost = dataSO.EquipmentData.originEquipUpCost;
        originEquipUpCost = dataSO.EquipmentData.originEquipUpCost;
        equipIcon = dataSO.EquipmentData.equipIcon;
    }

    public void SetEquipID(int value) => equipID = value;
}
