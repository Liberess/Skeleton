using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private DataManager dataMgr;
    private GameManager gameMgr;

    [HorizontalLine(color: EColor.Red), BoxGroup("# Skill Settings"), SerializeField]
    private Button[] skillBtns;
    [BoxGroup("# Skill Settings"), SerializeField]
    private Image[] skillCoolImgs;
    private TextMeshProUGUI[] skillCoolTimeTxts = new TextMeshProUGUI[3];
    [BoxGroup("# Skill Settings"), SerializeField]
    private Toggle autoUseSkillTog;

    private bool[] isCoolSkills = { false, false, false };
    private float[] skillCoolTimes = { 0.0f, 0.0f, 0.0f };
    private float[] curSkillCoolTimes = { 0.0f, 0.0f, 0.0f };

    private bool isAutoUseSkill = false;
    private bool isBlockUseSkill = false;

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
    
    [HorizontalLine(color: EColor.Green), BoxGroup("# Game UI Settings"), SerializeField]
    private GameObject gameQuitCanvas;

    [BoxGroup("# Game UI Settings"), SerializeField]
    private Button[] menuBtns;
    
    [BoxGroup("# Game UI Settings"), SerializeField]
    private GameObject[] menuPanels;
    
    [HorizontalLine(color: EColor.Blue), BoxGroup("# Offline Reward UI Settings"), SerializeField]
    private GameObject offlineRewardPanel;
    [BoxGroup("# Offline Reward UI Settings"), SerializeField]
    private TextMeshProUGUI offlineTimeTxt;
    [BoxGroup("# Offline Reward UI Settings"), SerializeField]
    private TextMeshProUGUI closeRemainTimeTxt;
    [BoxGroup("# Offline Reward UI Settings"), SerializeField]
    private TextMeshProUGUI offlineRewardAmountTxt;
    
    private GameObject curOpenPanel;
    
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
        gameMgr = GameManager.Instance;

        playerCtrl = FindObjectOfType<PlayerController>();

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

        InitializedUI();
        BindingMenuButton();
        StartCoroutine(SetupEquipmentUICo());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && gameMgr.IsPlaying)
        {
            OpenPanel();

            if (gameQuitCanvas.activeSelf)
            {
                gameQuitCanvas.SetActive(true);
                gameQuitCanvas.transform.GetChild(0).GetComponent<DOTweenAnimation>().DORestart();
            }
        }
    }

    public void InitializedUI()
    {
        gameQuitCanvas.SetActive(false);

        UpdateCurrencyUI().Forget();

        for (int i = 0; i < dataMgr.PlayerSkillDatas.Length; i++)
        {
            skillCoolTimes[i] = dataMgr.PlayerSkillDatas[i].skillCoolTime;
            skillCoolTimeTxts[i] = skillCoolImgs[i].GetComponentInChildren<TextMeshProUGUI>();
            skillCoolImgs[i].gameObject.SetActive(false);
            skillBtns[i].transform.GetChild(0).GetComponent<Image>().sprite = dataMgr.PlayerSkillDatas[i].skillIcon;
        }
    }

    private void BindingMenuButton()
    {
        foreach (var panel in menuPanels)
        {
            panel.SetActive(true);
            panel.SetActive(false);
        }

        for (int i = 0; i < menuBtns.Length; i++)
        {
            int index = i;
            menuBtns[i].onClick.RemoveAllListeners();
            menuBtns[i].onClick.AddListener(() =>
            {
                if (menuPanels[index].activeSelf)
                {
                    OpenPanel();

                    foreach (var doTween in menuBtns[index].GetComponentsInChildren<DOTweenAnimation>())
                        doTween.DORewind();
                }
                else
                {
                    OpenPanel(menuPanels[index]);
                    menuBtns[index].GetComponent<DOTweenAnimation>().DOPlay();
                }
            });
        }
    }

    public async UniTaskVoid UpdateCurrencyUI(float delay = 0.0f)
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
        }
        
        UpdateStatusUIActionList[(int)type]?.Invoke(formatStr);
    }

    public async UniTaskVoid SetOfflineRewardUI(string infoTxt, string amountTxt)
    {
        offlineRewardPanel.gameObject.SetActive(true);
        offlineTimeTxt.text = infoTxt;
        offlineRewardAmountTxt.text = amountTxt;
        offlineRewardPanel.GetComponentInChildren<DOTweenAnimation>().DOPlay();

        float remainTime = 10.0f;
        closeRemainTimeTxt.text = remainTime.ToString("F0");
        
        while (true)
        {
            remainTime -= Time.deltaTime;
            closeRemainTimeTxt.text = remainTime.ToString("F0");

            if (Input.anyKeyDown)
                break;
            
            if (remainTime <= 0)
                break;
            
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        closeRemainTimeTxt.gameObject.SetActive(false);
        offlineTimeTxt.gameObject.SetActive(false);
        offlineRewardPanel.gameObject.SetActive(false);

        DataManager.Instance.GetOfflineReward();
    }

    #region Skill

    public void SetAutoUseSkillToggle(bool isActive)
    {
        isAutoUseSkill = isActive;

        if (isActive)
            StartCoroutine(AutoSkillCo());
        else
            StopCoroutine(AutoSkillCo());
    }

    private IEnumerator AutoSkillCo()
    {
        while (isAutoUseSkill && gameMgr.IsPlaying)
        {
            yield return null;

            for (int i = 0; i < isCoolSkills.Length; i++)
            {
                //현재 쿨타임이 돌지 않는다면, 사용 가능하다는 뜻이다.
                if (!isCoolSkills[i])
                    SetCoolSkill(i);
            }
        }
        
        yield return null;
    }
    
    public void SetCoolSkill(int skillIndex)
    {
        // 동시에 스킬을 사용하지 못하게 막는다.
        if (isBlockUseSkill || !gameMgr.IsPlaying)
            return;
        
        if (skillIndex < 2 && playerCtrl.TargetEntity == null)
            return;
        
        if(skillIndex == 0 && !Utility.IsExistObjectInCamera(playerCtrl.TargetEntity.transform))
            return;

        isBlockUseSkill = true;
        Invoke(nameof(DisableBlockUseSkill), 0.5f);
        skillCoolImgs[skillIndex].gameObject.SetActive(true);
        curSkillCoolTimes[skillIndex] = skillCoolTimes[skillIndex];
        isCoolSkills[skillIndex] = true;
        playerCtrl.UseSkill((ESkillType)skillIndex);
        StartCoroutine(ProgressCoolSkillCo(skillIndex));
    }

    private void DisableBlockUseSkill() => isBlockUseSkill = false;

    /// <summary>
    /// skillIndex의 스킬이 사용되면 쿨타임을 계산하고 이미지를 갱신한다.
    /// </summary>
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
    }

    #endregion
    
    #region Equipment

    private IEnumerator SetupEquipmentUICo()
    {
        yield return new WaitForEndOfFrame();
        
        foreach (var weaponData in DataManager.Instance.GameData.weaponDataList)
        {
            var weaponSlot = Instantiate(equipSlotPrefab).GetComponent<EquipmentSlot>();
            weaponSlot.transform.SetParent(equipWeaponGrid.transform, false);
            weaponSlot.transform.SetAsLastSibling();
            weaponSlot.transform.localScale = weaponSlot.transform.parent.localScale;
            weaponSlot.transform.localPosition = Vector3.zero;
            weaponSlot.SetupSlot(weaponData);
        }
        
        foreach (var armorData in DataManager.Instance.GameData.armorDataList)
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
    
    public void OpenPanel(GameObject panel = null)
    {
        if (curOpenPanel)
        {
            var dotAnims = curOpenPanel.GetComponentsInChildren<DOTweenAnimation>();
            foreach (var dotAnim in dotAnims)
                dotAnim.DORewind();
            curOpenPanel.SetActive(false);
        }
        
        curOpenPanel = panel;
    }
}
