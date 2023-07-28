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
    }

    [ContextMenu("Update Equipment Origin Database")]
    private void UpdateEquipmentOriginDatabase()
    {
        weaponOriginDataList.Clear();
        armorOriginDataList.Clear();

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
                (data.EquipmentData.equipType == EEquipType.Weapon ? weaponOriginDataList : armorOriginDataList).Add(data);
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
        mGameData.stageCount = 0;
        
        mGameData.sfx = 50f;
        mGameData.bgm = 50f;
        
        mGameData.stageStr = "1-0";

        mGameData.totalPlayTime = 0.0f;

        mGameData.playerData = new EntityData(playerOriginData.entityData);
        
        mGameData.statUpLevels = new int[] { 1, 1, 1, 1, 1, 1};
        mGameData.skillUpLevels = new int[] { 1, 1, 1 };

        for (int i = 0; i < PlayerSkillDatas.Length; i++)
            mGameData.skillEffectAmounts[i] = PlayerSkillDatas[i].skillImpactAmount;
        
        mGameData.weaponDataList.Clear();
        mGameData.armorDataList.Clear();
        
        UpdateEquipmentOriginDatabase();
    }

    public void LoadGameData()
    {
        string filePath = Application.persistentDataPath + GameDataFileName;

        if (File.Exists(filePath))
        {
            string code = File.ReadAllText(filePath);
            byte[] bytes = System.Convert.FromBase64String(code);
            string FromJsonData = System.Text.Encoding.UTF8.GetString(bytes);
            mGameData = JsonUtility.FromJson<GameData>(FromJsonData);
            
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

        string ToJsonData = JsonUtility.ToJson(mGameData);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(ToJsonData);
        string code = System.Convert.ToBase64String(bytes);
        File.WriteAllText(filePath, code);
    }

    public int GetCurrency(ECurrencyType type)
    {
        int value = 0;
        
        switch (type)
        {
            case ECurrencyType.GD: value = mGameData.gold; break;
            case ECurrencyType.KM: value = mGameData.karma; break;
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

    public void AcquireEquipment(EEquipType equipType, int equipID)
    {
        EquipmentDataSO data = null;
        List<EquipmentData> targetList = null;

        if (equipType == EEquipType.Weapon)
        {
            data = weaponOriginDataList.Find(e => e.EquipmentData.EquipID == equipID);
            targetList = mGameData.weaponDataList;
        }
        else
        {
            data = armorOriginDataList.Find(e => e.EquipmentData.EquipID == equipID);
            targetList = mGameData.armorDataList;
        }

        if (data != null)
        {
            data.EquipmentData.isEquipUnlock = true;
            targetList.Add(new EquipmentData(data));
        }
    }

    private void OnApplicationPause(bool pause) => SaveGameData();
    private void OnApplicationQuit() => SaveGameData();
}