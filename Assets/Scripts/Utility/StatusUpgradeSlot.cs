using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusUpgradeSlot : MonoBehaviour
{
    [SerializeField] private EStatusType statusType;
    [SerializeField] private int ID = 0;

    [SerializeField] private TextMeshProUGUI statLvTxt;
    [SerializeField] private TextMeshProUGUI statNameTxt;
    [SerializeField] private TextMeshProUGUI statValueTxt;

    [SerializeField] private TextMeshProUGUI costTxt;
    [SerializeField] private Button upBtn;

    private void Start()
    {
        ID = (int)statusType;

        switch (statusType)
        {
            case EStatusType.AttackPower:
                statNameTxt.text = "공격력";
                break;
            case EStatusType.HealthPoint:
                statNameTxt.text = "최대 체력";
                break;
            case EStatusType.AttackRange:
                statNameTxt.text = "공격 범위";
                break;
            case EStatusType.AttackPerSecond:
                statNameTxt.text = "공격 속도";
                break;
            case EStatusType.DPS:
                statNameTxt.text = "초당 공격력";
                break;
            case EStatusType.MoveSpeed:
                statNameTxt.text = "이동 속도";
                break;
            default: throw new ArgumentOutOfRangeException();
        }

        upBtn.onClick.AddListener(OnClickStatUp);

        UpdateUI();
    }

    private void UpdateUI()
    {
        int curStatLv = DataManager.Instance.GameData.statUpLevels[ID];
        statLvTxt.text = curStatLv.ToString();

        float statValue = 0;
        switch (statusType)
        {
            case EStatusType.AttackPower:
                statValue = DataManager.Instance.GameData.playerData.attackPower;
                statValueTxt.text = string.Concat("+", statValue);
                break;
            case EStatusType.HealthPoint:
                statValue = DataManager.Instance.GameData.playerData.healthPoint;
                statValueTxt.text = string.Concat("+", statValue);
                break;
            case EStatusType.AttackRange:
                statValue = DataManager.Instance.GameData.playerData.attackRange;
                statValueTxt.text = string.Concat("+", statValue.ToString("F2"), "/m");
                break;
            case EStatusType.AttackPerSecond:
                statValue = DataManager.Instance.GameData.playerData.attackPerSecond;
                statValueTxt.text = string.Concat("+", statValue.ToString("F2"), "/s");
                break;
            case EStatusType.MoveSpeed:
                statValue = DataManager.Instance.GameData.playerData.moveSpeed;
                statValueTxt.text = string.Concat("+", statValue.ToString("F2"));
                break;
            default: throw new ArgumentOutOfRangeException();
        }

        if (curStatLv >= DataManager.Instance.GameData.MaxStatLevel)
            costTxt.gameObject.SetActive(false);

        int nextCost = Mathf.RoundToInt(curStatLv * 100 * 1.5f);
        costTxt.text = string.Concat("<sprite=0>", nextCost > 0 ? $"{nextCost:#,###}" : "0");

        UIManager.Instance.InvokeStatusUI(statusType, statValue);
        
        if(statusType == EStatusType.AttackPower || statusType == EStatusType.AttackPerSecond)
            UIManager.Instance.InvokeStatusUI(EStatusType.DPS, DataManager.Instance.GameData.playerData.DPS);
    }

    private void OnClickStatUp()
    {
        //AudioManager.Instance.PlaySFX("UIClick");

        int curStatLv = DataManager.Instance.GameData.statUpLevels[ID];
        if (curStatLv >= DataManager.Instance.GameData.MaxStatLevel)
            return;

        int value = Mathf.RoundToInt(curStatLv * 100 * 1.5f);
        if (DataManager.Instance.GameData.gold < value)
            return;

        switch (statusType)
        {
            case EStatusType.AttackPower:
                DataManager.Instance.GameData.playerData.attackPower += 5;
                break;
            case EStatusType.HealthPoint:
                DataManager.Instance.GameData.playerData.healthPoint += 10;
                FindObjectOfType<PlayerController>().UpdateHpUI();
                break;
            case EStatusType.AttackRange:
                DataManager.Instance.GameData.playerData.attackRange =
                    Mathf.Clamp(DataManager.Instance.GameData.playerData.attackRange + 0.01f, 0.0f,
                        DataManager.Instance.GameData.playerData.maxAttackRange);
                break;
            case EStatusType.AttackPerSecond:
                DataManager.Instance.GameData.playerData.attackPerSecond =
                    Mathf.Clamp(DataManager.Instance.GameData.playerData.attackPerSecond + 0.001f, 0.0f,
                        DataManager.Instance.GameData.playerData.maxAttackPerSecond);
                break;
            case EStatusType.MoveSpeed:
                DataManager.Instance.GameData.playerData.moveSpeed =
                    Mathf.Clamp(DataManager.Instance.GameData.playerData.moveSpeed + 0.01f, 0.0f,
                        DataManager.Instance.GameData.playerData.maxMoveSpeed);
                Debug.Log(DataManager.Instance.GameData.playerData.moveSpeed);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        GameManager.Instance.SetGold(-value);
        ++DataManager.Instance.GameData.statUpLevels[ID];

        UpdateUI();
    }
}