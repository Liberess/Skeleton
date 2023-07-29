using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
    private PlayerController playerCtrl;

    [SerializeField] private GameObject gameCanvas;
    [SerializeField] private GameObject gameOverCanvas;

    [SerializeField] private TextMeshProUGUI stageTxt;
    [SerializeField] private TextMeshProUGUI remainTxt;
    [SerializeField] private Image remainFillImg;
    [SerializeField] private Slider remainBar;

    [SerializeField] private TextMeshProUGUI lvTxt;
    [SerializeField] private Slider expSlider;

    public UnityAction NextWaveAction;
    public UnityAction GameOverAction;

    [SerializeField] private bool isPlaying = true;
    public bool IsPlaying => isPlaying;

    public float Exp
    {
        get => dataMgr.GameData.exp;
        set
        {
            dataMgr.GameData.exp = Mathf.Clamp(value, 0.0f, 100.0f);

            if (dataMgr.GameData.exp >= dataMgr.GameData.needsExp)
            {
                AudioManager.Instance.PlaySFX(ESFXName.LevelUp);
                
                ++dataMgr.GameData.level;
                dataMgr.GameData.exp = Mathf.Clamp(dataMgr.GameData.exp - dataMgr.GameData.needsExp, 0.0f,
                    dataMgr.GameData.needsExp);

                lvTxt.text = dataMgr.GameData.level.ToString();

                // 맨 뒤의 계수는 10만 테이블당 1을 뜻함
                dataMgr.GameData.needsExp = Mathf.Pow(((dataMgr.GameData.level - 1) * 50 / 49), 2.5f) * 10;
            }

            expSlider.value = dataMgr.GameData.exp / dataMgr.GameData.needsExp;
        }
    }

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

    public int curkillCount = 0;
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

        playerCtrl = FindObjectOfType<PlayerController>();

        expSlider.maxValue = 1.0f;
        remainBar.maxValue = 1.0f;

        startTime = DateTime.Now;

        UpdateGameUI();

        StartCoroutine(InvokeNextWaveCo(1.0f));

        AudioManager.Instance.PlayBGM(EBGMName.InGame);
    }

    public void SetGold(int value) => Gold += value;
    public void SetKarma(int value) => Karma += value;

    public IEnumerator InvokeNextWaveCo(float delay = 0.0f)
    {
        curkillCount = 0;
        
        yield return new WaitForSeconds(delay);

        NextWaveAction?.Invoke();
    }

    public void NextStage()
    {
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
        
        ++dataMgr.GameData.stageCount;
        
        dataMgr.GameData.karma += mainStageNum;

        dataMgr.GameData.stageStr = string.Concat(mainStageNum, '-', subStageNum);
        stageTxt.text = dataMgr.GameData.stageStr;
        remainFillImg.fillAmount = 1.0f;
    }

    private void BossStage()
    {
        
    }

    private void UpdateGameUI()
    {
        stageTxt.text = dataMgr.GameData.stageStr;
        lvTxt.text = dataMgr.GameData.level.ToString();
        expSlider.value = dataMgr.GameData.exp / dataMgr.GameData.needsExp;
    }

    public void UpdateRemainMonsterUI(int count)
    {
        remainTxt.text = count.ToString();
        remainFillImg.fillAmount = ((float)count / MonsterManager.Instance.StageSpawnCount);
        remainBar.value = (float)curkillCount / MonsterManager.Instance.StageSpawnCount;
    }

    private void GameOver()
    {
        isPlaying = false;
        
        DateTime endTime = DateTime.Now;
        TimeSpan timeDif = endTime - startTime;
        currentPlayTime = (float)timeDif.TotalSeconds;
        dataMgr.GameData.totalPlayTime += currentPlayTime;

        ++dataMgr.GameData.deathCount;

        gameCanvas.SetActive(false);
        gameOverCanvas.SetActive(true);
        
        AudioManager.Instance.StopBGM();
    }

    public void RetryGame()
    {
        //scenema
        
        isPlaying = true;
        
        curkillCount = 0;
        remainFillImg.fillAmount = 1.0f;

        UpdateGameUI();
        UpdateRemainMonsterUI(0);

        playerCtrl.gameObject.SetActive(false);
        playerCtrl.gameObject.SetActive(true);
  
        MonsterManager.Instance.Spawn();
        
        gameCanvas.SetActive(true);
        gameOverCanvas.SetActive(false);
        
        AudioManager.Instance.PlayBGM(EBGMName.InGame);
    }
    
    public void QuitGame()
    {
        if (isPlaying)
        {
            isPlaying = false;
        
            DateTime endTime = DateTime.Now;
            TimeSpan timeDif = endTime - startTime;
            currentPlayTime = (float)timeDif.TotalSeconds;
            dataMgr.GameData.totalPlayTime += currentPlayTime;
        }
        
        Application.Quit();
    }
}