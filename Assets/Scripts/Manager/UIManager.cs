using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private DataManager dataMgr;
    
    public List<Action<int>> UpdateCurrencyUIActionList = new List<Action<int>>();
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        UpdateCurrencyUIActionList.Clear();
        for (int i = 0; i < Enum.GetValues(typeof(ECurrencyType)).Length; i++)
            UpdateCurrencyUIActionList.Add(null);
    }

    private void Start()
    {
        dataMgr = DataManager.Instance;
    }

    public async UniTaskVoid UpdateCurrencyUI(float delay)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        foreach (ECurrencyType type in Enum.GetValues(typeof(ECurrencyType)))
            InvokeCurrencyUI(type, dataMgr.GetCurrency(type));
    }

    public void InvokeCurrencyUI(ECurrencyType type, int amount)
    {
        UpdateCurrencyUIActionList[(int)type]?.Invoke(amount);
    }
}
