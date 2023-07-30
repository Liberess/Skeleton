using System;
using TMPro;
using UnityEngine;

public class CurrencyText : MonoBehaviour
{
    [SerializeField] private ECurrencyType currencyType;
    public ECurrencyType CurrencyType => currencyType;

    [SerializeField] private TextMeshProUGUI currencyTxt;

    private void Awake()
    {
        if(!currencyTxt)
            currencyTxt = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        UIManager.Instance.UpdateCurrencyUIActionList[(int)currencyType] += UpdateGoodsText;
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.Instance.GameState == EGameState.InGame)
        {
            if (currencyType == ECurrencyType.GD)
                UpdateGoodsText(DataManager.Instance.GameData.gold);
            else
                UpdateGoodsText(DataManager.Instance.GameData.karma);
        }
    }

    private void UpdateGoodsText(int value)
    {
        currencyTxt.text = string.Concat("<sprite=0>", value > 0 ? $"{value:#,###}" : "0");
    }
}
