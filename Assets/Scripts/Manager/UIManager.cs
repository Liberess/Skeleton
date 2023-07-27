using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private DataManager dataMgr;

    [Header("# Skill UI Settings"), Space(5)]
    [SerializeField] private Button[] skillBtns;
    [SerializeField] private Image[] skillCoolImgs;
    private TextMeshProUGUI[] skillCoolTimeTxts = new TextMeshProUGUI[3];
    [SerializeField] private Toggle autoUseSkillTog;
    
    private bool[] isCoolSkills = { false, false, false };

    private float[] skillCoolTimes = { 0.0f, 0.0f, 0.0f };
    private float[] curSkillCoolTimes = { 0.0f, 0.0f, 0.0f };

    private bool isAutoUseSkill = false;
    
    public List<Action<int>> UpdateCurrencyUIActionList = new List<Action<int>>();
    public List<Action<string>> UpdateStatusUIActionList = new List<Action<string>>();
    
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        UpdateCurrencyUIActionList.Clear();
        for (int i = 0; i < Enum.GetValues(typeof(ECurrencyType)).Length; i++)
            UpdateCurrencyUIActionList.Add(null);
        
        UpdateStatusUIActionList.Clear();
        for (int i = 0; i < Enum.GetValues(typeof(EStatusType)).Length; i++)
            UpdateStatusUIActionList.Add(null);
    }

    private void Start()
    {
        dataMgr = DataManager.Instance;

        for (int i = 0; i < dataMgr.PlayerSkillDatas.Length; i++)
        {
            skillCoolTimes[i] = dataMgr.PlayerSkillDatas[i].skillCoolTime;
            skillCoolTimeTxts[i] = skillCoolImgs[i].GetComponentInChildren<TextMeshProUGUI>();
            skillCoolImgs[i].gameObject.SetActive(false);
            skillBtns[i].transform.GetChild(0).GetComponent<Image>().sprite = dataMgr.PlayerSkillDatas[i].skillIcon;
        }
        
        autoUseSkillTog.onValueChanged.AddListener(SetAutoUseSkillToggle);
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

    public void InvokeStatusUI(EStatusType type, float value)
    {
        string formatStr = "";
        
        switch (type)
        {
            case EStatusType.AttackPower:
                formatStr = value.ToString();
                break;
            case EStatusType.HealthPoint:
                formatStr = value.ToString();
                break;
            case EStatusType.AttackRange:
                formatStr = string.Concat(value, "/m");
                break;
            case EStatusType.AttackPerSecond:
                formatStr = string.Concat(value.ToString("F2"), "/s");
                break;
            case EStatusType.DPS:
                formatStr = value.ToString("F2");
                break;
            case EStatusType.MoveSpeed:
                formatStr = value.ToString("F2");
                break;
            default: throw new ArgumentOutOfRangeException();
        }
        
        UpdateStatusUIActionList[(int)type]?.Invoke(formatStr);
    }

    public void SetCoolSkill(int skillIndex)
    {
        skillCoolImgs[skillIndex].gameObject.SetActive(true);
        curSkillCoolTimes[skillIndex] = skillCoolTimes[skillIndex];
        isCoolSkills[skillIndex] = true;
        StartCoroutine(ProgressCoolSkillCo(skillIndex));
    }

    private IEnumerator ProgressCoolSkillCo(int skillIndex)
    {
        do
        {
            yield return null;
            
            curSkillCoolTimes[skillIndex] -= Time.deltaTime;

            if (curSkillCoolTimes[skillIndex] <= 0.0f)
            {
                curSkillCoolTimes[skillIndex] = 0.0f;
                isCoolSkills[skillIndex] = false;
                skillCoolImgs[skillIndex].gameObject.SetActive(false);
            }

            skillCoolTimeTxts[skillIndex].text = Mathf.RoundToInt(curSkillCoolTimes[skillIndex]).ToString();
            skillCoolImgs[skillIndex].fillAmount = curSkillCoolTimes[skillIndex] / skillCoolTimes[skillIndex];

        } while (isCoolSkills[skillIndex]);

        if (isAutoUseSkill)
            SetCoolSkill(skillIndex);
    }

    public void SetAutoUseSkillToggle(bool isActive)
    {
        isAutoUseSkill = isActive;

        for (int i = 0; i < isCoolSkills.Length; i++)
        {
            if(!isCoolSkills[i])
                SetCoolSkill(i);
        }
    }
}
