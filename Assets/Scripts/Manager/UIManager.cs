using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private DataManager dataMgr;

    [HorizontalLine(color: EColor.Red), BoxGroup("# Skill UI Settings"), SerializeField]
    private Button[] skillBtns;
    [BoxGroup("# Skill UI Settings"), SerializeField]
    private Image[] skillCoolImgs;
    private TextMeshProUGUI[] skillCoolTimeTxts = new TextMeshProUGUI[3];
    [BoxGroup("# Skill UI Settings"), SerializeField]
    private Toggle autoUseSkillTog;
    
    private bool[] isCoolSkills = { false, false, false };

    private float[] skillCoolTimes = { 0.0f, 0.0f, 0.0f };
    private float[] curSkillCoolTimes = { 0.0f, 0.0f, 0.0f };

    private bool isAutoUseSkill = false;

    [HorizontalLine(color: EColor.Orange), BoxGroup("# Equipment UI Settings"), SerializeField]
    private GameObject equipWeaponGrid;
    [BoxGroup("# Equipment UI Settings"), SerializeField]
    private GameObject equipArmorGrid;
    [BoxGroup("# Equipment UI Settings"), SerializeField]
    private GameObject equipSlotPrefab;
    [BoxGroup("# Equipment UI Settings"), SerializeField]
    private Button equipWeaponBtn;
    [BoxGroup("# Equipment UI Settings"), SerializeField]
    private Button equipArmorBtn;

    [HorizontalLine(color: EColor.Yellow), BoxGroup("# Joystick UI Settings"), SerializeField]
    private Toggle dynamicJoystickTog;
    [BoxGroup("# Joystick UI Settings"), SerializeField]
    private Toggle staticJoystickTog;
    
    private PlayerController playerCtrl;
    
    public List<Action<int>> UpdateCurrencyUIActionList = new List<Action<int>>();
    public List<Action<string>> UpdateStatusUIActionList = new List<Action<string>>();
    public Action UpdateWeaponUIAction;
    public Action UpdateArmorUIAction;
    
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

        playerCtrl = FindObjectOfType<PlayerController>();

        for (int i = 0; i < dataMgr.PlayerSkillDatas.Length; i++)
        {
            skillCoolTimes[i] = dataMgr.PlayerSkillDatas[i].skillCoolTime;
            skillCoolTimeTxts[i] = skillCoolImgs[i].GetComponentInChildren<TextMeshProUGUI>();
            skillCoolImgs[i].gameObject.SetActive(false);
            skillBtns[i].transform.GetChild(0).GetComponent<Image>().sprite = dataMgr.PlayerSkillDatas[i].skillIcon;
        }
        
        autoUseSkillTog.onValueChanged.AddListener(SetAutoUseSkillToggle);

        dynamicJoystickTog.onValueChanged.AddListener((active) =>
        {
            dynamicJoystickTog.isOn = active;
            staticJoystickTog.isOn = !active;
            playerCtrl.ControlJoystick(EJoystickType.Dynamic, active);
        });

        staticJoystickTog.onValueChanged.AddListener((active) =>
        {
            dynamicJoystickTog.isOn = !active;
            staticJoystickTog.isOn = active;
            playerCtrl.ControlJoystick(EJoystickType.Static, active);
        });

        StartCoroutine(SetupEquipmentUICo());
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

    #region Skill

    public void SetAutoUseSkillToggle(bool isActive)
    {
        isAutoUseSkill = isActive;

        for (int i = 0; i < isCoolSkills.Length; i++)
        {
            if(!isCoolSkills[i])
                SetCoolSkill(i);
        }
    }

    private IEnumerator AutoSkillCo()
    {
        while (isAutoUseSkill)
        {
            
        }
        yield return null;
    }
    
    public void SetCoolSkill(int skillIndex)
    {
        if (skillIndex < 2 && playerCtrl.TargetEntity == null)
            return;
        
        if(skillIndex == 0 && !Utility.IsExistObjectInCamera(playerCtrl.TargetEntity.transform))
            return;
            
        skillCoolImgs[skillIndex].gameObject.SetActive(true);
        curSkillCoolTimes[skillIndex] = skillCoolTimes[skillIndex];
        isCoolSkills[skillIndex] = true;
        playerCtrl.UseSkill((ESkillType)skillIndex);
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

    #endregion
    
    #region Equipment

    private IEnumerator SetupEquipmentUICo()
    {
        yield return new WaitForEndOfFrame();
        
        foreach (var weaponData in DataManager.Instance.WeaponOriginDataList)
        {
            var weaponSlot = Instantiate(equipSlotPrefab).GetComponent<EquipmentSlot>();
            weaponSlot.transform.SetParent(equipWeaponGrid.transform, false);
            weaponSlot.transform.SetAsLastSibling();
            weaponSlot.transform.localScale = weaponSlot.transform.parent.localScale;
            weaponSlot.transform.localPosition = Vector3.zero;
            weaponSlot.SetupSlot(weaponData);
        }
        
        foreach (var armorData in DataManager.Instance.ArmorOriginDataList)
        {
            var armorSlot = Instantiate(equipSlotPrefab).GetComponent<EquipmentSlot>();
            armorSlot.transform.SetParent(equipArmorGrid.transform, false);
            armorSlot.transform.SetAsLastSibling();
            armorSlot.transform.localScale = armorSlot.transform.parent.localScale;
            armorSlot.transform.localPosition = Vector3.zero;
            armorSlot.SetupSlot(armorData);
        }
        
        equipWeaponBtn.onClick.AddListener(() =>
        {
            equipWeaponBtn.image.color = Color.yellow;
            equipArmorBtn.image.color = Color.white;
        });
        
        equipWeaponBtn.image.color = Color.yellow;
        
        equipArmorBtn.onClick.AddListener(() =>
        {
            equipArmorBtn.image.color = Color.yellow;
            equipWeaponBtn.image.color = Color.white;
        });

        yield return null;
    }

    public void UpdatePlayerHpUI() => playerCtrl.UpdateHpUI();

    #endregion
}
