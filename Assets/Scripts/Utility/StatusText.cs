using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusText : MonoBehaviour
{
    [SerializeField] private EStatusType statusType;
    public EStatusType StatusType => statusType;

    [SerializeField] private TextMeshProUGUI statusTxt;

    private void Awake()
    {
        if(!statusTxt)
            statusTxt = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        UIManager.Instance.UpdateStatusUIActionList[(int)statusType] += UpdateStatusText;
    }

    private void UpdateStatusText(string str)
    {
        statusTxt.text = str;
    }
}
