using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    [SerializeField] private GameData mGameData;
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

    public void InitGameData()
    {
        mGameData.sfx = 10f;
        mGameData.bgm = 10f;

        mGameData.karma = 0;
        mGameData.killCount = 0;
        mGameData.deathCount = 0;
        mGameData.waveCount = 0;

        mGameData.bestPlayTime = 0.0f;
        mGameData.totalPlayTime = 0.0f;
        
        for (int i = 0; i < mGameData.statUpLevels.Length; i++)
            mGameData.statUpLevels[i] = 1;
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

    private void OnApplicationPause(bool pause) => SaveGameData();
    private void OnApplicationQuit() => SaveGameData();
}