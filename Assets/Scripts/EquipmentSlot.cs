using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentSlot : MonoBehaviour
{
    public EquipmentData EquipData { get; private set; }

    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private TextMeshProUGUI lvTxt;
    [SerializeField] private TextMeshProUGUI descriptTxt;
    [SerializeField] private Image equipIcon;
    [SerializeField] private GameObject equipStateIcon;
    [SerializeField] private TextMeshProUGUI costTxt;
    [SerializeField] private Button buyBtn;
    [SerializeField] private Button equipBtn;
    [SerializeField] private GameObject lockPanel;

    private string impactNameStr; 

    private void Start()
    {
        buyBtn.onClick.AddListener(OnClickBuy);
        equipBtn.onClick.AddListener(OnClickEquip);
    }

    public void SetupSlot(EquipmentData data)
    {
        //EquipData = DataManager.Instance.GetEquipmentData(equipDataSO.EquipmentData.equipType, equipDataSO.EquipmentData.EquipID);
        EquipData = data;
        
        if(EquipData.equipType == EEquipType.Weapon)
            UIManager.Instance.UpdateWeaponUIAction += UpdateSlot;
        else
            UIManager.Instance.UpdateArmorUIAction += UpdateSlot;

        nameTxt.text = EquipData.equipName;
        lvTxt.text = EquipData.equipLv.ToString();
        
        int cost = EquipData.equipBuyCost;
        costTxt.text = string.Concat("<sprite=0>", cost > 0 ? $"{cost:#,###}" : "0");

        if (EquipData.equipType == EEquipType.Weapon)
            impactNameStr = "공격력";
        else
            impactNameStr = "체력";

        descriptTxt.text = string.Format(EquipData.equipDescription, impactNameStr,
            EquipData.impactAmount);
        equipIcon.sprite = EquipData.equipIcon;
        
        UpdateSlot();
    }

    public void UpdateSlot()
    {
        int cost = EquipData.isEquipUnlock ? EquipData.equipUpCost : EquipData.equipBuyCost;
        
        buyBtn.GetComponentInChildren<TextMeshProUGUI>().text = EquipData.isEquipUnlock ? "강화" : "구매";
        equipBtn.gameObject.SetActive(EquipData.isEquipUnlock);
        lockPanel.SetActive(!EquipData.isEquipUnlock);
        equipBtn.interactable = !EquipData.isEquip;
        equipStateIcon.SetActive(EquipData.isEquip);

        bool isMaxLevel = EquipData.equipLv >= EquipData.maxEquipLv;
        lvTxt.text = isMaxLevel ? "Max" : EquipData.equipLv.ToString();
        costTxt.text = string.Concat("<sprite=0>", cost > 0 ? $"{cost:#,###}" : "0");
        buyBtn.gameObject.SetActive(!isMaxLevel);
        costTxt.gameObject.SetActive(!isMaxLevel);
        
        descriptTxt.text = string.Format(EquipData.equipDescription, impactNameStr,
            EquipData.impactAmount);
    }

    private void OnClickBuy()
    {
        if (EquipData.isEquipUnlock)
        {
            if (GameManager.Instance.Gold >= EquipData.equipUpCost)
            {
                GameManager.Instance.Gold -= EquipData.equipUpCost;
                EquipData.equipUpCost = Mathf.RoundToInt(
                    Mathf.Clamp(EquipData.equipUpCost + EquipData.originEquipUpCost * 1.4f, 0.0f,
                        DataManager.Instance.GameData.MaxGoodsAmount));
                EquipData.impactAmount = Mathf.RoundToInt(
                    Mathf.Clamp(EquipData.impactAmount + EquipData.originImpactAmount * 1.4f, 0.0f,
                        DataManager.Instance.GameData.MaxGoodsAmount));

                if (EquipData.equipType == EEquipType.Weapon)
                    DataManager.Instance.GameData.playerData.increaseAttackPower = EquipData.impactAmount;
                else
                    DataManager.Instance.GameData.playerData.increaseHealthPoint = EquipData.impactAmount;
                
                EquipData.equipLv += 1;
                UpdateSlot();
            }
        }
        else
        {
            if (GameManager.Instance.Gold >= EquipData.equipBuyCost)
            {
                GameManager.Instance.Gold -= EquipData.equipBuyCost;
                DataManager.Instance.AcquireEquipment(EquipData.equipType, EquipData.EquipID);
                
                UpdateSlot();
            }
        }
    }

    private void OnClickEquip()
    {
        if (EquipData.equipType == EEquipType.Weapon)
        {
            if (DataManager.Instance.GameData.curEquipWeaponID >= 0)
                DataManager.Instance.DisarmEquipment(EquipData.equipType);
  
            EquipData.isEquip = true;
            DataManager.Instance.GameData.curEquipWeapon = EquipData;
            DataManager.Instance.GameData.curEquipWeaponID = EquipData.EquipID;
            DataManager.Instance.GameData.playerData.increaseAttackPower = EquipData.impactAmount;
            UpdateSlot();
        }
        else
        {
            if (DataManager.Instance.IsChangeableArmor(EquipData.impactAmount))
            {
                if (DataManager.Instance.GameData.curEquipArmorID >= 0)
                    DataManager.Instance.DisarmEquipment(EquipData.equipType);
                EquipData.isEquip = true;
                DataManager.Instance.GameData.curEquipArmor = EquipData;
                DataManager.Instance.GameData.curEquipArmorID = EquipData.EquipID;
                DataManager.Instance.GameData.playerData.increaseHealthPoint = EquipData.impactAmount;
                UIManager.Instance.UpdatePlayerHpUI();
                UpdateSlot();
            }
        }
    }
}