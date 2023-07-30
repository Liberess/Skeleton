using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using NaughtyAttributes;

public class DataManager : MonoBehaviour
{
    private readonly string GameDataFileName = "/GameData.json";

    #region Singleton

    private static GameObject mContainer;

    private static DataManager mInstance;

    public static DataManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                mContainer = new GameObject();
                mContainer.name = "DataManager";
                mInstance = mContainer.AddComponent(typeof(DataManager)) as DataManager;
            }

            return mInstance;
        }
    }

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

    #endregion

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

    private PlayerController playerCtrl;

    private void Awake()
    {
        if (mInstance == null)
        {
            mInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (mInstance != this)
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
            "Assets/Scriptables/WeaponData",
            "Assets/Scriptables/ArmorData"
        };

        var guids = AssetDatabase.FindAssets("t:EquipmentDataSO", assetPaths);
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EquipmentDataSO data = AssetDatabase.LoadAssetAtPath<EquipmentDataSO>(path);

            if (data != null)
            {
                data.EquipmentData.SetEquipID(ParseEquipID(data.name));

                List<EquipmentDataSO> dataList = data.EquipmentData.equipType == EEquipType.Weapon
                    ? weaponOriginDataList
                    : armorOriginDataList;

                if (!dataList.Contains(data))
                {
                    dataList.Add(data);

                    if (data.EquipmentData.equipType == EEquipType.Weapon)
                        mGameData.weaponDataList.Add(new EquipmentData(data));
                    else
                        mGameData.armorDataList.Add(new EquipmentData(data));
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

    public void InitGameData()
    {
        mGameData.level = 1;
        mGameData.exp = 0.0f;
        mGameData.needsExp = 5.0f;

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

        weaponOriginDataList.Clear();
        armorOriginDataList.Clear();

        mGameData.weaponDataList.Clear();
        mGameData.armorDataList.Clear();

        mGameData.curEquipWeapon = null;
        mGameData.curEquipArmor = null;

        mGameData.curEquipWeaponID = -1;
        mGameData.curEquipArmorID = -1;

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

            InitGameData();
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
                        DisarmEquipment(EEquipType.Armor);
                }
                
                data.isEquip = true;
                mGameData.curEquipArmor = data;
                mGameData.curEquipArmorID = data.EquipID;
                mGameData.playerData.increaseHealthPoint = mGameData.curEquipArmor.impactAmount;
                playerCtrl.UpdateHpUI();
            }
        }
    }

    public void DisarmEquipment(EEquipType equipType)
    {
        if (equipType == EEquipType.Weapon)
        {
            mGameData.curEquipWeapon.isEquip = false;
            UIManager.Instance.UpdateWeaponUIAction?.Invoke();
            mGameData.curEquipWeaponID = -1;
            mGameData.curEquipWeapon = null;
        }
        else
        {
            mGameData.curEquipArmor.isEquip = false;
            UIManager.Instance.UpdateArmorUIAction?.Invoke();
            mGameData.curEquipArmorID = -1;
            mGameData.playerData.increaseHealthPoint = mGameData.curEquipArmor.impactAmount;
            playerCtrl.UpdateHpUI();
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

    private void OnApplicationPause(bool pause) => SaveGameData();
    private void OnApplicationQuit() => SaveGameData();
}