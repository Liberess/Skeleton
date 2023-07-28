using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentSlot : MonoBehaviour
{
    private EquipmentDataSO equipDataSO;

    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI descriptTxt;
    [SerializeField] private Image equipIcon;
    [SerializeField] private TextMeshProUGUI costTxt;
    [SerializeField] private Button buyBtn;
    
    [SerializeField] private GameObject soldoutPanel;

    private void Start()
    {
        buyBtn.onClick.AddListener(OnClickBuy);
    }

    public void SetupSlot(EquipmentDataSO dataSO)
    {
        equipDataSO = dataSO;

        UpdateEquipUnlockPanel();

        nameTxt.text = equipDataSO.EquipmentData.equipName;
        int cost = equipDataSO.EquipmentData.equipBuyCost;
        costTxt.text = string.Concat("<sprite=0>", cost > 0 ? $"{cost:#,###}" : "0");

        string impactNameStr;
        if (equipDataSO.EquipmentData.equipType == EEquipType.Weapon)
            impactNameStr = "공격력";
        else
            impactNameStr = "방어력";
        
        descriptTxt.text = string.Format(equipDataSO.EquipmentData.equipDescription, impactNameStr, equipDataSO.EquipmentData.impactAmount);
        equipIcon.sprite = equipDataSO.EquipmentData.equipIcon;
    }

    private void UpdateEquipUnlockPanel()
    {
        soldoutPanel.SetActive(equipDataSO.EquipmentData.isEquipUnlock);
        buyBtn.gameObject.SetActive(!equipDataSO.EquipmentData.isEquipUnlock);
        costTxt.gameObject.SetActive(!equipDataSO.EquipmentData.isEquipUnlock);
        
        if(equipDataSO.EquipmentData.isEquipUnlock)
            transform.SetAsLastSibling();
    }
    
    private void OnClickBuy()
    {
        if (GameManager.Instance.Gold >= equipDataSO.EquipmentData.equipBuyCost)
        {
            GameManager.Instance.Gold -= equipDataSO.EquipmentData.equipBuyCost;
            DataManager.Instance.AcquireEquipment(equipDataSO.EquipmentData.equipType, equipDataSO.EquipmentData.EquipID);
            UpdateEquipUnlockPanel();
        }
    }
}
