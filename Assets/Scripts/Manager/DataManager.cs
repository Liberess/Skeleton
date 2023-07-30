using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NaughtyAttributes;
using UnityEngine.Networking;

public class DataManager : MonoBehaviour
{
    private readonly string GameDataFileName = "/GameData.json";
    private readonly string Url = "www.naver.com";

    public static DataManager Instance { get; private set; }
    
    [HorizontalLine(color: EColor.Orange), BoxGroup("# GameData"), SerializeField]
    private GameData mGameData;

    public GameData GameData
    {
        get
        {
            if (mGameData == null)
            {
                LoadGameData();
                SaveGameData();
            }

            return mGameData;
        }
    }
    
    [HorizontalLine(color: EColor.Blue), BoxGroup("# PlayerData"), SerializeField]
    private EntitySO playerOriginData;

    [BoxGroup("# PlayerData"), SerializeField]
    private SkillSO[] playerSkillDatas = new SkillSO[3];

    public SkillSO[] PlayerSkillDatas => playerSkillDatas;

    [HorizontalLine(color: EColor.Green), BoxGroup("# EquipmentData"), SerializeField]
    private List<EquipmentDataSO> weaponOriginDataList;

    public List<EquipmentDataSO> WeaponOriginDataList => weaponOriginDataList;

    [BoxGroup("# EquipmentData"), SerializeField]
    private List<EquipmentDataSO> armorOriginDataList;

    public List<EquipmentDataSO> ArmorOriginDataList => armorOriginDataList;

    private int curGoldRewardAmount = 0;
    private int curKarmaRewardAmount = 0;

    private PlayerController playerCtrl;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        LoadGameData();
    }

    private void Start()
    {
        SaveGameData();

        playerCtrl = FindObjectOfType<PlayerController>();
    }

    [ContextMenu("Update Equipment Origin Database")]
    private void UpdateEquipmentOriginDatabase()
    {
        string[] assetPaths =
        {
            "Scriptable/WeaponData",
            "Scriptable/ArmorData"
        };

        foreach (var path in assetPaths)
        {
            var sos = Resources.LoadAll<EquipmentDataSO>(path);
            foreach (var so in sos)
            {
                so.EquipmentData.SetEquipID(ParseEquipID(so.name));

                List<EquipmentDataSO> dataList = so.EquipmentData.equipType == EEquipType.Weapon
                    ? weaponOriginDataList
                    : armorOriginDataList;

                if (!dataList.Contains(so))
                {
                    dataList.Add(so);

                    if (so.EquipmentData.equipType == EEquipType.Weapon)
                        mGameData.weaponDataList.Add(new EquipmentData(so));
                    else
                        mGameData.armorDataList.Add(new EquipmentData(so));
                }
            }
        }
    }

    private int ParseEquipID(string equipName)
    {
        int indexOfUnderscore = equipName.LastIndexOf('_');
        if (indexOfUnderscore >= 0 && indexOfUnderscore < equipName.Length - 1)
        {
            string equipIDString = equipName.Substring(indexOfUnderscore + 1).Trim();
            if (int.TryParse(equipIDString, out int equipID))
                return equipID;
        }

        return -1;
    }

    #region Init, Save, Load Data

     public void InitGameData()
    {
        mGameData.level = 1;
        mGameData.exp = 0.0f;
        mGameData.needsExp = 5.0f;

        mGameData.isNewGame = true;
        mGameData.gold = 0;
        mGameData.karma = 0;
        mGameData.killCount = 0;
        mGameData.deathCount = 0;
        mGameData.stageCount = 1;

        mGameData.sfx = 50f;
        mGameData.bgm = 50f;

        mGameData.stageStr = "1-1";

        mGameData.totalPlayTime = 0.0f;

        mGameData.playerData = new EntityData(playerOriginData.entityData);

        mGameData.statUpLevels = new int[] { 1, 1, 1, 1, 1, 1 };
        mGameData.skillUpLevels = new int[] { 1, 1, 1 };

        for (int i = 0; i < PlayerSkillDatas.Length; i++)
            mGameData.skillEffectAmounts[i] = PlayerSkillDatas[i].skillImpactAmount;

        weaponOriginDataList = new List<EquipmentDataSO>();
        armorOriginDataList = new List<EquipmentDataSO>();

        mGameData.weaponDataList = new List<EquipmentData>();
        mGameData.armorDataList = new List<EquipmentData>();

        mGameData.curEquipWeapon = null;
        mGameData.curEquipArmor = null;

        mGameData.curEquipWeaponID = -1;
        mGameData.curEquipArmorID = -1;
        
        SaveTime();

        UpdateEquipmentOriginDatabase();
    }

    public void LoadGameData()
    {
        string filePath = Application.persistentDataPath + GameDataFileName;

        if (File.Exists(filePath))
        {
            string code = File.ReadAllText(filePath);
            byte[] bytes = System.Convert.FromBase64String(code);
            string fromJsonData = System.Text.Encoding.UTF8.GetString(bytes);
            mGameData = JsonUtility.FromJson<GameData>(fromJsonData);

            mGameData.isNewGame = false;
        }
        else
        {
            mGameData = new GameData();
            File.Create(Application.persistentDataPath + GameDataFileName);

            InitGameData();
        }
    }

    public void SaveGameData()
    {
        string filePath = Application.persistentDataPath + GameDataFileName;

        string toJsonData = JsonUtility.ToJson(mGameData);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(toJsonData);
        string code = System.Convert.ToBase64String(bytes);
        File.WriteAllText(filePath, code);
    }

    private void SaveTime()
    {
        mGameData.lastLogInTime = DateTime.Now;
        mGameData.lastLogInTimeStr = mGameData.lastLogInTime.ToString();
    }
    
    #endregion

#region 오프라인 보상

public void CalculateOfflineTime() => StartCoroutine(CalculateOfflineTimeCo());

private IEnumerator CalculateOfflineTimeCo()
    {
        UnityWebRequest request = new UnityWebRequest();

        using (request = UnityWebRequest.Get(Url))
        {
            yield return request.SendWebRequest();

            if (request.isNetworkError)
            {
                Debug.LogError(request.error);
            }
            else
            {
                string offTime = mGameData.lastLogInTimeStr;
                DateTime exitTime = Convert.ToDateTime(offTime);
                
                DateTime dateTime = DateTime.Now;
                TimeSpan timeStamp = TimeSpan.FromSeconds((dateTime - exitTime).TotalSeconds);

                if(timeStamp.Minutes >= mGameData.minOfflineTime)
                    ShowOfflineReward(timeStamp);
            }
        }
    }

    public void ShowOfflineReward(TimeSpan timeStamp)
    {
        string dayStr = timeStamp.Days > 0 ? string.Concat(timeStamp.Days, "일") : "";
        string hourStr = timeStamp.Hours > 0 ? string.Concat(timeStamp.Hours, "시간") : "";
        string minutesStr = timeStamp.Minutes > 0 ? string.Concat(timeStamp.Minutes, "분") : "";
        string secondsStr = timeStamp.Seconds >= 0 ? string.Concat(timeStamp.Seconds, "초") : "";

        curKarmaRewardAmount = Mathf.RoundToInt(mGameData.level * timeStamp.Minutes * 0.4f);
        curGoldRewardAmount = Mathf.RoundToInt(mGameData.level * timeStamp.Seconds * 0.2f);

        string amountTxt = $"획득 골드 : {curGoldRewardAmount:n0}" + "\n" + $"획득 카르마 : {curKarmaRewardAmount:n0}";
        string infoTxt = string.Concat("자동 파밍 시간 : ", dayStr, " ", hourStr, " ", minutesStr, " ", secondsStr);
        
        UIManager.Instance.SetOfflineRewardUI(infoTxt, amountTxt).Forget();
    }

    public void GetOfflineReward()
    {
        mGameData.gold += curGoldRewardAmount;
        mGameData.karma += curKarmaRewardAmount;

        UIManager.Instance.UpdateCurrencyUI().Forget();
    }
    
    #endregion

    public int GetStageNumber(EStageNumberType stageNumberType)
    {
        string[] subStr = mGameData.stageStr.Split('-');

        int stageNum = -1;
        switch (stageNumberType)
        {
            case EStageNumberType.All:
                stageNum = mGameData.stageCount;
                break;
            case EStageNumberType.Main:
                stageNum = int.Parse(subStr[0]);
                break;
            case EStageNumberType.Sub:
                stageNum = int.Parse(subStr[1]);
                break;
        }

        return stageNum;
    }

    public int GetCurrency(ECurrencyType type)
    {
        int value = 0;

        switch (type)
        {
            case ECurrencyType.GD:
                value = mGameData.gold;
                break;
            case ECurrencyType.KM:
                value = mGameData.karma;
                break;
        }

        return value;
    }

    public SkillSO GetSkillSO(ESkillType skillType)
    {
        int index = (int)skillType;
        if (index >= 0 && index < PlayerSkillDatas.Length)
            return PlayerSkillDatas[index];
        return null;
    }

    public SkillSO GetSkillSO(int skillIndex)
    {
        if (skillIndex >= 0 && skillIndex < PlayerSkillDatas.Length)
            return PlayerSkillDatas[skillIndex];
        return null;
    }

    public float GetSkillEffectAmount(ESkillType skillType)
    {
        int index = (int)skillType;
        if (index >= 0 && index < mGameData.skillEffectAmounts.Length)
            return mGameData.skillEffectAmounts[index];
        return -1;
    }

    public EquipmentData GetEquipmentData(EEquipType equipType, int equipID)
    {
        EquipmentData data = null;
        if (equipType == EEquipType.Weapon)
            data = mGameData.weaponDataList.Find(e => e.EquipID == equipID);
        else
            data = mGameData.armorDataList.Find(e => e.EquipID == equipID);

        return data;
    }

    public void AcquireEquipment(EEquipType equipType, int equipID)
    {
        var dataList = equipType == EEquipType.Weapon ? mGameData.weaponDataList : mGameData.armorDataList;
        EquipmentData data = dataList.Find(e => e.EquipID == equipID);

        if (data != null && !data.isEquipUnlock)
        {
            data.isEquipUnlock = true;

            if (equipType == EEquipType.Weapon)
            {
                if (mGameData.curEquipWeaponID >= 0)
                    DisarmEquipment(EEquipType.Weapon);

                data.isEquip = true;
                mGameData.curEquipWeapon = data;
                mGameData.curEquipWeaponID = data.EquipID;
                mGameData.playerData.increaseAttackPower = data.impactAmount;
            }
            else
            {
                // 현재 장착 중인 장비가 있다면
                if (mGameData.curEquipArmorID >= 0)
                {
                    // 현재의 최대 체력에서 기존의 장비의 효과를 빼고, 구매한 장비의 효과를 더한 값이
                    // 현재의 체력보다 낮다면, 의도치 않은 결과를 야기하므로 판단한다.
                    if (IsChangeableArmor(data.impactAmount))
                    {
                        DisarmEquipment(EEquipType.Armor);
                        
                        data.isEquip = true;
                        mGameData.curEquipArmor = data;
                        mGameData.curEquipArmorID = data.EquipID;
                        mGameData.playerData.increaseHealthPoint = mGameData.curEquipArmor.impactAmount;
                        playerCtrl.UpdateHpUI();
                    }
                }
                else
                {
                    data.isEquip = true;
                    mGameData.curEquipArmor = data;
                    mGameData.curEquipArmorID = data.EquipID;
                    mGameData.playerData.increaseHealthPoint = mGameData.curEquipArmor.impactAmount;
                    playerCtrl.UpdateHpUI();
                }
            }
        }
    }

    public void DisarmEquipment(EEquipType equipType)
    {
        if (equipType == EEquipType.Weapon)
        {
            GetEquipmentData(equipType, mGameData.curEquipWeapon.EquipID).isEquip = false;
            UIManager.Instance.UpdateWeaponUIAction?.Invoke();
            mGameData.curEquipWeaponID = -1;
            mGameData.curEquipWeapon = null;
        }
        else
        {
            GetEquipmentData(equipType, mGameData.curEquipArmor.EquipID).isEquip = false;
            UIManager.Instance.UpdateArmorUIAction?.Invoke();
            mGameData.curEquipArmorID = -1;
            mGameData.curEquipArmor = null;
        }
    }

    /// <summary>
    /// 현재의 최대 체력에서 기존의 장비의 효과를 빼고, 구매한 장비의 효과를 더한 값이
    /// 현재의 체력보다 낮은지 계산하여 이를 반환한다.
    /// </summary>
    public bool IsChangeableArmor(int nextAmount)
    {
        int preAmount = mGameData.curEquipArmor?.impactAmount ?? 0;
        int tempHp = playerCtrl.originHp - preAmount + nextAmount;
        if (tempHp > playerCtrl.CurrentHp)
            return true;
        return false;
    }

    private void OnApplicationPause(bool pause)
    {
        if(GameManager.Instance.IsPlaying && GameManager.Instance.GameState == EGameState.InGame)
            SaveTime();
        SaveGameData();
    }
    
    private void OnApplicationQuit()
    {    
        Application.CancelQuit();
        SaveTime();
        SaveGameData();
        #if !UNITY_EDITOR
        System.Diagnostics.Process.GetCurrentProcess().Kill();
        #endif
    }
}