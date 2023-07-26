using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameObject mContainer;

    private static GameManager mInstance;
    public static GameManager Instance
    {
        get
        {
            if (mInstance == null)
            {
                mContainer = new GameObject();
                mContainer.name = "GameManager";
                mInstance = mContainer.AddComponent(typeof(GameManager)) as GameManager;
            }

            return mInstance;
        }
    }
    #endregion
    
    private UIManager uiMgr;
    private DataManager dataMgr;

    [SerializeField] private GameObject gameOverCanvas;
    
    [SerializeField] private TextMeshProUGUI stageTxt;
    [SerializeField] private TextMeshProUGUI goldTxt;
    [SerializeField] private TextMeshProUGUI karmaTxt;
    [SerializeField] private TextMeshProUGUI shopKarmaTxt;
    [SerializeField] private TextMeshProUGUI remainTxt;

    public UnityAction NextWaveAction;
    public UnityAction GameOverAction;

    [SerializeField] private bool isPlaying = true;
    public bool IsPlaying => isPlaying;

    public int Gold
    {
        get => dataMgr.GameData.gold;
        set
        {
            dataMgr.GameData.gold = Mathf.Clamp(value, 0, dataMgr.GameData.MaxGoodsAmount);
            uiMgr.InvokeCurrencyUI(ECurrencyType.GD, dataMgr.GameData.gold);
        }
    }

    public int Karma
    {
        get => dataMgr.GameData.karma;
        set
        {
            dataMgr.GameData.karma = Mathf.Clamp(value, 0, dataMgr.GameData.MaxGoodsAmount);
            uiMgr.InvokeCurrencyUI(ECurrencyType.KM, dataMgr.GameData.karma);
        }
    }

    public int currentkillCount = 0;
    public float currentPlayTime = 0;

    private DateTime startTime;

    private void Awake()
    {
        if (mInstance == null)
            mInstance = this;
        else if (mInstance != this)
            Destroy(gameObject);
        
        GameOverAction += GameOver;
        NextWaveAction += NextStage;
    }

    private void Start()
    {
        uiMgr = UIManager.Instance;
        dataMgr = DataManager.Instance;

        Gold = 50;

        startTime = DateTime.Now;
        
        UpdateGameUI();
    
        StartCoroutine(InvokeNextWaveCo(1.0f));

        //AudioManager.Instance.PlayBGM(EBGMName.InGame);
    }

    public void GetGold(int value) => Gold += value;
    public void GetKarma(int value) => Karma += value;

    private IEnumerator InvokeNextWaveCo(float delay = 0.0f)
    {
        yield return new WaitForSeconds(delay);

        NextWaveAction?.Invoke();
    }

    public void NextStage()
    {
        ++dataMgr.GameData.stageCount;

        string[] subStr = dataMgr.GameData.stageStr.Split('-');
        int mainStageNum = int.Parse(subStr[0]);
        int subStageNum = int.Parse(subStr[1]);

        if (subStageNum < 10)
        {
            ++subStageNum;
        }
        else
        {
            ++mainStageNum;
            subStageNum = 1;
            BossStage();
        }

        dataMgr.GameData.stageStr = string.Concat(mainStageNum, '-', subStageNum);
        stageTxt.text = dataMgr.GameData.stageStr;
    }

    private void BossStage()
    {
        
    }

    private void UpdateGameUI()
    {
        stageTxt.text = dataMgr.GameData.stageStr;
        /*goldTxt.text = string.Concat("<sprite=0>", 
            dataMgr.GameData.gold > 0 ? $"{dataMgr.GameData.gold:#,###}" : "0");
        karmaTxt.text = string.Concat("<sprite=0>", 
            dataMgr.GameData.karma > 0 ? $"{dataMgr.GameData.karma:#,###}" : "0");*/
    }

    public void UpdateRemainMonsterUI(int count)
    {
        remainTxt.text = count.ToString();
    }

    private void GameOver()
    {
        DateTime endTime = DateTime.Now;
        TimeSpan timeDif = endTime - startTime;

        currentPlayTime = (float)timeDif.TotalSeconds;

        dataMgr.GameData.totalPlayTime += currentPlayTime;
        dataMgr.GameData.karma += currentkillCount;
        
        ++dataMgr.GameData.deathCount;
        dataMgr.GameData.killCount += currentkillCount;

        gameOverCanvas.SetActive(true);
    }
}
